namespace EVR.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Numerics;

    public static class Helper
    {
        public static bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        /// <summary>
        /// Convert an array of bytes to a string representation
        /// </summary>
        /// <param name="arr">The byte array to convert to a string</param>
        /// <param name="totalBytes">0 to convert all</param>
        /// <param name="includeSpaceBetweenBytes">Include a space between the bytes</param>
        public static string ByteArrayToString(byte[] arr, int totalBytes, bool includeSpaceBetweenBytes)
        {
            StringBuilder sb = new StringBuilder();
            string spacer = (includeSpaceBetweenBytes) ? " " : "";
            if (null != arr)
            {
                int stop = (0 == totalBytes) ? arr.Length : totalBytes;

                for (int i = 0; i < stop; i++)
                {
                    sb.AppendFormat("{0:X2}{1}", arr[i], spacer);
                }
            }
            else
            {
                sb.Append("No data to print");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convert an array of bytes to a string representation
        /// </summary>
        /// <param name="arr">The byte array to convert to a string</param>
        /// <param name="includeSpaceBetweenBytes">Include a space between the bytes</param>
        public static string ByteArrayToString(byte[] arr, bool includeSpaceBetweenBytes)
        {
            return ByteArrayToString(arr, arr.Length, includeSpaceBetweenBytes);
        }

        /// <summary>
        /// Convert an array of bytes to a string representation
        /// </summary>
        /// <param name="arr">The byte array to convert to a string</param>
        /// <returns>A string representation of the byte array, bytes are seperated with a single space</returns>
        public static string ByteArrayToString(byte[] arr)
        {
            return ByteArrayToString(arr, arr.Length, true);
        }

        /// <summary>
        /// Get the CRL Distribution points from the certificate
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <returns>a list of CRL distribution points</returns>
        public static string[] GetCrlDistributionPoints(X509Certificate2 certificate)
        {
            X509Extension ext = null;

            foreach (var extension in certificate.Extensions)
            {
                if (string.Compare(extension.Oid.Value, "2.5.29.31") == 0)
                {
                    ext = extension;
                    break;
                }
            }

            if (ext == null || ext.RawData == null || ext.RawData.Length < 11)
                return null;

            int prev = -2;
            List<string> items = new List<string>();
            while (prev != -1 && ext.RawData.Length > prev + 1)
            {
                int next = IndexOf(ext.RawData, 0x86, prev == -2 ? 8 : prev + 1);
                if (next == -1)
                {
                    if (prev >= 0)
                    {
                        string item = Encoding.UTF8.GetString(ext.RawData, prev + 2, ext.RawData.Length - (prev + 2));
                        items.Add(item);
                    }

                    break;
                }

                if (prev >= 0 && next > prev)
                {
                    string item = Encoding.UTF8.GetString(ext.RawData, prev + 2, next - (prev + 2));
                    items.Add(item);
                }

                prev = next;
            }

            return items.ToArray();
        }

        /// <summary>
        /// Check that the KeyUsageExtension is DigitalSignature.
        /// </summary>
        /// <param name="SOD"></param>
        /// <param name="keyUsageStr"></param>
        /// <returns></returns>
        public static bool KeyUsageIsDigitalSignatureOnly(X509Certificate2 SOD)
        {
            bool keyUsageIsDigitalSignatureFound = false;

            foreach (var extension in SOD.Extensions)
            {
                if (extension is System.Security.Cryptography.X509Certificates.X509KeyUsageExtension)
                {
                    System.Security.Cryptography.X509Certificates.X509KeyUsageExtension keyUsage = extension as System.Security.Cryptography.X509Certificates.X509KeyUsageExtension;
                    if (string.Compare(keyUsage.KeyUsages.ToString().ToUpper(), "DIGITALSIGNATURE") != 0)
                    {
                        // KeyUsage found that is not DigitalSignature
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

        private static int IndexOf(byte[] instance, byte item, int start)
        {
            for (int i = start, l = instance.Length; i < l; i++)
            {
                if (instance[i] == item)
                {
                    return i;
                }
            }

            return -1;
        }

        public static void WriteToFile(string fname, byte[] data)
        {
            FileStream fs = new FileStream(fname, FileMode.Create);

            using (BinaryWriter br = new BinaryWriter(fs))
            {
                br.Write(data);
            }
        }

        public static void WriteToFile(string fname, string data)
        {
            using (TextWriter tw = new StreamWriter(fname))
            {
                tw.WriteLine(data);
            }
        }

        public static byte[] ReadFromFile(string fname)
        {
            byte[] data;

            FileStream fs = new FileStream(fname, FileMode.Open);
            data = new byte[fs.Length];
            using (BinaryReader br = new BinaryReader(fs))
            {
                br.Read(data, 0, (int)fs.Length);
            }
            return data;
        }
        
        //public static string DecodeString(byte[] value)
        //{
        //    string result = string.Empty;
        //    if (value != null)
        //    {
        //        result = Encoding.GetEncoding("iso-8859-1").GetString(value);
        //        //result = encoding.GetString(value);
        //        //result = Encoding.UTF8.GetString(value);
        //    }
        //    return result;
        //}

        public static string DecodeString(byte[] value, Encoding encoding)
        {
            string result = string.Empty;
            if (value != null)
            {
                result = encoding.GetString(value);
            }
            return result;
        }

        public static byte[] MakeRightByteArray(byte[] arr)
        {
            byte[] result = new byte[arr.Length];

            arr.CopyTo(result, 0);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }
            return result;
        }

        public static int ByteArrayToInt(byte[] arr)
        {
            int result = 0;
            byte[] arrAsRightByteArray = MakeRightByteArray(arr);

            if (arrAsRightByteArray.Length == 1)
            {
                byte[] tmp = new byte[] { 0x00, arrAsRightByteArray[0] };
                result = BitConverter.ToUInt16(tmp, 0);
            }
            else if (arrAsRightByteArray.Length == 2)
            {
                result = BitConverter.ToUInt16(arrAsRightByteArray, 0);
            }
            else if (arrAsRightByteArray.Length == 3)
            {
                result = arrAsRightByteArray[2] << 16 | arrAsRightByteArray[1] << 8 | arrAsRightByteArray[0];
            }
            else
            {
                result = BitConverter.ToInt32(arrAsRightByteArray, 0);
            }
            return result;
        }

        public static string DecodeBinairy(byte[] value)
        {
            string result = string.Empty;
            if (value != null)
            {
                result = string.Format("{0}", Helper.ByteArrayToInt(value));
            }
            return result;
        }

        public static string GUnzip2(byte[] zipData)
        {
            using (MemoryStream ms = new MemoryStream(zipData))
            {
                using (GZipStream gzs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (StreamReader sr = new StreamReader(gzs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Calculate meldcode based on the vehicle identification number (vin).
        /// Meldcode contains last 4 numbers of the vin, padded left with X if less than 4 numbers in vin
        /// </summary>
        /// <param name="vin">Vehicle identification number</param>
        /// <returns>Meldcode, left padded with 'X' characters</returns>
        public static string ExtractMeldcode(string vin)
        {
            char[] meldcode = new char[4] { 'X', 'X', 'X', 'X' };
            int index = meldcode.Length - 1;
            for (int i = vin.Length - 1; i >= 0; i--)
            {
                if (index < 0)
                {
                    break;
                }

                if ("0123456789".IndexOf(vin[i]) != -1)
                {
                    meldcode[index--] = vin[i];
                }
            }

            return new string(meldcode);
        }

        public const int CRYPTUI_DISABLE_ADDTOSTORE = 0x00000010;

        public struct CRYPTUI_VIEWCERTIFICATE_STRUCT
        {
            public int dwSize;
            public IntPtr hwndParent;
            public int dwFlags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public String szTitle;
            public IntPtr pCertContext;
            public IntPtr rgszPurposes;
            public int cPurposes;
            public IntPtr pCryptProviderData; // or hWVTStateData
            public Boolean fpCryptProviderDataTrustedUsage;
            public int idxSigner;
            public int idxCert;
            public Boolean fCounterSigner;
            public int idxCounterSigner;
            public int cStores;
            public IntPtr rghStores;
            public int cPropSheetPages;
            public IntPtr rgPropSheetPages;
            public int nStartPage;
        }

     
        public static byte[] Decrypt(RSAParameters rsaP, byte[] encrypted)
        {
            BigInteger _Modulus = new BigInteger(new byte[] { 0x00 }.Concat(rsaP.Modulus).Reverse().ToArray());
            BigInteger _Exponent = new BigInteger(new byte[] { 0x00 }.Concat(rsaP.Exponent).Reverse().ToArray());

            /*
             * Add 00 byte to end of array for negative values
             */
            BigInteger encryptedAsBI = new BigInteger(new byte[] { 0x00 }.Concat(encrypted).ToArray().Reverse().ToArray());
            BigInteger decipheredAsBi = BigInteger.ModPow(encryptedAsBI, _Exponent, _Modulus);

            return decipheredAsBi.ToByteArray().Reverse().ToArray();
        }

        public static byte[] Decrypt(X509Certificate2 cert, byte[] encrypted)
        {
            RSACryptoServiceProvider key = cert.PublicKey.Key as RSACryptoServiceProvider;
            RSAParameters p = key.ExportParameters(false);

            return Decrypt(p, encrypted);
        }

        public static BigInteger Modulus(RSAParameters p)
        {
            return new BigInteger(new byte[] { 0x00 }.Concat(p.Modulus).Reverse().ToArray());
        }

        public static BigInteger Exponent(RSAParameters p)
        {
            return new BigInteger(new byte[] { 0x00 }.Concat(p.Exponent).Reverse().ToArray());
        }

        /// <summary>
        /// Read the asn.1 encoded SubjectPublicKeyInfo blob
        /// </summary>
        /// <param name="pubkey">A byte array representing the asn.1 encoded SubjectPublicKeyInfo</param>
        /// <returns>A RSAParameters with a modulus and an exponent available</returns>
        public static RSAParameters LoadRsaPublicKey(byte[] pubkey)
        {
            RSAParameters RSAKeyInfo = new RSAParameters();
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            BinaryReader binr = new BinaryReader(new MemoryStream(pubkey));
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                {
                    binr.ReadByte();    //advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    binr.ReadInt16();   //advance 2 bytes
                }
                else
                {
                    // Not public key byte array
                    return RSAKeyInfo;
                }

                seq = binr.ReadBytes(15);       //read the Sequence OID
                if (!Helper.CompareByteArrays(seq, SeqOID))    //make sure Sequence for OID is correct
                {
                    // Invalid object identifier
                    return RSAKeyInfo;
                }

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                {
                    binr.ReadByte();    //advance 1 byte
                }
                else if (twobytes == 0x8203)
                {
                    binr.ReadInt16();   //advance 2 bytes
                }
                else
                {
                    // Not public key byte array
                    return RSAKeyInfo;
                }

                bt = binr.ReadByte();
                if (bt != 0x00)
                {
                    //expect null byte next
                    return RSAKeyInfo;
                }

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) // data read as little endian order (actual data order for Sequence is 30 81)
                {
                    binr.ReadByte();    //advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    binr.ReadInt16();   //advance 2 bytes
                }
                else
                {
                    // Not public key byte array
                    return RSAKeyInfo;
                }

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102) // data read as little endian order (actual data order for Integer is 02 81)
                {
                    lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                }
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte(); // advance 2 bytes
                    lowbyte = binr.ReadByte();
                }
                else
                {
                    return RSAKeyInfo;
                }

                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   // reverse byte order since asn.1 key uses big endian order
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {
                    // if first byte (highest order) of modulus is zero, don't include it
                    binr.ReadByte();    // skip this null byte
                    modsize -= 1;   // reduce modulus buffer size by 1
                }

                byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                if (binr.ReadByte() != 0x02)            // expect an Integer for the exponent data
                {
                    return RSAKeyInfo;
                }

                int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                byte[] exponent = binr.ReadBytes(expbytes);

                RSAKeyInfo.Modulus = modulus;
                RSAKeyInfo.Exponent = exponent;

                return RSAKeyInfo;
            }
            catch (Exception)
            {
                return RSAKeyInfo;
            }

            finally
            {
                binr.Close();
            }
        }

        public static byte[] RandomByteArray(int length)
        {
            byte[] result = new byte[length];

            Random rnd = new Random();
            rnd.NextBytes(result);

            return result;
        }

        public static X509Extension GetX509Extension(X509Certificate2 cert, string OidValue)
        {
            foreach (var extension in cert.Extensions)
            {
                if (string.Compare(extension.Oid.Value, OidValue, true) == 0)
                {
                    // Found the AuthorityKeyIdentifier extension
                    return extension;
                }
            }
            return null;
        }

        public static string GetAuthorityKeyIdentifier(X509Certificate2 cert)
        {
            X509Extension extension = Helper.GetX509Extension(cert, "2.5.29.35");
            if (extension == null)
            {
                return null;
            }
            AsnEncodedData asndata = new AsnEncodedData(extension.Oid, extension.RawData);
            return asndata.Format(true);
        }
    }
}
