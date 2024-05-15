using eVR.Reader.Data;
using eVR.Reader.PCSC;
using System.Diagnostics.CodeAnalysis;

namespace eVR.Reader
{
    /// <summary>
    /// Class containing all data from an eVR card.
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    [SuppressMessage("csharpsquid", "S101", Justification = "The spelling of eVR does not match pascal case naming rules")]
    public class eVRCardState(IParserTlv parserTlv)
#pragma warning restore IDE1006 // Naming Styles
    {
        #region Properties
        public byte[]? ATR { get; set; }
        public bool MissingCSCA { get; set; }
        public CardGeneration? Generation
        {
            get
            {
                return Convert.ToHexString(ATR ?? []) switch
                {
                    eVRDefinitions.AtrGeneration1 => CardGeneration.Generation1,
                    eVRDefinitions.AtrGeneration2 => CardGeneration.Generation2,
                    eVRDefinitions.AtrGeneration3 => CardGeneration.Generation3,
                    _ => null
                };
            }
        }
        public AA AA { get; set; } = new();
        public SecurityInfos SecurityInfos { get; set; } = new();
        public C_IA_A_DS C_IA_A_DS { get; set; } = new();
        public C_IA_B_DS C_IA_B_DS { get; set; } = new();
        public RegistrationA RegistrationA { get; set; } = new();
        public RegistrationB RegistrationB { get; set; } = new();
        public RegistrationC RegistrationC { get; set; } = new();
        public SignatureA SignatureA { get; set; } = new();
        public SignatureB SignatureB { get; set; } = new();
        public SOd SOd { get; set; } = new(parserTlv);

        public IEnumerable<IElementaryFile> ElementaryFiles =>
        [
            AA,
            SecurityInfos,
            C_IA_A_DS,
            C_IA_B_DS,
            RegistrationA,
            RegistrationB,
            RegistrationC,
            SignatureA,
            SignatureB,
            SOd,
        ];

        #endregion
    }
}
