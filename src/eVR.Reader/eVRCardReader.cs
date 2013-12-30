/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

// -----------------------------------------------------------------------
// <copyright file="MTVCardReader.cs" company="RDW">
// RDW
// </copyright>
// -----------------------------------------------------------------------
namespace EVR.Reader
{
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using PCSC;
    using EVR.Utils;

    public class eVRCardReader
    {
        public static readonly byte[] eVRCApplicatie = new byte[] { 0xA0, 0x00, 0x00, 0x04, 0x56, 0x45, 0x56, 0x52, 0x2D, 0x30, 0x31 };
        private static TraceSource TS = new TraceSource("MTVCardReader");
        public bool DisplayError = false;
                
        public EFSOd EFSOd
        {
            get;
            private set;
        }

        public EFAA EFAA
        {
            get;
            private set;
        }      
        
        public RegistrationA RegistrationA
        {
            get;
            private set;
        }

        public RegistrationB RegistrationB
        {
            get;
            private set;
        }

        public RegistrationC RegistrationC
        {
            get;
            private set;
        }

        private CardReader cardReader
        {
            get;
            set;
        }

        private X509Certificate2 CSCA
        {
            get;
            set;
        }

        public void StopMonitor()
        {
            this.cardReader.StopMonitor();
        }

        public void StartMonitor()
        {
            this.cardReader.StartMonitor();
        }

        
        public eVRCardReader(X509Certificate2 CSCA, CardRemovedEvent removedEvent, CardInsertedEvent insertedEvent)
        {
            TS.TraceI("Constructing MTVCardReader object.");
            TS.TraceI("eVRCApplicatie = {0}", Helper.ByteArrayToString(eVRCApplicatie));
            if (CSCA != null)
            {
                this.CSCA = CSCA;
                TS.TraceV("CSCA Subject : \"{0}\".", CSCA.Subject);
                TS.TraceV("CSCA Effective date : \"{0}\".", CSCA.GetEffectiveDateString());
                TS.TraceV("CSCA Expiration date : \"{0}\".", CSCA.GetExpirationDateString());
            }
            this.cardReader = new CardReader(removedEvent, insertedEvent);
            TS.TraceI("MTVCardReader constructed.");
        }

        public void SelectReader(string readerName)
        {
            this.cardReader.SelectReader(readerName);
            TS.TraceV("Reader \"{0}\" selected.", readerName);
        }

        public bool CheckATR()
        {
            bool result = false;

            TS.TraceI("Start ATR check.");
            if (string.IsNullOrEmpty(this.cardReader.ReaderName))
            {
                throw new eVRCardReaderException("No reader selected.");
            }
            byte[] ATR = new byte[] { 0x3B, 0xD2, 0x18, 0x00, 0x81, 0x31, 0xFE, 0x45, 0x01, 0x01, 0xC1 };
            TS.TraceV("ATR check: {0}", Helper.ByteArrayToString(ATR));

            byte[] response = this.cardReader.GetATRString();
            TS.TraceV("ATR command response: {0}", Helper.ByteArrayToString(response));

            result = Helper.CompareByteArrays(response, ATR); 
            
            TS.TraceI("End ATR check, result: \"{0}\".", result);

            return result;
        }

        public bool CardManagerDisabled()
        {
            bool result = false;

            TS.TraceI("Start CardManagerDisabled check.");
            if (string.IsNullOrEmpty(this.cardReader.ReaderName))
            {
                throw new eVRCardReaderException("No reader selected.");
            }
            byte[] cmdSelectCardManager = new byte[] { 0x00, 0xA4, 0x04, 0x00, 0x08, 0xA0, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00 };
            TS.TraceV("SelectCardManager command: {0}", Helper.ByteArrayToString(cmdSelectCardManager));

            byte[] response = new byte[512];

            this.cardReader.Transmit(cmdSelectCardManager, ref response);
            TS.TraceV("SelectCardManager response: {0}", Helper.ByteArrayToString(response));

            result = (response.Length == 2 && response[0] == 0x6A && response[1] == 0x82);

            TS.TraceI("End CardManagerDisabled check, result: \"{0}\".", result);

            return (response.Length == 2 && response[0] == 0x6A && response[1] == 0x82);
        }

        public void Read()
        {
            TS.TraceI("Start reading EF data.");
            if (string.IsNullOrEmpty(this.cardReader.ReaderName))
            {
                throw new eVRCardReaderException("No reader selected.");
            }

            TS.TraceV("Reading EFSod.");
            try
            {
                this.EFSOd = new EFSOd(eVRCardReader.eVRCApplicatie, CSCA, this.cardReader);
            }
            catch (ElementaryFileException ex)
            {
                throw new eVRCardReaderException("Error reading EFSod.", ex);
            }
            TS.TraceV("EFSod read.");

            TS.TraceV("Reading EFAA.");
            try
            {
                this.EFAA = new EFAA(EFSOd, CSCA, eVRCardReader.eVRCApplicatie, this.cardReader);
            }
            catch (ElementaryFileException ex)
            {
                throw new eVRCardReaderException("Error reading EFAA.", ex);
            }

            TS.TraceV("Reading EFRegA.");
            try
            {
                this.RegistrationA = new RegistrationA(EFSOd, CSCA, eVRCardReader.eVRCApplicatie, this.cardReader);
            }
            catch (ElementaryFileException ex)
            {
                throw new eVRCardReaderException("Error reading EFRegA.", ex);
            }
            TS.TraceV("EFRegA read.");
            
            TS.TraceV("Reading EFRegB.");
            try
            {
                this.RegistrationB = new RegistrationB(EFSOd, CSCA, eVRCardReader.eVRCApplicatie, this.cardReader, this.RegistrationA.CharacterSetEncoding);
            }
            catch (ElementaryFileException ex)
            {
                throw new eVRCardReaderException("Error reading EFRegB.", ex);
            }
            TS.TraceV("EFRegB read.");

            TS.TraceV("Reading EFRegC.");
            try
            {
                this.RegistrationC = new RegistrationC(EFSOd, CSCA, eVRCardReader.eVRCApplicatie, this.cardReader, this.RegistrationA.CharacterSetEncoding);
            }
            catch (ElementaryFileException ex)
            {
                throw new eVRCardReaderException("Error reading EFRegC.", ex);
            }
            TS.TraceV("EFRegC read.");

            TS.TraceI("End reading EF data.");
        }

        public void Read(string ReaderName)
        {
            this.cardReader.SelectReader(ReaderName);
            this.Read();
        }
    }
}
