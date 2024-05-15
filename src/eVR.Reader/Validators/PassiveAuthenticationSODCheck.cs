using eVR.Reader.PCSC;
using eVR.Reader.Services;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace eVR.Reader.Validators
{
    /// <summary>
    /// Class used to check the signature in EF SOD
    /// </summary>
    /// <param name="csCaCache">A cache with the CSCA certificates</param>
    /// <param name="logger">A logger</param>
    public class PassiveAuthenticationSOdCheck(
          CsCaCache csCaCache
        , ILogger<PassiveAuthenticationSOdCheck> logger)
        : IValidator
    {
        #region Properties

        public string Name => "Passive Authentication SOd";

        #endregion

        #region Interface - IValidationCheck

        /// <summary>
        /// Validate whether the the signature in EF SOD is correct
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>A boolean indicating whether the signature is correct</returns>
        public async Task<bool> Validate(eVRCardState state)
        {
            return await Task.Run(() =>
            {
                return ValidateDsCertificate(state) &&
                VerifySignature(state) &&
                VerifyEcContentHash(state);
            });
        }

        #endregion

        #region Private Methods
        private bool ValidateDsCertificate(eVRCardState state)
        {
            var ds = state.SOd.DSCertificate!;
            if (!Helper.KeyUsageIsDigitalSignatureOnly(ds))
            {
                logger.LogError("The Key Usage of the DS Certificate in EF SOD is not set to Digital Signature Only");
                return false;
            }
            if (DateTime.Now < ds.NotBefore) // note: ds.NotAfter is not checked: a card with an expired DS Certificate is still valid.
            {
                logger.LogError("The DS Certificate is not valid at this moment ({notBefore} - {notAfter}", ds.NotBefore, ds.NotAfter);
                return false;
            }

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
            try
            {
                // verify the signature of the DSCertificate
                var cscaBC = new X509CertificateParser().ReadCertificate(csca.RawData);
                var dsCertificateBC = new X509CertificateParser().ReadCertificate(ds.RawData);
                dsCertificateBC.Verify(cscaBC.GetPublicKey());
            }
            catch (Exception)
            {
                logger.LogError("Could not verify the signature of the DS Certificate");
                return false;
            }
            return true;
        }

        private bool VerifySignature(eVRCardState state)
        {
            try
            {
                var signedFile = new CmsSignedData(state.SOd.RawData);
                var certStore = signedFile.GetCertificates();
                var certs = certStore.EnumerateMatches(new X509CertStoreSelector());
                var signerStore = signedFile.GetSignerInfos();
                var signers = signerStore.GetSigners();
                foreach (Org.BouncyCastle.X509.X509Certificate certification in certs)
                {
                    foreach (SignerInformation tempSigner in signers)
                    {
                        if (!tempSigner.Verify(certification))
                        {
                            logger.LogError("Could not verify the signature of EF SOD");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool VerifyEcContentHash(eVRCardState state) 
        {
            using var hashAlgorithm = Oids.GetHashAlgorithm(state.SOd.DigestAlgorithm!);
            byte[] hashedEContent = hashAlgorithm.ComputeHash(state.SOd.RDWIdsSecurityObject);
            var result = Helper.CompareByteArrays(state.SOd.AttributeValue, hashedEContent);
            if (result)
            {
                logger.LogInformation("Passive Authentication SOD Check finished succesfully.");
            }
            else
            {
                logger.LogError("Could not verify the hash on the EContent in EF SOD.");
            }
            return result;
        }
        #endregion
    }
}

