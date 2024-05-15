using eVR.Reader.Demo.Models;

namespace eVR.Reader.Demo.ViewModels
{
    /// <summary>
    /// ViewModel that represents the back of an old eVR card (befor 2024)
    /// </summary>
    /// <param name="printedData">All the printed data on the card</param>
    public class CardBackOldViewModel(CardPrintedData printedData) 
        : IViewModel
    {
        #region Properties

        public string Name => "Card front";
        public CardPrintedData PrintedData => printedData;

        #endregion
    }
}
