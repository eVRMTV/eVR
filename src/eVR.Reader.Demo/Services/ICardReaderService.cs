namespace eVR.Reader.Demo.Services
{
    /// <summary>
    /// Interface that represents the CardReaderService
    /// </summary>
    public interface ICardReaderService
        : IDisposable
    {
        /// <summary>
        /// Initialize the CardReaderService
        /// </summary>
        /// <param name="client">A reference to the class that uses the CardReaderService</param>
        /// <returns></returns>
        Task Initialize(ICardReaderClient client);
    }
}
