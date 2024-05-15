using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using eVR.Reader.PCSC;
using eVR.Reader.Data;
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace eVR.Reader.Validators
{
    /// <summary>
    /// Class used to check whether Active Authentication works properly on the card.
    /// </summary>
    /// <param name="reader">A cardreader</param>
    /// <param name="generator">A random generator</param>
    /// <param name="logger">A logger</param>
    public class ActiveAuthenticationAACheck(
          IReader reader
        , IRandomGenerator generator
        , ILogger<ActiveAuthenticationAACheck> logger)
        : IValidator
    {
        #region Properties

        public string Name => "Active Authentication AA";

        #endregion

        #region Interface - IValidationCheck

        /// <summary>
        /// Validate Active Authentication.
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>A boolean indicating whether the Active Authentication works properly</returns>
        public async Task<bool> Validate(eVRCardState state)
        {
            var result = state.AA.KeyType!.Value switch
            {
                Oids.EcPublicKey => await EcActiveAuthentication(state),
                _ => await RsaActiveAuthentication(state)
            };
            if (result)
            {
                logger.LogInformation("Active Authentication Check finished succesfully.");
            }
            else 
            {
                logger.LogError("Could not finish Active Authentication Check succesfully.");
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Active Authentication using Elliptic Curve.
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>A boolean indicating whether the Active Authentication works properly</returns>
        private async Task<bool> EcActiveAuthentication(eVRCardState state)
        {
            logger.LogInformation("Executing Active Authentication using Elliptic Curve");
            await reader.SelectApplication(eVRDefinitions.AID);
            var random = await generator.GenerateRandom(8);
            var data = await reader.InternalAuthenticate(random);

            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfo.GetInstance(state.AA.ActiveAuthenticationPublicKeyInfo);
            ECPublicKeyParameters publicKey = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(publicKeyInfo);

            int keySize = publicKey.Parameters.Curve.FieldSize / 8;
            var di1 = new DerInteger(ConvertToBigInteger(data.Take(keySize).ToArray()));
            var di2 = new DerInteger(ConvertToBigInteger(data.Skip(keySize).ToArray()));

            var seq = new DerSequence(new Asn1Encodable[] { di1, di2 });

            var signatureAlgorithm = Oids.GetSignatureAlgorithm(state.SecurityInfos.HashAlgorithmOid!);
            var signer = SignerUtilities.GetSigner(signatureAlgorithm);

            signer.Init(false, publicKey);

            signer.BlockUpdate(random, 0, random.Length);
            return signer.VerifySignature(seq.GetDerEncoded());
        }

        /// <summary>
        /// Active Authentication using RSA.
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>A boolean indicating whether the Active Authentication works properly</returns>
        private async Task<bool> RsaActiveAuthentication(eVRCardState state)
        {
            logger.LogInformation("Executing Active Authentication using RSA");
            await reader.SelectApplication(eVRDefinitions.AID);
            var random = await generator.GenerateRandom(8);
            var data = await reader.InternalAuthenticate(random);
            var aaPublicKey = Helper.LoadRsaPublicKey(state.AA.ActiveAuthenticationPublicKeyInfo);
            var deciphered = Helper.Decrypt(aaPublicKey, [0x00, .. data]);
            var modulusBitLength = (int)Math.Ceiling(BigInteger.Log(Helper.Modulus(aaPublicKey), 2));
            var m1 = new byte[modulusBitLength / 8 - 35 + random.Length];
            Array.Copy(deciphered, 1, m1, 0, deciphered.Length - 35);
            Array.Copy(random, 0, m1, deciphered.Length - 35, random.Length);
            var hashAlgoritm = SHA256.Create();
            var hashedM1 = hashAlgoritm.ComputeHash(m1);
            var hashDeciphered = new byte[hashAlgoritm.HashSize / 8];
            Array.Copy(deciphered, deciphered.Length - 34, hashDeciphered, 0, 32);
            return Helper.CompareByteArrays(hashDeciphered, hashedM1);
        }

        /// <summary>
        /// Convert a byte array to a BigInteger
        /// </summary>
        /// <param name="bytes">A byte array</param>
        /// <returns>A BigInteger</returns>
        private static Org.BouncyCastle.Math.BigInteger ConvertToBigInteger(byte[] bytes)
        {
            var result = Org.BouncyCastle.Math.BigInteger.Zero;
            var bas = Org.BouncyCastle.Math.BigInteger.ValueOf(256);

            for (int i = 0; i < bytes.Length; i++)
            {
                result = result.Multiply(bas);
                result = result.Add(Org.BouncyCastle.Math.BigInteger.ValueOf(bytes[i] & 0xff));
            }

            return result;
        }
    }
}

