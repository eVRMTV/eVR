using BerTlv;
using eVR.Reader.PCSC;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace eVR.Reader.Data
{
    /// <summary>
    /// This class contains the content of file EF C.IA_B.DS on the card.
    /// 
    /// EF C.IA_B.DS contains the DS certificate used to set the signature 
    /// EF Signature_B over the data in EF Registration_B. 
    /// </summary>
    [SuppressMessage("csharpsquid", "S101", Justification = "The spelling of this elementary file does not match pascal case naming rules")]
    public class C_IA_B_DS
        : IElementaryFile
    {
        #region Properties

        public string Name => "CIA B DS";
        public byte[] Identifier => new byte[] { 0xC0, 0x11 };
        public string FileIDHexValue { get; private set; }
        public byte[] RawData { get; set; } = [];
        public bool NeedsParsing => true;
        public IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; } = [];
        public X509Certificate2? DSCertificate { get; set; }

        #endregion

        #region Constructor

        public C_IA_B_DS()
        {
            FileIDHexValue = Helper.ToHexWithSpaces(Identifier);
        }

        #endregion

        #region Interface - IElementaryFile

        public Task Construct()
        {
            return Task.Run(() =>
            {
                this.DSCertificate = new X509Certificate2(this.RawData);
            });
        }

        #endregion
    }
}
