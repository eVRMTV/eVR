using PCSC.Iso7816;
using PCSC.Monitoring;

namespace eVR.Reader.PCSC
{
    public interface IReader
        : IDisposable
    {
        /// <summary>
        /// Add an event handler to the CardInserted event
        /// </summary>
        /// <param name="event">The event handler</param>
        void SetCardInsertedEvent(CardInsertedEvent @event);

        /// <summary>
        /// Remove an event handler from the CardInserted event
        /// </summary>
        /// <param name="event">The event handler</param>
        void RemoveCardInsertedEvent(CardInsertedEvent @event);

        /// <summary>
        /// Select an application by its AID
        /// </summary>
        /// <param name="aID">The Application Identifier</param>
        /// <param name="checkStatusBytes">An indication whether the status bytes should be checked</param>
        /// <returns></returns>
        Task<ResponseApdu?> SelectApplication(byte[] aID, bool checkStatusBytes = true);

        /// <summary>
        /// Select a Master File (MF)
        /// </summary>
        /// <returns></returns>
        Task SelectMF();

        /// <summary>
        /// Read an Elementary File (EF)
        /// </summary>
        /// <param name="aID">The application identifier</param>
        /// <param name="fileID">The file identifier</param>
        /// <returns>A byte array with the conent of the file</returns>
        /// <exception cref="CardReaderException"></exception>
        /// <exception cref="ArgumentException"></exception>
        Task<byte[]> ReadElementaryFile(byte[] aID, byte[] fileID);

        /// <summary>
        /// Get the names of all cardreaders on the system.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetReaderNames();

        /// <summary>
        /// Start monitoring the card readers.
        /// </summary>
        /// <param name="readerNames">The names of the cardreaders to be monitored</param>
        void StartMonitor(IEnumerable<string> readerNames);

        /// <summary>
        /// Stop monitoring the cardreaders.
        /// </summary>
        void StopMonitor();

        /// <summary>
        /// Connect a specific cardreader
        /// </summary>
        /// <param name="readerName">The name of the cardreader</param>
        /// <returns></returns>
        /// <exception cref="CardReaderException"></exception>
        Task ConnectReader(string readerName);

        /// <summary>
        /// Transmit data to the card.
        /// </summary>
        /// <param name="sendBuffer">The data to be sent</param>
        /// <param name="recvBuffer">The data to be received</param>
        /// <returns></returns>
        bool Transmit(byte[] sendBuffer, ref byte[] recvBuffer);

        /// <summary>
        /// Get the ATR string of the card.
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GetATRString();

        /// <summary>
        /// Execute Internal Authentication
        /// </summary>
        /// <param name="random">A random byte array</param>
        /// <returns></returns>
        /// <exception cref="CardReaderException"></exception>
        Task<byte[]> InternalAuthenticate(byte[] random);
    }
}
