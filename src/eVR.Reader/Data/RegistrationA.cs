using BerTlv;
using eVR.Reader.PCSC;
using System.Text;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF Registration_A on the card.
    /// 
    /// EF Registration_A contains data concerning the Vehicle Registration.
    /// </summary>
    public class RegistrationA
        : IRegistrationFile
    {
        #region Properties

        public string Name => "Registration A";
        public byte[] Identifier => new byte[] { 0xD0, 0x01 };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];
        public Encoding? CharacterSetEncoding { get; set; }

        #endregion

        #region Constructor

        public RegistrationA()
        {
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }

        #endregion

        #region Interface - IElementaryFile

        public Task Construct()
        {
            return Task.Run(() =>
            {
                this.CharacterSetEncoding = Helper.GetEncoding(this.ParsedData.Tag("71|9F37")!.Value);               
            });
        }

        #endregion
    }
}
