using BerTlv;
using eVR.Reader.PCSC;
using System.Security.Cryptography;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF Signature_A on the card.
    /// 
    /// EF Signature_A contains a signature over the data in EF Registration_A.
    /// </summary>
    public class SignatureA
       : IElementaryFile
    {
        #region Properties

        public string Name => "Signature A";
        public byte[] Identifier => new byte[] { 0xE0, 0x01 };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];

        public byte[] Signature { get; set; } = [];
        public Oid? SignatureAlgorithmOid { get; set; }

        #endregion

        #region Constructor

        public SignatureA()
        {
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }

        #endregion

        #region Interface - IElementaryFile

        public Task Construct()
        {
            return Task.Run(() =>
            {
                this.Signature = this.ParsedData.Tag("30|03")!.Value;
                // extract the oid value from the signature
                var oidTLV = this.ParsedData.Tag("30|30|06")!.Value;
                SignatureAlgorithmOid = Helper.ConvertOid(oidTLV);
            });
        }

        #endregion
    }
}
