using BerTlv;
using global::PCSC;
using global::PCSC.Iso7816;
using global::PCSC.Monitoring;
using Microsoft.Extensions.Logging;

namespace eVR.Reader.PCSC
{
    /// <summary>
    /// Class used to read data from a smart card
    /// </summary>
    /// <param name="logger">A logger</param>
    /// <param name="reader">A smart card reader</param>
    /// <param name="context">An application context to the PC/SC Resource Manager</param>
    /// <param name="monitor">A monitor for card reader events/triggers</param>
    public class CardReader(
          ILogger<CardReader> logger
        , ISCardReader reader
        , ISCardContext context
        , ISCardMonitor monitor)
        : IReader
    {
        #region Properties

        // needed for IDisposable
        private bool _disposedValue;
        private const int _blockSize = 255;

        #endregion

        #region Interface - IReader     

        /// <summary>
        /// Add an event handler to the CardInserted event
        /// </summary>
        /// <param name="event">The event handler</param>
        public void SetCardInsertedEvent(CardInsertedEvent @event)
        {
            logger.LogTrace($"Set monitoring insertedEvent");
            monitor.CardInserted += @event;
        }

        /// <summary>
        /// Remove an event handler from the CardInserted event
        /// </summary>
        /// <param name="event">The event handler</param>
        public void RemoveCardInsertedEvent(CardInsertedEvent @event)
        {
            logger.LogTrace($"Remove monitoring insertedEvent");
            monitor.CardInserted -= @event;
        }

        /// <summary>
        /// Select an application by its AID
        /// </summary>
        /// <param name="aID">The Application Identifier</param>
        /// <param name="checkStatusBytes">An indication whether the status bytes should be checked</param>
        /// <returns></returns>
        public async Task<ResponseApdu?> SelectApplication(byte[] aID, bool checkStatusBytes = true)
        {
            return await Task.Run(() =>
            {
                logger.LogInformation("Start SelectApplication with AID:{AID}", Helper.ToHexWithSpaces(aID));

                byte[] pbRecvBuffer = new byte[256];
                var command = new CommandApdu(IsoCase.Case4Short, reader.ActiveProtocol)
                {
                    CLA = 0x00,
                    Instruction = InstructionCode.SelectFile,
                    P1 = 0x04,
                    P2 = 0x00,
                    Data = aID,
                };

                Transmit(command.ToArray(), ref pbRecvBuffer);

                var response = new ResponseApdu(pbRecvBuffer, IsoCase.Case4Extended, reader.ActiveProtocol);
                if (checkStatusBytes && response.SW1 != 0x90 && response.SW2 != 0x00)
                {
                    return null;
                }

                logger.LogInformation("End SelectApplication");

                return response;
            });
        }

        /// <summary>
        /// Select a Master File (MF)
        /// </summary>
        /// <returns></returns>
        public async Task SelectMF() 
        {
            await Task.Run(() =>
            {
                byte[] pbRecvBuffer = new byte[256];

                var command = new CommandApdu(IsoCase.Case4Short, reader.ActiveProtocol)
                {
                    CLA = 0x00,
                    Instruction = InstructionCode.SelectFile,
                    P1 = 0x04,
                    P2 = 0x00,
                    Data = [],
                    Le = 0x00,
                };
                Transmit(command.ToArray(), ref pbRecvBuffer);
            });
        }

        /// <summary>
        /// Read an Elementary File (EF)
        /// </summary>
        /// <param name="aID">The application identifier</param>
        /// <param name="fileID">The file identifier</param>
        /// <returns>A byte array with the conent of the file</returns>
        /// <exception cref="CardReaderException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<byte[]> ReadElementaryFile(byte[] aID, byte[] fileID)
        {
            logger.LogInformation("Start ReadElementaryFile with AID {AID}: and FileID: {FileID}", Helper.ToHexWithSpaces(aID), Helper.ToHexWithSpaces(fileID));

            _ = await SelectApplication(aID) ?? throw new CardReaderException("Invalid response");
            if (fileID.Length != 0x02)
            {
                throw new ArgumentException("Invalid length FileID", nameof(fileID));
            }

            byte[] pbRecvBuffer = new byte[256];

            var command = new CommandApdu(IsoCase.Case4Short, reader.ActiveProtocol)
            {
                CLA = 0x00,
                Instruction = InstructionCode.SelectFile,
                P1 = 0x02,
                P2 = 0x04,
                Data = fileID,
            };
            Transmit(command.ToArray(), ref pbRecvBuffer);

            var response = new ResponseApdu(pbRecvBuffer, IsoCase.Case4Extended, reader.ActiveProtocol);
            if (response.SW1 != 0x90 &&
                response.SW2 != 0x00)
            {
                return [];
            }

            _ = response.GetData() ?? throw new CardReaderException($"No data available reading AID = AID {Helper.ToHexWithSpaces(aID)} with FileID = {Helper.ToHexWithSpaces(fileID)}");
            var fileControlParameters = Tlv.Parse(response.GetData());
            var fileLengthTLV = (fileControlParameters
                .FirstOrDefault(e => e.HexTag == "62")?.Children
                .FirstOrDefault(c => c.HexTag == "80")?.Value) ?? throw new CardReaderException("Missing tag: 62|80");
            var fileLength = Helper.ByteArrayToInt(fileLengthTLV);

            // Read the remaining bytes for this file
            byte[] fileData = new byte[fileLength];
            int bytesRead = 0;

            while (bytesRead < fileLength)
            {
                int lngth = _blockSize;
                if (fileLength - bytesRead < _blockSize)
                {
                    // Cannot read an entire block anymore; adjust length of data to read
                    lngth = fileLength - bytesRead;
                    if (lngth == 0)
                    {
                        break;
                    }
                }
                byte[] nextBlock = ReadFileNextBlock(bytesRead, lngth);
                Buffer.BlockCopy(nextBlock, 0, fileData, bytesRead, nextBlock.Length);
                bytesRead += nextBlock.Length;
            }
            logger.LogInformation("End ReadElementaryFile");

            return fileData;
        }

        /// <summary>
        /// Get the names of all cardreaders on the system.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetReaderNames()
        {
            var readerNames = context.GetReaders();
            foreach (var readerName in readerNames)
            {
                logger.LogTrace("Found reader with name: {readerName}", readerName);
            }
            return readerNames;
        }

        /// <summary>
        /// Start monitoring the card readers.
        /// </summary>
        /// <param name="readerNames">The names of the cardreaders to be monitored</param>
        public void StartMonitor(IEnumerable<string> readerNames)
        {
            foreach (var readerName in readerNames)
            {
                logger.LogTrace("Monitoring started for reader: {readerName}", readerName);
            }
            monitor?.Start(readerNames.ToArray());
        }

        /// <summary>
        /// Stop monitoring the cardreaders.
        /// </summary>
        public void StopMonitor()
        {
            logger.LogTrace($"Monitoring readers stopped");

            monitor?.Cancel();
        }

        /// <summary>
        /// Connect a specific cardreader
        /// </summary>
        /// <param name="readerName">The name of the cardreader</param>
        /// <returns></returns>
        /// <exception cref="CardReaderException"></exception>
        public Task ConnectReader(string readerName)
        {
            return Task.Run(() =>
            {
                reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.T0 | SCardProtocol.T1);
                if (reader.ActiveProtocol != SCardProtocol.T0 &&
                    reader.ActiveProtocol != SCardProtocol.T1)
                {
                    throw new CardReaderException(SCardError.ProtocolMismatch, "Protocol not supported: " + reader.ActiveProtocol.ToString());
                }
                logger.LogTrace("Connected to reader {readerName} with protocol(s) {activeProtocol}", readerName, reader.ActiveProtocol);
            });
        }

        /// <summary>
        /// Transmit data to the card.
        /// </summary>
        /// <param name="sendBuffer">The data to be sent</param>
        /// <param name="recvBuffer">The data to be received</param>
        /// <returns></returns>
        public bool Transmit(byte[] sendBuffer, ref byte[] recvBuffer)
        {
            logger.LogInformation($"Start Transmit");

            var response = reader.Transmit(sendBuffer, ref recvBuffer);

            logger.LogInformation($"End Transmit");

            return response == SCardError.Success;

        }

        /// <summary>
        /// Get the ATR string of the card.
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetATRString()
        {
            return Task.Run(() =>
            {
                logger.LogInformation($"Start GetATRString");

                reader.GetAttrib(SCardAttribute.AtrString, out byte[] attr);

                logger.LogInformation($"End GetATRString");

                return attr;
            });
        }

        /// <summary>
        /// Execute Internal Authentication
        /// </summary>
        /// <param name="random">A random byte array</param>
        /// <returns></returns>
        /// <exception cref="CardReaderException"></exception>
        public Task<byte[]> InternalAuthenticate(byte[] random)
        {
            return Task.Run(() =>
            {
                var command = new CommandApdu(IsoCase.Case4Short, reader.ActiveProtocol)
                {
                    CLA = 0x00,
                    Instruction = (InstructionCode)0x88,
                    P1 = 0x00,
                    P2 = 0x00,
                    Data = random
                };

                byte[] pbRecvBuffer = new byte[1024];
                if (!this.Transmit(command.ToArray(), ref pbRecvBuffer))
                {
                    throw new CardReaderException("APDU command INTERNAL_AUTHENTICATE failed");
                }
                var response = new ResponseApdu(pbRecvBuffer, IsoCase.Case4Short, reader.ActiveProtocol);
                if (response.SW1 != 0x90 &&
                    response.SW2 != 0x00)
                {
                    throw new CardReaderException($"APDU command INTERNAL_AUTHENTICATE - invalid status bytes: {response.SW1:X2} {response.SW2:X2}");
                }
                return response.GetData();
            });
        }



        

        

        #endregion

        #region Private Methods           

        /// <summary>
        /// Read a part of the data of a file
        /// </summary>
        /// <param name="offSet">The position where to start reading this part of the data</param>
        /// <param name="lenght">The length of this part of the data</param>
        /// <returns></returns>
        /// <exception cref="CardReaderException"></exception>
        private byte[] ReadFileNextBlock(int offSet, int lenght)
        {
            logger.LogTrace("Start ReadFileNextBlock with offset: {offSet} and length: {lenght}", offSet, lenght);

            var pbRecvBuffer = new byte[lenght + 2]; // Last 2 bytes are sw1 + sw2
            var offSetArr = Helper.IntToByteArray(offSet, 2);
            var lenghtArr = Helper.IntToByteArray(lenght, 1);
            var readFile = new byte[] { 0x00, 0xB0, offSetArr[0], offSetArr[1], lenghtArr[0] };

            Transmit(readFile, ref pbRecvBuffer);

            var response = new ResponseApdu(pbRecvBuffer, IsoCase.Case4Extended, reader.ActiveProtocol);
            if (response.SW1 != 0x90 &&
                response.SW2 != 0x00)
            {
                throw new CardReaderException($"Error reading next block for file at offset {offSet} and length {lenght}");
            }

            logger.LogTrace("End ReadFileNextBlock");

            return response.GetData();
        }

        #endregion

        #region Interface - IDisposable

        /// <summary>
        /// Dispose the CardReader and its dependencies.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    monitor.Dispose();
                    reader.Dispose();
                    context.Dispose();
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose the CardReader and its dependencies.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}