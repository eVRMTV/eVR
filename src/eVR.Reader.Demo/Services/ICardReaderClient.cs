using eVR.Reader.Demo.Models;

namespace eVR.Reader.Demo.Services
{
    /// <summary>
    /// Interface that represents the class that makes use of the CardReaderService
    /// </summary>
    public interface ICardReaderClient
    {
        /// <summary>
        /// Present the status of the CardReaderService to the user
        /// </summary>
        /// <param name="status">The read status</param>
        /// <returns></returns>
        Task UpdateReadStatus(string status);

        /// <summary>
        /// Receive the data read from an eVR card and its validation result
        /// </summary>
        /// <param name="cardReadResult">The data and validition result of the card</param>
        /// <returns></returns>
        Task ReceiveCardReadResult(CardReadResult cardReadResult);
    }
}
