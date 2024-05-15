using BerTlv;
using eVR.Reader.PCSC;
using System.Security.Cryptography;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF Signature_B on the card.
    /// 
    /// EF Signature_B contains a signature over the data in EF Registration_B.
    /// </summary>
    public class SignatureB
        : IElementaryFile
    {
        #region Properties

        public string Name => "Signature B";
        public byte[] Identifier => new byte[] { 0xE0, 0x11 };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];

        public byte[] Signature { get; set; } = [];
        public Oid? SignatureAlgorithmOid { get; set; }

        #endregion

        #region Constructor

        public SignatureB()
        {
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }

        #endregion

        #region Interface - IElementaryFile

        public async Task Construct()
        {
            await Task.Run(() =>
            {
                Signature = this.ParsedData.Tag("30|03")!.Value;
                // extract the oid value from the signature
                var oidTLV = this.ParsedData.Tag("30|30|06")!.Value;
                SignatureAlgorithmOid = Helper.ConvertOid(oidTLV);
            });
        }

        #endregion
    }
}
