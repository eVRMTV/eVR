using eVR.Reader.PCSC;
using Microsoft.Extensions.Logging;

namespace eVR.Reader.Validators
{
    /// <summary>
    /// Class used to check the hash over EF AA in EF SOD.
    /// </summary>
    /// <param name="logger">A logger</param>
    public class PassiveAuthenticationAACheck(ILogger<PassiveAuthenticationAACheck> logger)
                : IValidator
    {
        #region Properties

        public string Name => "Passive Authentication AA";

        #endregion

        #region Interface - IValidationCheck

        /// <summary>
        /// Validate whether the hash over EF AA in EF SOD is correct
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>A boolean indicating whether the hash is correct</returns>
        public async Task<bool> Validate(eVRCardState state)
        {
            return await Task.Run(() =>
            {
                using var hashAlgoritm = Oids.GetHashAlgorithm(state.SOd.DigestAlgorithm!);
                var hashedEF = hashAlgoritm.ComputeHash(state.AA.RawData);
                var hashEF = state.SOd.DatagroupHashValues[state.AA.FileIDHexValue];
                var result = Helper.CompareByteArrays(hashedEF, hashEF);
                if (result)
                {
                    logger.LogInformation("Passive Authentication AA Check finished succesfully.");
                }
                else
                {
                    logger.LogError("Could not finish Passive Authentication AA Check succesfully.");
                }
                return result;
            });
        }

        #endregion
    }
}

