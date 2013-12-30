//// -----------------------------------------------------------------------
//// <copyright file="CardReader.cs" company="RDW">
//// RDW
//// </copyright>
//// -----------------------------------------------------------------------
namespace EVR.Reader
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using PCSC;
    using PCSC.Iso7816;
    using EVR.TLVParser;
    using EVR.Utils;

    public class CardReader
    {
        private static TraceSource TS = new TraceSource("CardReader");

        private SCardReader crdReader = null;
        private static SCardContext _systemCardContext = null;
        private static SCardMonitor _monitor = null;
        string[] readerNames = null;

        public CardReader(CardRemovedEvent removedEvent, CardInsertedEvent insertedEvent)
        {
            TS.TraceI("Constructing CardReader object.");
            _systemCardContext = OpenSystemWideCardContext();

            if (removedEvent != null || insertedEvent != null)
            {
                _monitor = new SCardMonitor(_systemCardContext);
                if (removedEvent != null)
                {
                    TS.TraceV("Monitoring removedEvent");
                    _monitor.CardRemoved += new CardRemovedEvent(removedEvent);
                }
                if (insertedEvent != null)
                {
                    TS.TraceV("Monitoring insertedEvent");
                    _monitor.CardInserted += new CardInsertedEvent(insertedEvent);
                }

                readerNames = GetReaderNames();

                foreach (string s in readerNames)
                {
                    TS.TraceV("Reader detected: \"{0}\".", s);
                }

                this.StartMonitor();
                TS.TraceI("Monitor started.");
            }
            TS.TraceI("CardReader object constructed.");
        }

        public string ReaderName
        {
            get;
            set;
        }

        public void StopMonitor()
        {
            if (_monitor != null)
            {
                _monitor.Cancel();
                TS.TraceI("Monitoring readers stopped.");
            }
        }

        public void StartMonitor()
        {
            if ((_monitor != null) && (readerNames != null) && (readerNames.Length > 0))
            {
                _monitor.Start(readerNames);
                TS.TraceI("Monitoring readers started.");
            }
        }

        public byte[] GetATRString()
        {
            byte[] attr;

            this.crdReader.GetAttrib(SCardAttr.ATRString, out attr);
            return attr;
        }

        public byte[] ReadFile(byte[] AID, byte[] FileID)
        {
            TS.TraceI("Readfile with AID \"{0}\" and FileID \"{1}\".", Helper.ByteArrayToString(AID), Helper.ByteArrayToString(FileID));
            SelectApplet(AID);

            if (FileID.Length != 0x02)
            {
                throw new ArgumentException("Invalid length FileID", "FileID");
            }

            if (this.crdReader == null)
            {
                throw new CardReaderException("No valid reader available");
            }

            byte[] pbRecvBuffer = new byte[256];

            //// Send SELECT File
            byte[] selectFile = new byte[] { 0x00, 0xA4, 0x02, 0x04, 0x02, FileID[0], FileID[1], 0x00 };
            TS.TraceV("Select file with command: \"{0}\".", Helper.ByteArrayToString(selectFile));
            /*
                00 = Class
                A4 = Instructie
                02 = P1 (select EF under current DF)
                04 = P2 (return FCP data)
                02 = Lc
                XX = Data
                XX = Data
                00 = Le
            */
            SCardError err = this.crdReader.Transmit(selectFile, ref pbRecvBuffer);
            if (err != SCardError.Success)
            {
                throw new CardReaderException(err, SCardHelper.StringifyError(err));
            }

            ResponseApdu resp = new ResponseApdu(pbRecvBuffer, IsoCase.Case4Extended, this.crdReader.ActiveProtocol);
            if ((resp.SW1 != 0x90) && (resp.SW2 != 0x00))
            {
                return null;
            }

            byte[] fileInfo = resp.GetData();

            if (fileInfo == null)
            {
                throw new CardReaderException(string.Format(
                    "No data available reading FileID \"{0}\" with AID \"{1}\".", Helper.ByteArrayToString(FileID), Helper.ByteArrayToString(AID)));
            }

            TLVList myanswer = TLV.Parse(new MemoryStream(fileInfo), true);

            TLV fileLengthTLV = myanswer.getTag("1,62|1,80");
            if (fileLengthTLV == null)
            {
                throw new CardReaderException("Missing tag: 1,62|1,80");
            }
            
            int fileLength = Helper.ByteArrayToInt(fileLengthTLV.Value);
            TS.TraceV("File length = \"{0}\".", fileLength);

            //// Read the remaining bytes for this file
            byte[] fileData = new byte[fileLength];
            int bytesRead = 0;
            const int blockSize = 255;

            TS.TraceV("Start reading file with blocksize \"{0}\".", blockSize);
            while (bytesRead < fileLength)
            {
                int lngth = blockSize;
                if (fileLength - bytesRead < blockSize)
                {
                    //// Cannot read an entire block anymore; adjust length of data to read
                    lngth = fileLength - bytesRead;
                    if (lngth == 0)
                    {
                        break;
                    }
                }

                byte[] nextBlock = ReadFileNextBlock(bytesRead, lngth);
                Buffer.BlockCopy(nextBlock, 0, fileData, bytesRead, nextBlock.Length);
                bytesRead += nextBlock.Length;

                TS.TraceV("\"{0}\" bytes read.", bytesRead);
            }
            TS.TraceI("File read.");
            return fileData;
        }

        public bool Transmit(byte[] sendBuffer, ref byte[] recvBuffer)
        {
            TS.TraceV("Transmit sendBuffer: \"{0}\".", Helper.ByteArrayToString(sendBuffer));

            return crdReader.Transmit(sendBuffer, ref recvBuffer) == SCardError.Success;
        }

        public string[] GetReaderNames()
        {
            SCardContext context = OpenSystemWideCardContext();
            string[] szReaders = context.GetReaders();

            return szReaders;
        }

        public void SelectReader(string readerName)
        {
                this.ReaderName = readerName;
                this.crdReader = GetReader();                   
        }
        ///// <summary>
        ///// Select a special applet based on its AID (Application IDentifier) with a length of 5 bytes.
        ///// </summary>
        ///// <param name="AID">Application Identifier. A 5 byte array that represents the applet to select</param>
        public void SelectSpecialApplet(byte[] AID)
        {
            TS.TraceI("Start SelectApplet with ID \"{0}\".", Helper.ByteArrayToString(AID));
            this.crdReader = GetReader();

            byte[] pbRecvBuffer = new byte[256];

            if (AID.Length != 0x05)
            {
                throw new ArgumentException("Invalid length AID", "AID");
            }

            //// Send SELECT Applet command        
            byte[] selectApplet = new byte[] { 0x00, 0xA4, 0x04, 0x00, 0x05, AID[0], AID[1], AID[2], AID[3], AID[4], 0x00 };
            TS.TraceV("Select applet command: \"{0}\".", Helper.ByteArrayToString(selectApplet));

            /////*
            //// 00 = Class
            //// A4 = Instructie
            //// 04 = P1 = Select Dedicated File (DF)
            //// 00 = P2 = File Control Information (FCI)
            //// 0B = lc = Lengte van de AID
            //// AID
            //// 00 = End
            ////*/
            SCardError err = crdReader.Transmit(selectApplet, ref pbRecvBuffer);
            if (err != SCardError.Success)
            {
                throw new CardReaderException(err, SCardHelper.StringifyError(err));
            }

            ResponseApdu resp = new ResponseApdu(pbRecvBuffer, IsoCase.Case4Extended, this.crdReader.ActiveProtocol);

            if ((resp.SW1 == 0x6A) && (resp.SW2 == 0x82))
            {
                throw new CardReaderException("Applet not found");
            }

            if ((resp.SW1 != 0x62) && (resp.SW2 != 0x83))
               {
                throw new CardReaderException("Invalid response");
               }

            /*if ((resp.SW1 != 0x90) && (resp.SW2 != 0x00))
            {
                throw new CardReaderException("Invalid response");
            }
            */
            TS.TraceI("End SelectApplet.");         
        }


        ///// <summary>
        ///// Select an applet based on its AID (Application IDentifier). As for now the AID should have a length of 11 bytes.
        ///// </summary>
        ///// <param name="AID">Application Identifier. An 11 byte array that represents the applet to select</param>
        private void SelectApplet(byte[] AID)
        {
            TS.TraceI("Start SelectApplet with ID \"{0}\".", Helper.ByteArrayToString(AID));
            this.crdReader = GetReader();

            byte[] pbRecvBuffer = new byte[256];

            if (AID.Length != 0x0B)
            {
                throw new ArgumentException("Invalid length AID", "AID");
            }

            //// Send SELECT Applet command        
            byte[] selectApplet = new byte[] { 0x00, 0xA4, 0x04, 0x00, 0x0B, AID[0], AID[1], AID[2], AID[3], AID[4], AID[5], AID[6], AID[7], AID[8], AID[9], AID[10], 0x00 };
            TS.TraceV("Select applet command: \"{0}\".", Helper.ByteArrayToString(selectApplet));

            /////*
            //// 00 = Class
            //// A4 = Instructie
            //// 04 = P1 = Select Dedicated File (DF)
            //// 00 = P2 = File Control Information (FCI)
            //// 0B = lc = Lengte van de AID
            //// AID
            //// 00 = End
            ////*/
            SCardError err = crdReader.Transmit(selectApplet, ref pbRecvBuffer);
            if (err != SCardError.Success)
            {
                throw new CardReaderException(err, SCardHelper.StringifyError(err));
            }

            ResponseApdu resp = new ResponseApdu(pbRecvBuffer, IsoCase.Case4Extended, this.crdReader.ActiveProtocol);
            if ((resp.SW1 != 0x90) && (resp.SW2 != 0x00))
            {
                throw new CardReaderException("Invalid response");
            }

            TS.TraceI("End SelectApplet.");
        }

        private SCardContext OpenSystemWideCardContext()
        {
            TS.TraceV("Start OpenSystemWideCardContext.");
            
            if (_systemCardContext == null)
            {
                _systemCardContext = new SCardContext();
                _systemCardContext.Establish(SCardScope.System);
                TS.TraceV("New SystemWideCardContext created.");
            }

            TS.TraceV("End OpenSystemWideCardContext.");

            return _systemCardContext;
        }

        private byte[] ReadFileNextBlock(int offSet, int lenght)
        {
            TS.TraceV("Start ReadFileNextBlock, offset = \"{0}\", length = \"{1}\".", offSet, lenght);
            byte[] pbRecvBuffer = new byte[lenght + 2]; //// Last 2 bytes are sw1 + sw2
            byte[] offSetArr = MTVReader.Converter.IntToByteArray(offSet, 2);
            byte[] lenghtArr = MTVReader.Converter.IntToByteArray(lenght, 1);
            byte[] readFile = new byte[] { 0x00, 0xB0, offSetArr[0], offSetArr[1], lenghtArr[0] };

            SCardError err = this.crdReader.Transmit(readFile, ref pbRecvBuffer);
            if (err != SCardError.Success)
            {
                throw new CardReaderException(err, SCardHelper.StringifyError(err));
            }

            ResponseApdu resp = new ResponseApdu(pbRecvBuffer, IsoCase.Case4Extended, this.crdReader.ActiveProtocol);
            if ((resp.SW1 != 0x90) && (resp.SW2 != 0x00))
            {
                throw new CardReaderException(string.Format("Error reading next block for file at offset {0}, length {1}", offSet, lenght));
            }
            TS.TraceV("End ReadFileNextBlock.");
            return resp.GetData();
        }

        private SCardReader GetReader()
        {
            TS.TraceI("Start GetReader.");
            //// Establish SCard context
            SCardContext hContext = OpenSystemWideCardContext();

            //// Create a reader object using the existing context
            SCardReader reader = new SCardReader(hContext);

            //// Connect to the card
            SCardError err = reader.Connect(this.ReaderName, SCardShareMode.Shared, SCardProtocol.T0 | SCardProtocol.T1);
            if (err != SCardError.Success)
            {
                throw new CardReaderException(err, SCardHelper.StringifyError(err));
            }

            if (reader.ActiveProtocol != SCardProtocol.T0 && reader.ActiveProtocol != SCardProtocol.T1)
            {
                throw new CardReaderException(SCardError.ProtocolMismatch, "Protocol not supported: " + reader.ActiveProtocol.ToString());
            }
            TS.TraceV("AciveProtocol: \"{0}\".", reader.ActiveProtocol);

            TS.TraceI("End GetReader.");
            return reader;
        }
    }
}
