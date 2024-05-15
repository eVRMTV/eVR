using eVR.Reader.PCSC;
using eVR.Reader.Services;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;

namespace eVR.Reader.Validators
{
    /// <summary>
    /// Class used to check the hashes over EF Registration_A in EF SOD and in EF Signature_A
    /// </summary>
    /// <param name="csCaCache">A cache with the CSCA certificates</param>
    /// <param name="logger">A logger</param>
    public class PassiveAuthenticationRegistrationACheck(
          CsCaCache csCaCache
        , ILogger<PassiveAuthenticationRegistrationACheck> logger)
        : IValidator
    {
        #region Properties

        public string Name => "Passive Authentication Registration A";

        #endregion

        #region Interface - IValidationCheck

        /// <summary>
        /// Validate whether the hashes over EF Registration_A in EF SOD and in 
        /// EF Signature_A are correct
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>A boolean indicating whether the hashes are correct</returns>
        public async Task<bool> Validate(eVRCardState state)
        {
            return await Task.Run(() =>
            {
                var ds = state.C_IA_A_DS.DSCertificate!;
                return CheckEfSodHash(state) &&
                ValidateDsCertificate(ds, state) &&
                VerifySignature(ds, state);
            });
        }

        #endregion

        #region Private Methods

        private bool CheckEfSodHash(eVRCardState state)
        {
            using var hashAlgoritmSod = Oids.GetHashAlgorithm(state.SOd.DigestAlgorithm!);
            var hashedEF = hashAlgoritmSod.ComputeHash(state.RegistrationA.RawData);
            var hashEF = state.SOd.DatagroupHashValues[state.RegistrationA.FileIDHexValue];
            if (!Helper.CompareByteArrays(hashedEF, hashEF))
            {
                logger.LogError("Could not verify the hash of EF Registration A in EF SOD");
                return false;
            }
            return true;
        }

        private bool ValidateDsCertificate(X509Certificate2 ds, eVRCardState state)
        {
            var csca = csCaCache.GetCsCaCertificate(ds);
            if (csca == null)
            {
                state.MissingCSCA = true;
                return false;
            }

            var ski = Helper.GetSubjectKeyIdentifier(csca);
            var aki = Helper.GetAuthorityKeyIdentifier(ds);
            if (!Helper.CompareByteArrays(ski.SubjectKeyIdentifierBytes.ToArray(), aki.KeyIdentifier!.Value.ToArray()))
            {
                logger.LogError("Could not match the Subject Key Identifier of the CSCA certificate with the Authority Key Identifier of the DS Certificate");
                return false;
            }

            if (DateTime.Now < ds.NotBefore) // note: ds.NotAfter is not checked: a card with an expired DS Certificate is still valid.
            {
                logger.LogError("The DS Certificate is not valid at this moment ({notBefore} - {notAfter}", ds.NotBefore, ds.NotAfter);
                return false;
            }
            return VerifySignatureDsCertificate(ds, csca);
        }

        private bool VerifySignatureDsCertificate(X509Certificate2 ds, X509Certificate2 csca)
        {
            try
            {
                // verify the signature of the DSCertificate
                var cscaBC = new X509CertificateParser().ReadCertificate(csca.RawData);
                var dsCertificateBC = new X509CertificateParser().ReadCertificate(ds.RawData);
                dsCertificateBC.Verify(cscaBC.GetPublicKey());
                return true;
            }
            catch (Exception)
            {
                logger.LogError("Could not verify the signature of the DS Certificate");
                return false;
            }
        }
        private bool VerifySignature(X509Certificate2 ds, eVRCardState state)
        {
            var descipheredSignature = Helper.Decrypt(ds, state.SignatureA.Signature).Reverse().Take(32).Reverse().ToArray();
            using var hashAlgoritmSignature = Oids.GetHashAlgorithm(state.SignatureA.SignatureAlgorithmOid!);
            var hashedRegistration = hashAlgoritmSignature.ComputeHash(state.RegistrationA.RawData);
            var result = Helper.CompareByteArrays(descipheredSignature, hashedRegistration);
            if (result)
            {
                logger.LogInformation("Passive Authentication Registration A Check finished succesfully.");
            }
            else
            {
                logger.LogError("Could not verify Signature A.");
            }
            return result;
        }
        #endregion
    }
}

