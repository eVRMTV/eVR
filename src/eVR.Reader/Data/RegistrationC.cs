using BerTlv;
using eVR.Reader.PCSC;
using System.Text;
using System.Xml;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF Registration_C on the card.
    /// 
    /// EF Registration_C contains data concerning the Vehicle Registration.
    /// </summary>
    public class RegistrationC
        : IRegistrationFile
    {
        #region Properties

        public string Name => "Registration C";
        public byte[] Identifier => new byte[] { 0xD0, 0x21 };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];
        public Encoding? CharacterSetEncoding { get; set; }
        public string[] CVOs { get; private set; } = [];
        public IList<string> VIN { get; private set; }

        #endregion

        #region Constructor

        public RegistrationC()
        {
            VIN = new List<string>();
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }

        #endregion

        #region Interface - IElementaryFile

        public Task Construct()
        {
            return Task.Run(() =>
            {
                this.CharacterSetEncoding = Encoding.GetEncoding("iso-8859-1");              
                var zippedXMLsTLV = ParsedData.Tag("BF8700|BF8703|9F8705")?.Value;
                int i = 0;
                var cvos = new List<string>();
                VIN = new List<string>();

                while (zippedXMLsTLV != null && zippedXMLsTLV.Length != 0)
                {
                    var unzipped = Helper.GUnzip2(zippedXMLsTLV);
                    if (!string.IsNullOrEmpty(unzipped))
                    {
                        cvos.Add(unzipped);
                        var doc = new XmlDocument();
                        doc.LoadXml(unzipped);

                        var xnList = doc.SelectNodes("InitialVehicleInformation/Body/CocDataGroup/VehicleIdentificationNumber");
                        if (xnList != null && xnList.Count == 1)
                        {
                            this.VIN.Add(xnList[0]!.InnerXml);
                        }
                        i++;
                    }
                    zippedXMLsTLV = this.ParsedData.Tag("BF8700|BF8703|9F8705", i)?.Value;

                }
                this.CVOs = [.. cvos];
            });
        }

        #endregion
    }
}
