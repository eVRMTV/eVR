namespace eVR.Reader.Demo.ViewModels
{
    /// <summary>
    /// Class representing CVO data (xml) on the card.
    /// </summary>
    /// <param name="cvo">An xml string with CVO data</param>
    /// <param name="number">The number of this CVO data</param>
    public class CvoDataViewModel(string cvo, int number)
                : IViewModel
    {
        #region Properties
        public string Name { get; } = "CVO " + number;
        public string Cvo { get; } = cvo;

        #endregion
    }
}
