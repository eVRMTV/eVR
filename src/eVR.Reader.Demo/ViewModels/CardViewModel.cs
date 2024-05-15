using eVR.Reader.Demo.Models;

namespace eVR.Reader.Demo.ViewModels
{
    /// <summary>
    /// ViewModel representing all printed data on the front and back of an eVR card
    /// </summary>
    /// <param name="cardReadResult">The data read from the card</param>
    public class CardViewModel(CardReadResult cardReadResult)
        : IViewModel
    {
        #region Properties

        public string Name => "eVR";

        /// <summary>
        /// A ViewModel representing the front of the card
        /// </summary>
        public IViewModel CardFront { get; } =
            cardReadResult.CardState!.Generation != CardGeneration.Generation3 ?
            new CardFrontOldViewModel(cardReadResult.PrintedData!) :
            new CardFrontNewViewModel(cardReadResult.PrintedData!);

        /// <summary>
        /// A ViewModel representing the back of the card.
        /// </summary>
        public IViewModel CardBack { get; } =
            cardReadResult.CardState!.Generation != CardGeneration.Generation3 ?
            new CardBackOldViewModel(cardReadResult.PrintedData!) :
            new CardBackNewViewModel(cardReadResult.PrintedData!);

        /// <summary>
        /// An indication whether the card should be considered as an invalid card.
        /// </summary>
        public bool Error => !cardReadResult.Valid && !cardReadResult.CardState!.MissingCSCA;

        /// <summary>
        /// An error message to be shown to the user.
        /// </summary>
        public string? ErrorMessage
        {
            get
            {
                if (cardReadResult.Valid)
                {
                    return null;
                }
                if (cardReadResult.CardState?.MissingCSCA == true)
                {
                    return "The integrity of the data cannot be verified, please make sure that all published certificates are properly configured\r\nhttps://www.rdw.nl/zakelijk/paginas/csca-certificaten-en-de-certificate-revocation-list-downloaden";
                }
                return "Something is wrong with the card. Please contact RDW!";
            }
        }

        #endregion
    }
}
