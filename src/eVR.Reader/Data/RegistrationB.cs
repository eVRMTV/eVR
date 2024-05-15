using BerTlv;
using eVR.Reader.PCSC;
using System.Text;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF Registration_B on the card.
    /// 
    /// EF Registration_B contains data concerning the Vehicle Registration.
    /// </summary>
    public class RegistrationB
        : IRegistrationFile
    {
        #region Properties

        public string Name => "Registration B";
        public byte[] Identifier => new byte[] { 0xD0, 0x11 };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];
        public Encoding? CharacterSetEncoding { get; set; }
        public string VehicleCategory { get; private set; } = string.Empty;
        #endregion

        #region Constructor

        public RegistrationB()
        {
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }

        #endregion

        #region Interface - IElementaryFile

        public Task Construct()
        {
            return Task.Run(() =>
            {
                this.CharacterSetEncoding = Encoding.GetEncoding("iso-8859-1");
                this.VehicleCategory = Helper.DecodeString(this.ParsedData.Tag("72|98")!.Value, this.CharacterSetEncoding);
            });
        }

        #endregion
    }
}
