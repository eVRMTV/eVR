using BerTlv;
using eVR.Reader.PCSC;
using System.Security.Cryptography;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF SecurityInfos on the card.
    ///
    /// EF SecurityInfos shall be present when ECC is used for Active Authentication, 
    /// this datagroup shall be absent in case Active Authentication is based on RSA
    /// </summary>
    public class SecurityInfos
        : IElementaryFile
    {
        #region Properties
        public string Name => "SecurityInfos";
        public byte[] Identifier => new byte[] { 0x00, 0x0E };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];
        public Oid? ActiveAuthenticationOid { get; set; }
        public byte[] Version { get; set; } = [];
        public Oid? HashAlgorithmOid { get; set; }        
        #endregion

        #region Constructor
        public SecurityInfos()
        {
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }
        #endregion

        #region Interface - IElementaryFile

        public Task Construct()
        {
            return Task.Run(() =>
            {
                if (ParsedData.Any())
                {
                    ActiveAuthenticationOid = Helper.ConvertOid(this.ParsedData.Tag("6E|31|30|06", 0)!.Value);
                    Version = this.ParsedData.Tag("6E|31|30|02")!.Value;
                    HashAlgorithmOid = Helper.ConvertOid(this.ParsedData.Tag("6E|31|30|06", 1)!.Value);
                }
            });
        }
        #endregion
    }
}
