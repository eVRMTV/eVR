using eVR.Reader.Demo.Models;

namespace eVR.Reader.Demo.ViewModels
{
    /// <summary>
    /// ViewModel that represents the back of a new eVR card (since 2024)
    /// </summary>
    /// <param name="printedData">All the printed data on the card</param>
    public class CardBackNewViewModel(CardPrintedData printedData)
        : IViewModel
    {
        #region Properties

        public string Name => "Card front";
        public CardPrintedData PrintedData => printedData;

        #endregion
    }
}
