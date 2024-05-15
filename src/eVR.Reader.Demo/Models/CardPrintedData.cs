using System.IO;
using System.Text.Json;

namespace eVR.Reader.Demo.Models
{
    /// <summary>
    /// Class that represent the data that is printed on the outside of the eVR card.
    /// </summary>
    public class CardPrintedData
    {
        #region Dependencies
        private readonly eVRCardState _state;
        private readonly Dictionary<string, PrintedItem> _printedItems;
        #endregion

        #region Public Properties

        /// <summary>
        /// Get a specific printed item
        /// </summary>
        /// <param name="key">A string representing a position on the card. 
        /// E.g. V-1 represents the first item on the front of the card, 
        /// and A-2 represents the second item on the back of the card.</param>
        /// <returns></returns>
        public PrintedItem? this[string key]
        {
            get
            {
                return !_printedItems.TryGetValue(key, out PrintedItem? value) ? null : value;
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">The data read from the card</param>
        public CardPrintedData(eVRCardState state)
        {
            _state = state;
            _printedItems = new Dictionary<string, PrintedItem>(StringComparer.InvariantCultureIgnoreCase);
            ReadCardBackPrintedDataFromFile();
            ReadCardFrontPrintedDataFromFile();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Read the data used on the front of the card using template file CardFrontPrintedData.json
        /// </summary>
        private void ReadCardFrontPrintedDataFromFile()
        {
            using var file = File.Open("CardFrontPrintedData.json", FileMode.Open);
            var items = JsonSerializer.Deserialize<Dictionary<string, PrintedItem>>(file);
            foreach (var item in items!)
            {
                item.Value.DetermineValue(_state);
                _printedItems.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Read the data used on the back of the card using template file CardBackPrintedData.json
        /// </summary>
        private void ReadCardBackPrintedDataFromFile()
        {
            using var file = File.Open("CardBackPrintedData.json", FileMode.Open);
            var configuration = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, PrintedItem>>>(file);
            var vehicleCategory = _state.RegistrationB.VehicleCategory;
            var key = _state.Generation switch
            {
                CardGeneration.Generation3 => configuration!.Keys.FirstOrDefault(k => k.Split(";").Any(s =>
                    vehicleCategory.StartsWith(s, StringComparison.InvariantCultureIgnoreCase)),
                    configuration.Keys.First()), // first key will be used as default (functionally not necessary but thechnical fallback value)
                _ => "OLD"
            };
            var items = configuration![key];
            var keyQueue = new Queue<string>();
            foreach (var item in items)
            {
                // Do not move fields A-21 to A-24 to the left part of the card
                ClearQueueWhenNecessary(item.Key, keyQueue);

                keyQueue.Enqueue(item.Key);
                item.Value.DetermineValue(_state);
                
                // Add the item always, except in case of optional empty values
                // In case of optional empty values, the next field will be assigned to
                // the previous key in the queue
                if (!ShouldBeSkipped(item.Value))
                {
                    _printedItems.Add(keyQueue.Dequeue(), item.Value);
                }     
            }
        }

        /// <summary>
        /// Determine whether a printed item should be skipped, so that the next item 
        /// will move upwards.
        /// </summary>
        /// <param name="printedItem"></param>
        /// <returns></returns>
        private static bool ShouldBeSkipped(PrintedItem printedItem) 
        {
            return printedItem.Optional &&
                (string.IsNullOrEmpty(printedItem.Value) || printedItem.Value == "-");
        }

        /// <summary>
        /// A queue is used to place the printed items on the back of the card on the 
        /// next available position. This is done because optional items may be skipped
        /// and then the next item will move upwards. However, items on the right side 
        /// (A-21 to A-24) will remain on the right side and should not move to the left 
        /// part of the card.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyQueue"></param>
        private static void ClearQueueWhenNecessary(string key, Queue<string> keyQueue) 
        {
            static int parseNumber(string s) => int.Parse(new string(s.Where(char.IsDigit).ToArray()));
            if (parseNumber(key) > 20 && keyQueue.Any(s => parseNumber(s) <= 20)) 
            {
                keyQueue.Clear();
            }
        }

        #endregion
    }
}
