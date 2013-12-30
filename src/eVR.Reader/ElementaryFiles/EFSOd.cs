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
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using EVR.TLVParser;
    using EVR.Utils;
    using System;

    public class EFSOd : ElementaryFile
    {
        #region EFSOd_Properties
        public X509Certificate2 CSCA
        {
            get;
            set;
        }

        public X509Certificate2 DSCertificate
        {
            get;
            private set;
        }

        public TLV Signature
        {
            get;
            private set;
        }

        public TLV SignedAttrs
        {
            get;
            private set;
        }

        public TLV AttributeValue
        {
            get;
            private set;
        }

        public System.Security.Cryptography.Oid SignatureAlgorithm
        {
            get;
            private set;
        }

        public System.Security.Cryptography.Oid DigestAlgorithm
        {
            get;
            private set;
        }

        public TLV EContent
        {
            get;
            private set;
        }
        #endregion

        public override System.Text.Encoding CharacterSetEncoding
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public EFSOd(byte[] AID, X509Certificate2 CSCA, CardReader cardReader)
            : base(AID, cardReader, new byte[] { 0x00, 0x1D })
        {
            this.CSCA = CSCA;

            TLV tag = null;

            //// Haal DS certificaat uit EF.SOd data
            //// DS Certificaat bevindt zich in de EF.SOd data onder:
            //// T:'30' (ContentInfo)
            ////      T:'A0' (Content)
            ////          T:'30' (SignedData)
            ////              T:'A0' (Certificates)
            ////                  V: '30 XX XX ... XX'
            ////                      (DS certificaat begint met tag '30')
            tag = this.GetTag("1,30|1,A0|1,30|1,A0");
            if (tag != null)
            {
                DSCertificate = new X509Certificate2(tag.Value);
            }

            ////      i. Haal handtekening en algoritme uit EF.SOd
            ////         De handtekening bevindt zich in de EF.SOd data onder:
            ////         T:'30' (ContentInfo)
            ////              T:'A0' (Content)
            ////                   T:'30' (SignedData)
            ////                        T:'31' (SignerInfos)
            ////                             T:'30' (signerInfo)
            ////                                  T:'04' (Signature)
            Signature = this.GetTag("1,30|1,A0|1,30|2,31|1,30|1,04");

            //// Het signedAttrs veld bevindt zich in de EF.SOd data onder:
            //// T:'30' (ContentInfo)
            ////      T:'A0' (Content)
            ////           T:'30' (SignedData)
            ////                T:'30' (SignerInfo)
            ////                     T:'A0' (signedAttrs)
            SignedAttrs = this.GetTag("1,30|1,A0|1,30|2,31|1,30|1,A0");

            ////         Het gebruikte algoritme is te vinden in de EF.SOd onder:
            ////         T:'30' (ContentInfo)
            ////              T:'A0' (Content)
            ////                   T:'30' (SignedData)
            ////                        T:'31' (SignerInfos)
            ////                             T:'30' (SignerInfo)
            ////                                  T:''30' (SignatureAlgorithm)
            ////                                        T:'06' (algorithm)
            tag = this.GetTag("1,30|1,A0|1,30|2,31|1,30|3,30|1,06");
            if (tag != null)
            {
                SignatureAlgorithm = new System.Security.Cryptography.Oid(EVR.Utils.Oid.Decode(tag.Value));
            }

            ////    Het hash algoritme bevindt zich in EF.SOd onder:
            ////    T:'30' (ContentInfo)
            ////         T:'A0' (Content)
            ////              T:'30' (SignedData)
            ////                   T:'31' (SignerInfos)
            ////                        T:'30' (SignerInfo)
            ////                             T:'30' (digestAlgorithm)
            ////                                  T:'06' (algorithm)
            tag = this.GetTag("1,30|1,A0|1,30|2,31|1,30|2,30|1,06");
            if (tag != null)
            {
                DigestAlgorithm = new System.Security.Cryptography.Oid(EVR.Utils.Oid.Decode(tag.Value));
            }

            //// vi Haal AttributeValue uit signedAttrs
            ////    Binnen signedAttrs bevindt de AttributeValue zich onder:
            ////    T:'30' (Attribute)
            ////         T:'31' (AttrValues)
            ////              T:'04' (AttributeValue)
            AttributeValue = this.GetTag("1,30|1,A0|1,30|2,31|1,30|1,A0|2,30|1,31|1,04");

            //// vii Haal eContent (=RDWidsSecurityObject) uit EF.SOd
            ////     eContent bevindt zich onder:
            ////     T:'30' (ContentInfo)
            ////          T:'A0' (Content)
            ////               T:'30' (SignedData)
            ////                    T:'30' (encapContentInfo)
            ////                         T:'A0' (explicit content)
            ////                              T:'04' (eContent)
            EContent = this.GetTag("1,30|1,A0|1,30|1,30|1,A0|1,04");
        }

        public bool IsValid
        {
            get
            {
                /*
                 * Constructor voert volgende stappen uit:
                 * 1. Selecteer eVRC applicatie (AID = ‘A0 00 00 04 56 45 56 52 2D 30 31’)
                 * 2. Passive Authentication (1 e deel)
                 *      a. Selecteer EF.SOd
                 *      b. Lees EF.SOd
                 */

                /*
                 * c. Controleer DS certificaat uit EF.SOd
                 *      i.   Haal DS certificaat uit EF.SOd data => zie constructor
                 *      ii.  DS Certificaat profiel voldoen aan CP/CS
                 *           - Niet geimplementeerd
                 *      iii. DS certificaat is niet verlopen
                 *           - Wel geimplementeerd
                 *      iv.  DS certificaat komt niet voor op CRL
                 *           - Niet geimplementeerd
                 *      v.   DS certificaat is uitgegeven door CSCA (controle tegen CSCA certificaat)
                 */
                if (this.CSCA == null)
                {
                    //// Validation cannot be peformed when CSCA is null
                    return false;
                }

                if (!Helper.KeyUsageIsDigitalSignatureOnly(this.DSCertificate))
                {
                    return false;
                }

                ////     iii. DS Certificaat is niet verlopen
                ////          Certifcaat valid to date ligt na huidige datum
                if (DateTime.Now > this.DSCertificate.NotAfter || DateTime.Now < this.DSCertificate.NotBefore)
                {
                   return false;
                }

                if (string.Compare(Helper.GetAuthorityKeyIdentifier(this.CSCA), Helper.GetAuthorityKeyIdentifier(this.DSCertificate)) != 0)
                {
                    return false;
                }

                /*
                 * d. Controleer handtekening uit EF.SOd met public key uit DS certificaat
                 *      i. Haal handtekening en algoritme uit EF.SOd
                 *          Handtekening => zie constructor
                 */
                HashAlgorithm hashAlgoritm = this.RDWIdsSecurityObjectHashAlgorithm;
                /*          
                 *      ii.  'Controleer’ / ‘Ontcijfer’ met DS public key
                 *           Door  ‘controle’ / ‘ontcijfering’ van de handtekening met de DS public key uit
                 *           het DS certificaat volgt het signedAttrs TLV veld met dien verstande dat dit
                 *           veld begint met tag ‘31’ i.p.v. met tag ‘A0’.
                 *      iii. Sla signedAttrs uit handtekening op in geheugen 
                 *           Sla signedAttrs op in geheugen na de begintag vervangen te hebben door ‘A0’.
                 */
                byte[] desciphered = Helper.Decrypt(this.DSCertificate, Signature.Value).Reverse().Take(32).Reverse().ToArray();
                /*
                 *      iv. Haal signedAttrs uit EF.SOd => zie constructor
                 */
                byte[] adjSignedAttr = new byte[] { 0x31, (byte)SignedAttrs.Length }.Concat(SignedAttrs.Value).ToArray();

                /*
                 *      v.  Vergelijk signedAttrs uit EF.SOd met signedAttrs uit handtekening 
                 *          (geheugen)
                 */
                byte[] hashedAdhSignedAttr = hashAlgoritm.ComputeHash(adjSignedAttr);

                if (!Helper.CompareByteArrays(desciphered, hashAlgoritm.ComputeHash(adjSignedAttr)))
                {
                    return false;
                }
                return true;
            }
        }

        public bool PassiveAuthentication
        {
            get
            {
                /*
                 * 2. Passive Authentication (2e deel)
                 *      vi. Haal AttributeValue uit signedAttrs
                 *              Zie constructor
                 *      vii. Haal eContent (=RDWIdsSecurityObject) uit EF.SOd
                 *              Zie constuctor
                 */
                HashAlgorithm hashAlgorithm = HashAlgorithm.Create(this.DigestAlgorithm.FriendlyName);
                /*
                 *      viii. Bereken hash over eContent
                 */
                byte[] hashedEContent = hashAlgorithm.ComputeHash(this.EContent.Value);
                /*
                 *      ix. Vergelijk hashes
                 *      Vergelijk hash over eContents met hash uit vi. (AttributeValue uit signedAttrs).
                 *      Indien deze 2 overeenkomen is het eerste deel van PA geslaagd.
                 */
                return EVR.Utils.Helper.CompareByteArrays(this.AttributeValue.Value, hashedEContent);
            }
        }

        //// Het hash algoritme bevindt zich in EF.SOd onder:
        ////  T:'30' (RDWIdsSecurityObject)
        ////       T:'30' (hashAlgorithm)
        ////            T:'06' (algorithm)
        public HashAlgorithm RDWIdsSecurityObjectHashAlgorithm
        {
            get
            {
                System.Security.Cryptography.Oid oid = new System.Security.Cryptography.Oid(EVR.Utils.Oid.Decode(this.GetTag("1,30|1,A0|1,30|1,30|1,A0|1,04|1,30|1,30|1,06").Value));
                return HashAlgorithm.Create(oid.FriendlyName);
            }
        }

        public byte[] GetDataGroupHashValue(byte[] dataGroupFileIdentifier)
        {
            TLV dataGroupHashValues = this.GetTag("1,30|1,A0|1,30|1,30|1,A0|1,04|1,30|2,30");

            for (int i = 0; i < dataGroupHashValues.Childs.Count; i++)
            {
                if (EVR.Utils.Helper.CompareByteArrays(dataGroupHashValues.Childs[i].Childs[0].Value, dataGroupFileIdentifier))
                {
                    return dataGroupHashValues.Childs[i].Childs[1].Value;
                }
            }
            return null;
        }
    }
}
