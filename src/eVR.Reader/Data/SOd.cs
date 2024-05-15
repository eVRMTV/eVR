using BerTlv;
using eVR.Reader.PCSC;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF SOD on the card.
    /// 
    /// EF SOD contains hashes over 
    /// - EF AA
    /// - EF SecurityInfos
    /// - EF Registration_A
    /// - EF Registration_B
    /// - EF Registration_C
    /// It also contains a signature over these hashes
    /// </summary>
    public class SOd
        : IElementaryFile
    {
        #region Dependencies

        private readonly IParserTlv _parser;

        #endregion

        #region Properties

        public string Name => "SOd";
        public byte[] Identifier => new byte[] { 0x00, 0x1D };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];

        public X509Certificate2? DSCertificate { get; set; }
        public byte[] Signature { get; set; } = [];
        public byte[] Algorithm { get; set; } = [];
        public byte[] RDWIdsSecurityObject { get; set; } = [];
        public byte[] SignedAttributes { get; set; } = [];
        public byte[] HashAlgorithm { get; set; } = [];
        public Oid? DigestAlgorithm { get; private set; }
        public byte[] AttributeValue { get; private set; } = [];
        public IDictionary<string, byte[]> DatagroupHashValues { get; private set; }

        #endregion

        #region Constructor

        public SOd(IParserTlv parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            DatagroupHashValues = new Dictionary<string, byte[]>();
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }

        #endregion

        #region Interface - IElementaryFile

        public async Task Construct()
        {
            var certificate = this.ParsedData.Tag("30|A0|30|A0")!.Value;
            this.DSCertificate = new X509Certificate2(certificate);

            this.Signature = this.ParsedData.Tag("30|A0|30|31|30|04")!.Value;
            this.Algorithm = this.ParsedData.Tag("30|A0|30|31|30|30|06")!.Value;
            this.RDWIdsSecurityObject = this.ParsedData.Tag("30|A0|30|30|A0|04")!.Value;
            this.SignedAttributes = this.ParsedData.Tag("30|A0|30|31|30|A0")!.Value;
            this.HashAlgorithm = this.ParsedData.Tag("30|A0|30|31|30|30|06")!.Value;

            this.DigestAlgorithm = Helper.ConvertOid(this.HashAlgorithm);
            this.AttributeValue = this.ParsedData.Tag("30|A0|30|31|30|A0|30|31|04")!.Value;

            // extract the hashed data groups from the eContent (RDWIdsSecurityObject)
            var parsedDataGroups = (await _parser.Parse(RDWIdsSecurityObject)).Tags("30|30|30|04").ToArray();
            DatagroupHashValues = new Dictionary<string, byte[]>();
            for (int i = 0; i < parsedDataGroups.Length; i += 2)            
            {   
                DatagroupHashValues.Add(Helper.ToHexWithSpaces(parsedDataGroups[i].Value), parsedDataGroups[i + 1].Value);
            }
        }

        #endregion
    }
}
