using eVR.Reader.Demo.Models;
using eVR.Reader.Demo.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace eVR.Reader.Demo.ViewModels
{
    /// <summary>
    /// MainViewModel, the ViewModel that represents the data context of the MainWindow
    /// </summary>
    public sealed class MainViewModel
        : ObservableObject
        , IViewModel
        , ICardReaderClient
        , IDisposable
    {
        #region Dependencies
        private readonly ICardReaderService _cardReaderService;
        #endregion

        #region Properties
        public string Name => $"Demo Tool - View Electronic Vehicle Registration information from the smartcard";

        public ObservableCollection<IViewModel> ViewModels { get; }


        private IViewModel? _currentViewModel;
        public IViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private string? _readStatus;

        public string? ReadStatus
        {
            get => _readStatus;
            set => SetProperty(ref _readStatus, value);
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cardReaderService">A CardReaderService</param>
        /// <param name="logger">A logger</param>
        public MainViewModel(
              ICardReaderService cardReaderService
            , ILogger<MainViewModel> logger)
        {
            _cardReaderService = cardReaderService;
            ViewModels = [new EmptyCardViewModel()];
            CurrentViewModel = ViewModels.First();
        }

        /// <summary>
        /// Initialize the MainViewModel
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            await _cardReaderService.Initialize(this);
        }

        /// <summary>
        /// Dispose the MainViewModel
        /// </summary>
        public void Dispose()
        {
            _cardReaderService?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Present the status of the CardReaderService to the user
        /// </summary>
        /// <param name="status">The read status</param>
        /// <returns></returns>
        public async Task UpdateReadStatus(string status)
        {
            await Task.Run(() => ReadStatus = status);
        }

        /// <summary>
        /// Receive the data read from an eVR card and its validation result
        /// </summary>
        /// <param name="cardReadResult"></param>
        /// <returns>The data and validition result of the card</returns>
        public async Task ReceiveCardReadResult(CardReadResult cardReadResult)
        {
            await Task.Run(() =>
            {
                int cvoNumber = 1;

                // Application.Current.Dispatcher.Invoke: this method is called from the CardReaderService
                // that does not run on the UI thread. Without the dispatcher this will cause errors.
                Application.Current.Dispatcher.Invoke(ViewModels.Clear);
                Application.Current.Dispatcher.Invoke(() => ViewModels.Add(new CardViewModel(cardReadResult)));
                if (cardReadResult.CardState?.RegistrationC?.CVOs?.Length > 0)
                {
                    foreach (var cvo in cardReadResult.CardState.RegistrationC.CVOs)
                    {
                        Application.Current.Dispatcher.Invoke(() => ViewModels.Add(new CvoDataViewModel(cvo, cvoNumber++)));
                    }
                }
                Application.Current.Dispatcher.Invoke(() => CurrentViewModel = ViewModels.First());
            });
        }
        #endregion
    }
}
