using Microsoft.Extensions.Logging;
using PCSC;
using PCSC.Utils;
using System.Diagnostics.CodeAnalysis;

namespace eVR.Reader.PCSC.Services
{
    /// <summary>
    /// Decorator around class SCardReader that provides logging
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    [SuppressMessage("external_roslyn", "CA1873")]
    public sealed class CardReaderDecorator(
          ISCardContext context
        , ILogger<CardReaderDecorator> logger)
        : ISCardReader
    {
        #region Dependencies

        private readonly SCardReader _reader = new(context);

        #endregion

        #region Properties

        public string ReaderName => _reader.ReaderName;
        public ISCardContext CurrentContext => _reader.CurrentContext;
        public SCardShareMode CurrentShareMode => _reader.CurrentShareMode;
        public SCardProtocol ActiveProtocol => _reader.ActiveProtocol;
        public IntPtr CardHandle => _reader.CardHandle;
        public bool IsConnected => _reader.IsConnected;

        #endregion

        #region Interface - ISCardReader

        public SCardError BeginTransaction()
        {
            var result = _reader.BeginTransaction();
            return LogPossibleErrorResult(result);
        }

        public SCardError Connect(string readerName, SCardShareMode mode, SCardProtocol preferredProtocol)
        {
            var result = _reader.Connect(readerName, mode, preferredProtocol);
            return LogPossibleErrorResult(result);
        }

        public SCardError Control(IntPtr controlCode, byte[] sendBuffer, ref byte[] receiveBuffer)
        {
            var result = _reader.Control(controlCode, sendBuffer, ref receiveBuffer);
            return LogPossibleErrorResult(result);
        }

        public SCardError Disconnect(SCardReaderDisposition disconnectExecution)
        {
            var result = _reader.Disconnect(disconnectExecution);
            return LogPossibleErrorResult(result);
        }

        public void Dispose()
        {
            _reader.Dispose();
            GC.SuppressFinalize(this);
        }

        public SCardError EndTransaction(SCardReaderDisposition disposition)
        {
            var result = _reader.EndTransaction(disposition);
            return LogPossibleErrorResult(result);
        }

        public SCardError GetAttrib(IntPtr attributeId, byte[] attribute, out int attributeBufferLength)
        {
            LogTransmitting(attributeId);
            var result = _reader.GetAttrib(attributeId, attribute, out attributeBufferLength);
            LogReceiving(attribute);
            return LogPossibleErrorResult(result);
        }

        public SCardError GetAttrib(IntPtr attributeId, out byte[] attribute)
        {
            LogTransmitting(attributeId);
            var result = _reader.GetAttrib(attributeId, out attribute);
            LogReceiving(attribute);
            return LogPossibleErrorResult(result);
        }

        public SCardError GetAttrib(SCardAttribute attributeId, byte[] attribute, out int attributeBufferLength)
        {
            LogTransmitting(attributeId);
            var result = _reader.GetAttrib(attributeId, attribute, out attributeBufferLength);
            LogReceiving(attribute);
            return LogPossibleErrorResult(result);
        }

        public SCardError GetAttrib(SCardAttribute attributeId, out byte[] attribute)
        {
            LogTransmitting(attributeId);
            var result = _reader.GetAttrib(attributeId, out attribute);
            LogReceiving(attribute);
            return LogPossibleErrorResult(result);
        }

        public SCardError Reconnect(SCardShareMode mode, SCardProtocol preferredProtocol, SCardReaderDisposition initialExecution)
        {
            var result = _reader.Reconnect(mode, preferredProtocol, initialExecution);
            return LogPossibleErrorResult(result);
        }

        public SCardError SetAttrib(IntPtr attributeId, byte[] attribute, int attributeBufferLength)
        {
            var result = _reader.SetAttrib(attributeId, attribute, attributeBufferLength);
            return LogPossibleErrorResult(result);
        }

        public SCardError SetAttrib(IntPtr attributeId, byte[] attribute)
        {
            var result = _reader.SetAttrib(attributeId, attribute);
            return LogPossibleErrorResult(result);
        }

        public SCardError SetAttrib(SCardAttribute attributeId, byte[] attribute, int attributeBufferLength)
        {
            var result = _reader.SetAttrib(attributeId, attribute, attributeBufferLength);
            return LogPossibleErrorResult(result);
        }

        public SCardError SetAttrib(SCardAttribute attributeId, byte[] attribute)
        {
            var result = _reader.SetAttrib(attributeId, attribute);
            return LogPossibleErrorResult(result);
        }

        public SCardError Status(out string[] readerName, out SCardState state, out SCardProtocol protocol, out byte[] atr)
        {
            var result = _reader.Status(out readerName, out state, out protocol, out atr);
            return LogPossibleErrorResult(result);
        }

        public SCardError Transmit(IntPtr sendPci, byte[] sendBuffer, int sendBufferLength, SCardPCI receivePci, byte[] receiveBuffer, ref int receiveBufferLength)
        {
            LogTransmitting(sendBuffer);
            var result = _reader.Transmit(sendPci, sendBuffer, sendBufferLength, receivePci, receiveBuffer, ref receiveBufferLength);
            LogReceiving(receiveBuffer);
            return LogPossibleErrorResult(result);
        }

        public SCardError Transmit(IntPtr sendPci, byte[] sendBuffer, SCardPCI receivePci, ref byte[] receiveBuffer)
        {
            LogTransmitting(sendBuffer);
            var result = _reader.Transmit(sendPci, sendBuffer, receivePci, ref receiveBuffer);
            LogReceiving(receiveBuffer);
            return LogPossibleErrorResult(result);
        }

        public SCardError Transmit(IntPtr sendPci, byte[] sendBuffer, ref byte[] receiveBuffer)
        {
            LogTransmitting(sendBuffer);
            var result = _reader.Transmit(sendPci, sendBuffer, ref receiveBuffer);
            LogReceiving(receiveBuffer);
            return LogPossibleErrorResult(result);
        }

        public SCardError Transmit(SCardPCI sendPci, byte[] sendBuffer, SCardPCI receivePci, ref byte[] receiveBuffer)
        {
            LogTransmitting(sendBuffer);
            var result = _reader.Transmit(sendPci, sendBuffer, receivePci, ref receiveBuffer);
            LogReceiving(receiveBuffer);
            return LogPossibleErrorResult(result);
        }

        public SCardError Transmit(byte[] sendBuffer, int sendBufferLength, byte[] receiveBuffer, ref int receiveBufferLength)
        {
            LogTransmitting(sendBuffer);
            var result = _reader.Transmit(sendBuffer, sendBufferLength, receiveBuffer, ref receiveBufferLength);
            LogReceiving(receiveBuffer);
            return LogPossibleErrorResult(result);
        }

        public SCardError Transmit(byte[] sendBuffer, byte[] receiveBuffer, ref int receiveBufferLength)
        {
            LogTransmitting(sendBuffer);
            var result = _reader.Transmit(sendBuffer, receiveBuffer, ref receiveBufferLength);
            LogReceiving(receiveBuffer);
            return LogPossibleErrorResult(result);
        }

        public SCardError Transmit(byte[] sendBuffer, ref byte[] receiveBuffer)
        {
            LogTransmitting(sendBuffer);
            var result = _reader.Transmit(sendBuffer, ref receiveBuffer);
            LogReceiving(receiveBuffer);
            return LogPossibleErrorResult(result);
        }

        #endregion

        #region Private Methods

        private void LogTransmitting(IntPtr attributeId)
        {
            logger.LogInformation("transmitting: {AttributeId}", attributeId);
        }

        private void LogTransmitting(SCardAttribute attributeId)
        {
            logger.LogInformation("transmitting: {IntAttributeId}-{AttributeId}", (int)attributeId, attributeId);
        }

        private void LogTransmitting(byte[] sendBuffer)
        {
            logger.LogInformation("transmitting: {SendBuffer}", BitConverter.ToString(sendBuffer));
        }

        private void LogReceiving(byte[] buffer)
        {
            logger.LogInformation("receiving: {Buffer}", BitConverter.ToString(buffer));
        }

        private SCardError LogPossibleErrorResult(SCardError result)
        {
            if (result != SCardError.Success)
            {
                logger.LogError("A non-success result was returned: {Error}", SCardHelper.StringifyError(result));
                throw new CardReaderException(result);
            }
            return result;
        }

        #endregion

    }
}
