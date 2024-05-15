using System.Security.Cryptography;

namespace eVR.Reader
{

    /// <summary>
    /// Class with various OID constants
    /// </summary>
    public static class Oids
    {
        public const string EcPublicKey = "1.2.840.10045.2.1";
        public const string AaProtocolObject = "2.23.136.1.1.5";
        public const string Sha256 = "2.16.840.1.101.3.4.2.1";
        public const string Sha384 = "2.16.840.1.101.3.4.2.2";
        public const string Sha512 = "2.16.840.1.101.3.4.2.3";
        public const string Sha256WithRSAEncryption = "1.2.840.113549.1.1.11";
        public const string Sha384WithRSAEncryption = "1.2.840.113549.1.1.12";
        public const string Sha512WithRSAEncryption = "1.2.840.113549.1.1.13";
        public const string EcdsaPlainSha256 = "0.4.0.127.0.7.1.1.4.1.3";
        public const string EcdsaPlainSha384 = "0.4.0.127.0.7.1.1.4.1.4";
        public const string EcdsaPlainSha512 = "0.4.0.127.0.7.1.1.4.1.5";

        /// <summary>
        /// Get an HashAlgorithm based on a specific OID.
        /// </summary>
        /// <param name="oid">An OID</param>
        /// <returns>The HashAlgorithm that corresponds to the OID</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static HashAlgorithm GetHashAlgorithm(Oid oid)
        {
            return oid.Value switch
            {
                Sha256 or Sha256WithRSAEncryption => SHA256.Create(),
                Sha384 or Sha384WithRSAEncryption => SHA384.Create(),
                Sha512 or Sha512WithRSAEncryption => SHA512.Create(),
                _ => throw new ArgumentOutOfRangeException(nameof(oid)),
            };
        }

        /// <summary>
        /// Get a signature algorithm based on a specific OID.
        /// </summary>
        /// <param name="oid">An OID</param>
        /// <returns>A string with the signature algorithm</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetSignatureAlgorithm(Oid oid) 
        {
            return oid.Value switch
            {
                EcdsaPlainSha256 => "SHA-256withECDSA",
                EcdsaPlainSha384 => "SHA-384withECDSA",
                EcdsaPlainSha512 => "SHA-512withECDSA",
                _ => throw new ArgumentOutOfRangeException(nameof(oid)),
            };
        }

    }
}
