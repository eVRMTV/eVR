using BerTlv;
using eVR.Reader.PCSC;
using System.Security.Cryptography;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF AA on the card.
    /// 
    /// EF AA contains the public key info for active authentication
    /// </summary>
    public class AA
        : IElementaryFile
    {
        #region Properties

        public string Name => "AA";
        public byte[] Identifier => new byte[] { 0x00, 0x0D };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];
        public byte[] ActiveAuthenticationPublicKeyInfo { get; set; } = [];
        public Oid? KeyType { get; set; }

        #endregion

        #region Constructor

        public AA()
        {
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }

        #endregion

        #region Interface - IElementaryFile

        public async Task Construct()
        {
            await Task.Run(() =>
            {
                this.ActiveAuthenticationPublicKeyInfo = this.ParsedData.Tag("6F")!.Value;
                this.KeyType = Helper.ConvertOid(this.ParsedData.Tag("6F|30|30|06")!.Value);
            });
        }

        #endregion
    }
}
