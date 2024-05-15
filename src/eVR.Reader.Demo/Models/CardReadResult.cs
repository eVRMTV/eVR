namespace eVR.Reader.Demo.Models
{
    /// <summary>
    /// Class containing the data and validation result of an eVR card
    /// </summary>
    public class CardReadResult
    {
        #region Properties
        public eVRCardState? CardState { get; set; }
        public bool Valid { get; set; }
        public CardPrintedData? PrintedData { get; set; }
        #endregion
    }
}
