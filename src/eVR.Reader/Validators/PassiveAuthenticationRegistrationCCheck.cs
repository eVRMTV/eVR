using eVR.Reader.PCSC;
using Microsoft.Extensions.Logging;

namespace eVR.Reader.Validators
{
    /// <summary>
    /// Class used to check the hash over EF Registration_C in EF SOD
    /// </summary>
    /// <param name="logger">A logger</param>
    public class PassiveAuthenticationRegistrationCCheck(
          ILogger<PassiveAuthenticationRegistrationCCheck> logger)
        : IValidator
    {
        #region Properties

        public string Name => "Passive Authentication Registration C";

        #endregion

        #region Interface - IValidationCheck

        /// <summary>
        /// Validate whether the hash over EF Registration_C in EF SOD is correct
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>A boolean indicating whether the hash is correct</returns>
        public async Task<bool> Validate(eVRCardState state)
        {
            return await Task.Run(() =>
            {
                using var hashAlgoritm = Oids.GetHashAlgorithm(state.SOd.DigestAlgorithm!);
                var hashedEF = hashAlgoritm.ComputeHash(state.RegistrationC.RawData);
                var hashEF = state.SOd.DatagroupHashValues[state.RegistrationC.FileIDHexValue];
                var result = Helper.CompareByteArrays(hashedEF, hashEF);
                if (result)
                {
                    logger.LogInformation("Passive Authentication Registration C Check finished succesfully.");
                }
                else
                {
                    logger.LogError("Could not verify the hash of EF Registration C in EF SOD.");
                }
                return result;
            });
        }

        #endregion
    }
}
