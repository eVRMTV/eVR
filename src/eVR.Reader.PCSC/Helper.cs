using System.Formats.Asn1;
using System.IO.Compression;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace eVR.Reader.PCSC
{
    /// <summary>
    /// Utility class with helper functions.
    /// </summary>
    public static partial class Helper
    {
        #region Public Methods

        /// <summary>
        /// Convert a byte array to a hex string with spaces between the different bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexWithSpaces(byte[] bytes)
        {
            return string.Join(" ", bytes.Select(b => b.ToString("X2")));
        }

        /// <summary>
        /// Convert an integer to a byte array with certain length
        /// </summary>
        /// <param name="nr">The integer to be converted</param>
        /// <param name="maxLenght">The maximum length of the byte array</param>
        /// <returns></returns>
        public static byte[] IntToByteArray(int nr, int maxLenght)
        {
            return maxLenght switch
            {
                1 => [(byte)nr],
                2 => [(byte)(nr >> 8), (byte)nr],
                3 => [(byte)(nr >> 16), (byte)(nr >> 8), (byte)nr],
                _ => [(byte)(nr >> 24), (byte)(nr >> 16), (byte)(nr >> 8), (byte)nr],
            };
        }

        /// <summary>
        /// Convert a byte array to an integer
        /// </summary>
        /// <param name="arr">The byte array to be converted</param>
        /// <returns></returns>

        public static int ByteArrayToInt(byte[] arr)
        {
            byte[] fourBytes = [.. new byte[4 - arr.Length], .. arr];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(fourBytes);
            }
            return BitConverter.ToInt32(fourBytes);
        }



        /// <summary>
        /// Check whether two byte arrays are equal
        /// </summary>
        /// <param name="left">The first byte array</param>
        /// <param name="right">The second byte array</param>
        /// <returns>An indication whether the two byte arrays are equal</returns>
        public static bool CompareByteArrays(byte[]? left, byte[]? right)
        {
            return left?.SequenceEqual(right ?? []) == true;
        }

        /// <summary>
        /// Read the asn.1 encoded SubjectPublicKeyInfo blob
        /// </summary>
        /// <param name="publicKey">A byte array representing the asn.1 encoded SubjectPublicKeyInfo</param>
        /// <returns>A RSAParameters with a modulus and an exponent available</returns>
        public static RSAParameters LoadRsaPublicKey(byte[] publicKey)
        {
            using var rsa = RSA.Create();
            try
            {
                rsa.ImportSubjectPublicKeyInfo(publicKey, out _);
            }
            catch 
            {
                // the public key could not be loaded, return the newly created parameters
            }
            return rsa.ExportParameters(false);
        }

        /// <summary>
        /// Decrypt data encrypted with an RSA private key
        /// </summary>
        /// <param name="rsaP">The RSA public key</param>
        /// <param name="encrypted">The encrypted data</param>
        /// <returns></returns>
        public static byte[] Decrypt(RSAParameters rsaP, byte[] encrypted)
        {
            var modulus = new BigInteger(Combine([0x00], rsaP.Modulus!).Reverse().ToArray());
            var exponent = new BigInteger(Combine([0x00], rsaP.Exponent!).Reverse().ToArray());


            // Add 00 byte to end of array for negative values             
            var encryptedAsBI = new BigInteger(Combine([0x00], encrypted).Reverse().ToArray());
            var decipheredAsBi = BigInteger.ModPow(encryptedAsBI, exponent, modulus);

            return decipheredAsBi.ToByteArray().Reverse().ToArray();
        }

        /// <summary>
        /// Decrypt encrypted data using the RSA public key of a X509 certificate
        /// </summary>
        /// <param name="cert">The X509 Certificate</param>
        /// <param name="encrypted">The encrypted data</param>
        /// <returns></returns>
        public static byte[] Decrypt(X509Certificate2 cert, byte[] encrypted)
        {
            RSAParameters p = cert.GetRSAPublicKey()!.ExportParameters(false);
            return Decrypt(p, encrypted);
        }

        /// <summary>
        /// Get the modulus of an RSA key as a BigInteger
        /// </summary>
        /// <param name="p">The RSA key</param>
        /// <returns>A BigInteger with the value of the modulus</returns>
        public static BigInteger Modulus(RSAParameters p)
        {
            return new BigInteger(Combine([0x00], p.Modulus!).Reverse().ToArray());
        }

        /// <summary>
        /// Convert a characterset byte array to an Encoding
        /// </summary>
        /// <param name="characterSet">The character set byte array to be converted</param>
        /// <returns>A valid Encoding</returns>
        /// <exception cref="ArgumentException"></exception>

        public static Encoding GetEncoding(byte[] characterSet)
        {
            return characterSet[0] switch
            {
                0x00 => Encoding.GetEncoding("iso-8859-1"),
                0x01 => Encoding.GetEncoding("iso-8859-5"),
                0x02 => Encoding.GetEncoding("iso-8859-7"),
                _ => throw new ArgumentException("Invalid characterset value"),
            };
        }

        /// <summary>
        /// Decode a string using its encoding
        /// </summary>
        /// <param name="value">The raw binary data</param>
        /// <param name="encoding">The encoding</param>
        /// <returns>The decoded data</returns>
        public static string DecodeString(byte[] value, Encoding encoding)
        {
            string result = string.Empty;

            if (value != null)
            {
                result = encoding.GetString(value);
            }

            return result;
        }


        /// <summary>
        /// Check that the KeyUsageExtension is DigitalSignature.
        /// </summary>
        /// <param name="SOD">The DS Certificate of EF SOD</param>
        /// <returns>An indication whether the key usage is set to DigitalSignature only</returns>
        public static bool KeyUsageIsDigitalSignatureOnly(X509Certificate2 SOD)
        {
            bool keyUsageIsDigitalSignatureFound = false;

            foreach (var extension in SOD.Extensions)
            {
                if (extension is X509KeyUsageExtension keyUsage)
                {
                    if (keyUsage.KeyUsages != X509KeyUsageFlags.DigitalSignature)
                    {
                        // KeyUsage found that is not (only) DigitalSignature
                        return false;
                    }
                    else
                    {
                        // DigitalSignature found....
                        if (!keyUsage.Critical)
                        {
                            // ....but not critical
                            return false;
                        }
                        else
                        {
                            // ... and critical
                            keyUsageIsDigitalSignatureFound = true;
                        }
                    }
                }
            }

            return keyUsageIsDigitalSignatureFound;
        }

        /// <summary>
        /// Get the Authority Key Identifier Extension from a certificate
        /// </summary>
        /// <param name="cert">The certificate to be examined</param>
        /// <returns>The Authority Key Identifier Extension</returns>
        public static X509AuthorityKeyIdentifierExtension GetAuthorityKeyIdentifier(X509Certificate2 cert)
        {
            return cert.Extensions.OfType<X509AuthorityKeyIdentifierExtension>().First();
        }

        /// <summary>
        /// Get the Subject Key Identifier Extension from a certificate
        /// </summary>
        /// <param name="cert">The certificate to be examined</param>
        /// <returns>The Subject Key Identifier Extension</returns>
        public static X509SubjectKeyIdentifierExtension GetSubjectKeyIdentifier(X509Certificate2 cert)
        {
            return cert.Extensions.OfType<X509SubjectKeyIdentifierExtension>().First();
        }

        /// <summary>
        /// GUnzip compressed data
        /// </summary>
        /// <param name="zipData">The compressed data</param>
        /// <returns>The uncompressed data</returns>
        public static string GUnzip2(byte[] zipData)
        {
            using var memoryStream = new MemoryStream(zipData);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(gzipStream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Calculate meldcode based on the vehicle identification number (vin).
        /// Meldcode contains last 4 numbers of the vin, padded left with X if less than 4 numbers in vin
        /// </summary>
        /// <param name="vin">Vehicle identification number</param>
        /// <returns>Meldcode, left padded with 'X' characters</returns>
        public static string ExtractMeldcode(string vin)
        {
            char[] meldcode = ['X', 'X', 'X', 'X'];
            int index = meldcode.Length - 1;

            for (int i = vin.Length - 1; i >= 0; i--)
            {
                if (index < 0)
                {
                    break;
                }

                if (char.IsDigit(vin[i]))
                {
                    meldcode[index--] = vin[i];
                }
            }

            return new string(meldcode);
        }

        /// <summary>
        /// Converts string dateEEJJMMDD to EEYYMMDDToNLDate format
        /// </summary>
        /// <param name="dateEEJJMMDD">Date input string</param>
        /// <returns>Date output string</returns>
        public static string EEYYMMDDToNLDate(string dateEEJJMMDD)
        {

            // Is empty
            if (string.IsNullOrEmpty(dateEEJJMMDD))
            {
                return "-";
            }

            // Correct length
            if (dateEEJJMMDD.Length != 8)
            {
                return dateEEJJMMDD;
            }

            // An integer
            if (!int.TryParse(dateEEJJMMDD, out _))
            {
                return dateEEJJMMDD;
            }

            // Ok; parse it
            return dateEEJJMMDD.Substring(6, 2) + "-" + dateEEJJMMDD.Substring(4, 2) + "-" + dateEEJJMMDD[..4];
        }

        /// <summary>
        /// For every word in the string that is longer dan 'l' a space is inserted at position l
        /// </summary>
        /// <param name="sentence">The input sentence</param>
        /// <param name="l">The max length of a word in the string</param>
        /// <returns>The adjusted string</returns>
        public static string AddSpaceAtWordLength(string sentence, int l)
        {
            if (!string.IsNullOrEmpty(sentence))
            {
                var maxed = new List<string>();
                string[] pieces = sentence.Split(' ');

                foreach (var piece in pieces)
                {
                    if (piece.Length > l)
                    {
                        string s = piece;
                        while (s.Length > 0)
                        {
                            int min = Math.Min(s.Length, l);

                            maxed.Add(s[..min]);
                            s = s[min..];
                        }
                    }
                    else
                    {
                        maxed.Add(piece);
                    }
                }
                return string.Join(" ", maxed);
            }
            return string.Empty;
        }

        /// <summary>
        /// Split a string on spaces at a length of MaxLength characters
        /// </summary>
        /// <param name="sentence">The sentence to split</param>
        /// <param name="maxLength">The max length of a piece</param>
        /// <returns>The parts in a List</returns>
        public static List<string> StringSplitWrap(string sentence, int maxLength)
        {
            var parts = new List<string>();

            string[] pieces = AddSpaceAtWordLength(sentence, maxLength).Split(' ');

            var tempString = new StringBuilder(string.Empty);

            foreach (var piece in pieces)
            {
                if (piece.Length + tempString.Length > maxLength)
                {
                    parts.Add(tempString.ToString());
                    tempString.Clear();
                }

                tempString.Append((tempString.Length == 0 ? string.Empty : " ") + piece);
            }

            if (tempString.Length > 0)
            {
                parts.Add(tempString.ToString());
            }

            return parts;
        }

        /// <summary>
        /// Adds an address to a static List
        /// </summary>
        /// <param name="addressLine">string input</param>
        /// <param name="maxAddressLen">string length</param>
        /// <returns>a List of strings</returns>
        public static List<string> ToAddress(string addressLine, int maxAddressLen)
        {
            if (string.IsNullOrEmpty(addressLine))
            {
                return [];
            }
            List<string> parts = [];
            var regex = RegexAddress();
            Match match = regex.Match(addressLine);

            if (match.Success)
            {
                string postcode = match.Groups["postcode"].ToString();

                if (match.Groups["adres"] != null)
                {
                    string address = match.Groups["adres"].ToString();
                    parts.Add(address[..Math.Min(address.Length, maxAddressLen)]);
                    if (address.Length > maxAddressLen)
                    {
                        postcode = address[maxAddressLen..] + " " + postcode;
                    }
                }

                if (match.Groups["wpl"] != null)
                {
                    postcode += "  " + match.Groups["wpl"].ToString();
                }

                parts.Add(postcode);
                return parts;
            }
            else
            {
                // Could not parse string into postcode and wpl; split into 2 parts of MaxAdresLen
                parts.Add(addressLine[..Math.Min(addressLine.Length, maxAddressLen)]);

                if (addressLine.Length > maxAddressLen)
                {
                    string line = addressLine[maxAddressLen..];

                    parts.Add(line[..Math.Min(line.Length, maxAddressLen)]);
                }

                return parts;
            }
        }

        /// <summary>
        /// Convert a byte array to a valid OID
        /// </summary>
        /// <param name="oid">Byte array containing the OID</param>
        /// <returns>A valid OID</returns>
        public static Oid ConvertOid(byte[] oid)
        {
            byte[] oidTlv = [0x06, (byte)oid.Length, .. oid];
            var oidString = new AsnReader(oidTlv, AsnEncodingRules.DER).ReadObjectIdentifier();
            return new Oid(oidString);
        }

        #endregion

        #region private Methods

        /// <summary>
        /// Combine 2 byte arrays
        /// </summary>
        /// <param name="first">The first byte array</param>
        /// <param name="second">The second byte array</param>
        /// <returns></returns>
        private static byte[] Combine(byte[] first, byte[] second)
        {
            return [.. first, .. second];
        }

        [GeneratedRegex(@"(?<adres>.*)\s*(?<postcode>\d{4,4}\s[a-zA-Z]{2,2})\s*(?<wpl>.*)")]
        private static partial Regex RegexAddress();

        #endregion
    }
}
