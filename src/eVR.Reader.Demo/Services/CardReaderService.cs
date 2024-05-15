using eVR.Reader.Demo.Models;
using eVR.Reader.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCSC.Monitoring;
using System.Security.Cryptography;
using System.Windows;

namespace eVR.Reader.Demo.Services
{
    /// <summary>
    /// Service that is used to interact between the eVRCardReader and the UI of this application.
    /// </summary>
    /// <param name="config">A reference to the config file</param>
    /// <param name="logger">A logger</param>
    /// <param name="reader">An eVR specific card reader</param>
    /// <param name="csCaCache">A cache with CSCA certificates</param>
    /// <param name="validators">The validators that will be used to validate the card</param>
    internal sealed class CardReaderService(
          IOptions<Configuration> config
        , ILogger<CardReaderService> logger
        , eVRCardReader reader
        , CsCaCache csCaCache
        , IEnumerable<IValidator> validators)
        : ICardReaderService
    {
        #region Dependencies
        private readonly Configuration _config = config.Value;
        private readonly eVRCardReader _reader = reader;
        #endregion

        #region Private Fields
        private readonly List<string> _monitoredReaders = [];
        private CardReadResult? _cardReadResult;
        private ICardReaderClient? _client;
        private CancellationTokenSource? _processSource;

        #endregion

        #region Interface ICardReaderService

        /// <summary>
        /// Initialize the CardReaderService
        /// </summary>
        /// <param name="client">A reference to the class that uses the CardReaderService</param>
        /// <returns></returns>
        public async Task Initialize(ICardReaderClient client)
        {
            _client = client;
            _reader.SetCardInsertedEvent(Monitor_CardInserted);
            await LoadCSCA();
            SetReadersToMonitor();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Load the CSCA certificates into the cache. 
        /// Close the application if one of the certificates could not be parsed properly.
        /// </summary>
        /// <returns></returns>
        private async Task LoadCSCA()
        {
            try
            {
                await csCaCache.Initialize();
            }
            catch (CryptographicException)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Select the readers that are indicated in the config file with a number.
        /// </summary>
        private void SetReadersToMonitor()
        {
            var availableReaderNames = _reader.GetReaderNames().ToArray();
            foreach (var cardReaderNumber in _config.MonitorCardReaders)
            {
                _monitoredReaders.Add(availableReaderNames[cardReaderNumber - 1]);
            }
            _reader.StartMonitor(_monitoredReaders);
        }

        /// <summary>
        /// React on the event that a card is inserted in a reader
        /// </summary>
        /// <param name="sender">The monitor that detected an inserted card</param>
        /// <param name="e">Information about a smart card reader status</param>
        private void Monitor_CardInserted(object sender, CardStatusEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await _client!.UpdateReadStatus("reading card data");
                    if (await this.ReadInsertedCard(e.ReaderName))
                    {
                        await _client!.UpdateReadStatus("validating card data");
                        await this.PerformValidations();
                        await _client!.UpdateReadStatus("finished processing card data");
                        _cardReadResult!.PrintedData = new CardPrintedData(_cardReadResult.CardState!);
                    }
                    await _client!.ReceiveCardReadResult(_cardReadResult!);
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred: {ex}", ex.Message);
            }
        }

        /// <summary>
        /// Read all elementary files of an inserted card
        /// </summary>
        /// <param name="readerName">The name of the cardreader</param>
        /// <returns></returns>
        private async Task<bool> ReadInsertedCard(string readerName)
        {
            try
            {
                logger.LogInformation("Card inserted, waiting {CardAccessDelay} ms before reading, to avoid timing issues with fast readers", _config.CardAccessDelay);
                await Task.Delay(_config.CardAccessDelay);

                logger.LogInformation("Started reading card data with timeout of {ReadTimeout} ms", _config.ReadTimeout);

                _processSource = new CancellationTokenSource(_config.ReadTimeout);
                _cardReadResult = new CardReadResult
                {
                    CardState = await this._reader.Read(readerName, _processSource.Token)
                };

                logger.LogInformation("Finished reading card data");

                return true;
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Reading card data cancelled because of timeout or manual action");

                _cardReadResult!.CardState = null;
                await _client!.UpdateReadStatus("Reading card cancelled");
                logger.LogInformation("Card status: \"Aborted\"");

                return false;
            }
            catch (Exception ex)
            {
                await _client!.UpdateReadStatus("An error occurred, see logging");
                logger.LogError("Unable to read card: encountered an unknown or empty response: {Message}", ex.Message);
                return false;
            }
            finally
            {
                _processSource?.Dispose();
                _processSource = null;
            }
        }

        /// <summary>
        /// Perform all validations on the card.
        /// </summary>
        /// <returns></returns>
        private async Task PerformValidations()
        {
            _cardReadResult!.Valid = true;
            foreach (var validator in validators) 
            {
                logger.LogInformation("Validating Check {Name}", validator.Name);
                _cardReadResult.Valid &= await validator.Validate(_cardReadResult.CardState!);
            }
        }
        #endregion

        #region Interface IDisposable

        /// <summary>
        /// Dispose this service and its dependencies.
        /// </summary>
        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.RemoveCardInsertedEvent(this.Monitor_CardInserted);
                _reader.Dispose();
            }
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
