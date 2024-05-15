using Microsoft.Extensions.Logging;

namespace eVR.Reader.Validators
{
    /// <summary>
    /// Class used to check whether the card returns a valid ATR.
    /// </summary>
    /// <param name="logger">A logger</param>
    public class AtrCheck(ILogger<AtrCheck> logger)
        : IValidator
    {
        #region Properties

        public string Name => "ATR";

        #endregion

        #region Interface - IValidationCheck

        /// <summary>
        /// Validate whether the card returns a valid ATR.
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>A boolean indicating whether the ATR is valid</returns>
        public async Task<bool> Validate(eVRCardState state)
        {
            return await Task.Run(() =>
            {
                var atrString = Convert.ToHexString(state.ATR!);
                switch (atrString)
                {
                    case eVRDefinitions.AtrGeneration1:
                        logger.LogInformation("Card returned ATR of 1st generation card");
                        return true;
                    case eVRDefinitions.AtrGeneration2:
                        logger.LogInformation("Card returned ATR of 2nd generation card");
                        return true;
                    case eVRDefinitions.AtrGeneration3:
                        logger.LogInformation("Card returned ATR of 3th generation card");
                        return true;
                    default:
                        logger.LogError("Card returned unkown ATR: {atr}", atrString);
                        return false;
                }
            });
        }

        #endregion
    }
}