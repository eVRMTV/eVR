using eVR.Reader.PCSC;
using Microsoft.Extensions.Logging;

namespace eVR.Reader.Validators
{
    /// <summary>
    /// Class used to check the content of EF SecurityInfos and the hash over it in EF SOD
    /// </summary>
    /// <param name="logger">A logger</param>
    public class SecurityInfosCheck(ILogger<SecurityInfosCheck> logger)
        : IValidator
    {
        #region Properties
        public string Name => "SecurityInfos";

        #endregion

        #region Interface - IValidator
        public async Task<bool> Validate(eVRCardState state)
        {
            return await Task.Run(() =>
            {
                if (state.Generation != CardGeneration.Generation3)
                {
                    logger.LogInformation("EF Security is not checked on a card of this generation ({generation})", state.Generation);
                    return true;
                }

                if (!state.SecurityInfos.ActiveAuthenticationOid?.Value?.Equals(Oids.AaProtocolObject) == true)
                {
                    logger.LogError("The Active Authentication Oid of EF SecurityInfos is not set to AA Protocol Object ({oid})", state.SecurityInfos.ActiveAuthenticationOid);
                    return false;
                }
                if (!Helper.CompareByteArrays(state.SecurityInfos.Version, [0x01]))
                {
                    logger.LogError("The Version of EF Security Infos is not set to 0x01 ({version})", Convert.ToHexString(state.SecurityInfos.Version));
                    return false;
                }

                using var hashAlgoritm = Oids.GetHashAlgorithm(state.SOd.DigestAlgorithm!);
                var hashedEF = hashAlgoritm.ComputeHash(state.SecurityInfos.RawData);
                var hashEF = state.SOd.DatagroupHashValues[state.SecurityInfos.FileIDHexValue];
                var result = Helper.CompareByteArrays(hashedEF, hashEF);
                if (result)
                {
                    logger.LogInformation("SecurityInfos Check finished succesfully.");
                }
                else
                {
                    logger.LogError("Could not verify the hash of EF SecurityInfos in EF SOD.");
                }
                return result;
            });
        }
        #endregion
    }
}
