using eVR.Reader.PCSC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace eVR.Reader.Services
{
    /// <summary>
    /// This class is used to read the CSCA certificates from disk and store them in memory.
    /// </summary>
    /// <param name="config">A reference to the config file</param>
    /// <param name="logger">A logger</param>
    [SuppressMessage("external_roslyn", "CA1873")]
    public class CsCaCache(IOptions<Configuration> config, ILogger<CsCaCache> logger)
    {
        #region Private Fields

        private readonly List<X509Certificate2> _cscas = [];

        #endregion

        #region Public Methods

        /// <summary>
        /// Read the CSCA Certificates from disk and store them in memory.
        /// </summary>
        /// <returns></returns>
        public Task Initialize()
        {
            return Task.Run(() =>
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(config.Value.CSCAFolder))
                    {
                        var csca = X509CertificateLoader.LoadCertificateFromFile(file);
                        logger.LogTrace("CSCA Subject : \"{Subject}\".", csca.Subject);
                        logger.LogTrace("CSCA Effective date : \"{EffectiveDate}\".", csca.GetEffectiveDateString());
                        logger.LogTrace("CSCA Expiration date : \"{ExpirationDate}\".", csca.GetExpirationDateString());
                        _cscas.Add(csca);
                    }
                }
                catch (CryptographicException ex)
                {
                    logger.LogError(ex, "{Message}{NewLine}{CscaFolder}", ex.Message, Environment.NewLine,
                        string.IsNullOrEmpty(config.Value.CSCAFolder) ? "N/A" : config.Value.CSCAFolder);
                    throw new CryptographicException($"Could not read one of the CSCA Certificates: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Get the CSCA Certificate that is referenced in the Authority Key Identifier
        /// of the DS Certificate.
        /// </summary>
        /// <param name="dsCertificate">The DS certificate used to find the CSCA Certificate</param>
        /// <returns></returns>
        public X509Certificate2? GetCsCaCertificate(X509Certificate2 dsCertificate)
        {
            var authorityKeyIdentifier = Helper.GetAuthorityKeyIdentifier(dsCertificate);
            var csca = _cscas.FirstOrDefault(c => Helper.CompareByteArrays(Helper.GetAuthorityKeyIdentifier(c).RawData, authorityKeyIdentifier.RawData));
            if (csca != null)
            {
                logger.LogInformation("CSCA found: {Subject}", csca.Subject);
                return csca;
            }
            logger.LogWarning("The CSCA certificate could not be found");
            return null;
        }

        #endregion
    }
}
