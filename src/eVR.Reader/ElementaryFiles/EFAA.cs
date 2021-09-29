/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

namespace EVR.Reader
{
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using EVR.Utils;

    public class EFAA : Registration
    {
        public byte[] ActiveAuthenticationPublicKeyInfo
        {
            get
            {
                return this.GetTag("1,6F").Value;
            }
        }

        ///// <summary>
        ///// Test the active authentication of the card
        ///// </summary>
        ///// <returns></returns>
        public bool ActiveAuthentication
        {
            get
            {
                /*
                 * Constructor voert volgende stappen uit:
                 * 3. Active Authentication
                 *      a. Selecteer EF.AA
                 *      b. Lees EF.AA
                 *      c. Controleer authenticiteit EF.AA (Passive Authentication (2 e deel))
                
                if (!this.PassiveAuthentication)
                {
                    return false;
                }
                */
                /*
                 * d. Sla AA public key uit EF.AA op in geheugen
                 *    De AA public key bevind zich in EF.AA onder:
                 *    T:'6F' (ActiveAuthenticationPublicKeyInfo)
                 *         T:'30' (SubjectPublicKeyInfo)
                 *              T:'03' (subjectPublicKey)
                 *    Het algoritme is te vinden in EF.AA onder:
                 *    T:'6F' (ActiveAuthenticationPublicKeyInfo)
                 *         T:'30' (SubjectPublicKeyInfo)
                 *              T:'30' (AlogrithmIdentifier)
                 *                   T:'06' (algorithm)
                 *                   (rsaEncryption = 1.2.840.113549.1.1.1)
                 */
                RSAParameters AAPubKey = Helper.LoadRsaPublicKey(this.ActiveAuthenticationPublicKeyInfo);
                /*
                 * e. Genereer 8-byte challenge (RND.IFD)
                 *    De uitlees- en verificatiesoftware genereert een random 8 bytes
                 */
                byte[] RND = Helper.RandomByteArray(8);
                byte[] internalAuthent = new byte[] { 0x00, 0x88, 0x00, 0x00, 0x08 }.Concat(Helper.RandomByteArray(8)).Concat(new byte[] { 0x00 }).ToArray();

                //// f. Chip authenticatie
                ////    Stuur 8-byte challenge (RND.IFD) naar kaart via Internal Authenticate commando
                byte[] pbRecvBuffer = new byte[258];
                if (!this.Transmit(internalAuthent, ref pbRecvBuffer))
                {
                    ////throw new Exception("Fout bij versturen Internal Authenticate commando.");
                    return false;
                }

                //// g. Controleer response met behulp van AA public key
                ////    Gebruik de AA public key uit EF.AA (opgeslagen in geheugen) om de data uit de response
                ////    op het Internal Authenticate commando (dus zonder de status words '90 00') te 
                ////    ontcijferen / controleren met behulp van het algoritme uit stap d. (RSA)
                byte[] deciphered = Helper.Decrypt(AAPubKey, new byte[] { 0x00 }.Concat(pbRecvBuffer.Take(pbRecvBuffer.Length - 2)).ToArray());

                ////    Dit levert een string op die moet bestaan uit:
                ////    '6A' ++ M1 ++ 32 ++ hash (m1 ++ RND.IFD) ++ 34 CC'

                ////    Haal M1 uit de response (houd rekening met variabele key lengte door te kijken naar de bit length)
                int _ModulesBitLength = (int)Math.Ceiling(BigInteger.Log(Helper.Modulus(AAPubKey), 2));
                byte[] M1 = new byte[_ModulesBitLength / 8 - 35 + RND.Length];
                Array.Copy(deciphered, 1, M1, 0, deciphered.Length - 35);
                Array.Copy(RND, 0, M1, deciphered.Length - 35, RND.Length);

                ////    Bereken met behulp van sha256 de hash over M1 ++ RND.IFD
                HashAlgorithm hashAlgoritm = HashAlgorithm.Create("SHA256");
                byte[] hashedM1 = hashAlgoritm.ComputeHash(M1);
                //// Bereken de hash waarde van de response
                byte[] hashDeciphered = new byte[hashAlgoritm.HashSize / 8];
                Array.Copy(deciphered, deciphered.Length - 34, hashDeciphered, 0, 32);

                ////    Vergelijk deze hash met de hash-waarde uit de response
                ////    Indien de 2 overeenkomen is Active Authentication van de chip geslaagd
                if (!Helper.CompareByteArrays(hashDeciphered, hashedM1))
                {
                    return false;
                }
                return true;
            }
        }

        public override void CreateSignature()
        {
            this.Signature = null;
        }

        public override void CreateDocumentSigner()
        {
            this.DS = null;
        }

        public override System.Text.Encoding CharacterSetEncoding
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public EFAA(EFSOd EFSOd, X509Certificate2 CSCA, byte[] AID, CardReader cardReader)
            : base(EFSOd, CSCA, AID, cardReader, new byte[] { 0x00, 0x0D })
        {
        }
    }
}
