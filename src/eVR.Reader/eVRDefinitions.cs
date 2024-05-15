using System.Diagnostics.CodeAnalysis;

namespace eVR.Reader
{
    /// <summary>
    /// Class with some eVR specific constants
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    [SuppressMessage("csharpsquid", "S101", Justification = "The spelling of eVR does not match pascal case naming rules")]
    public static class eVRDefinitions
#pragma warning restore IDE1006 // Naming Styles
    {
        public static byte[] AID { get; } = Convert.FromHexString(AIDstring);

        public const string AIDstring = "A0000004564556522D3031";
        public const string AtrGeneration1 = "3BD218008131FE450101C1";
        public const string AtrGeneration2 = "3BD296FF81B1FE451F870102AB";
        public const string AtrGeneration3 = "3BD296008131FE4301034B";
    }
}
