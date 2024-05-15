using eVR.Reader.PCSC;

namespace eVR.Reader.Demo.Models
{

    /// <summary>
    /// Class representing a printed item on the outside of the eVR card.
    /// </summary>
    public class PrintedItem
    {
        #region Properties
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Registration { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Presentation { get; set; } = string.Empty;
        public int? PresentationMaxLength { get; set; }
        public int? PresentationIndex { get; set; }
        public bool Optional { get; set; }
        #endregion

        #region Public Method

        /// <summary>
        /// Determine the value of this printed item.
        /// </summary>
        /// <param name="state">The data read from the card</param>
        public void DetermineValue(eVRCardState state)
        {
            // Is the value already set in the configuration file?
            if (!string.IsNullOrWhiteSpace(Value))
            {
                return;
            }
            string rawValue = GetRawValue(state);
            Value = PresentValue(rawValue);
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Get the raw value from the state.
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns></returns>
        private string GetRawValue(eVRCardState state)
        {
            IRegistrationFile? ef = Registration.ToUpper() switch
            {
                "A" => state.RegistrationA,
                "B" => state.RegistrationB,
                "C" => state.RegistrationC,
                _ => null
            };

            if (ef == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(Tag) || ef.ParsedData.Tag(Tag) == null)
            {
                return string.Empty;
            }
            var bytes = ef.ParsedData.Tag(Tag)!.Value;
            return Helper.DecodeString(bytes, ef.CharacterSetEncoding!);
        }

        /// <summary>
        /// Format the raw data in the way it is printed on the outside of the card.
        /// </summary>
        /// <param name="rawValue">The raw value read from the card</param>
        /// <returns>A formatted value</returns>
        private string PresentValue(string rawValue)
        {
            switch (Presentation?.ToLower())
            {
                case "address":
                    var addressLines = Helper.ToAddress(rawValue, PresentationMaxLength.GetValueOrDefault());
                    if (addressLines.Count > PresentationIndex.GetValueOrDefault())
                    {
                        return addressLines[PresentationIndex.GetValueOrDefault()];
                    }
                    return string.Empty;
                case "date":
                    return Helper.EEYYMMDDToNLDate(rawValue);
                case "reportingcode":
                    return Helper.ExtractMeldcode(rawValue);
                case "split":
                    var splitted = Helper.StringSplitWrap(rawValue, PresentationMaxLength.GetValueOrDefault());
                    if (splitted.Count > PresentationIndex.GetValueOrDefault())
                    {
                        return splitted[PresentationIndex.GetValueOrDefault()];
                    }
                    return string.Empty;
                default:
                    return rawValue;
            }
        }



        #endregion
    }
}
