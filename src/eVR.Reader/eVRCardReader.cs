using eVR.Reader.Data;
using Microsoft.Extensions.Logging;
using eVR.Reader.PCSC;
using PCSC.Monitoring;
using PCSC;
using System.Diagnostics.CodeAnalysis;

namespace eVR.Reader
{
    /// <summary>
    /// Class used to read eVR specific data from an eVR card.
    /// </summary>
    /// <param name="parserTlv">A TLV parser</param>
    /// <param name="logger">A logger to be used in this class</param>
    /// <param name="readerLogger">A logger to be used in baseclass CardReader</param>
    /// <param name="reader">A cardreader</param>
    /// <param name="context">An application context to the PC/SC Resource Manager</param>
    /// <param name="monitor">A monitor for card reader events/triggers</param>
#pragma warning disable IDE1006 // Naming Styles
    [SuppressMessage("csharpsquid", "S101", Justification = "The spelling of eVR does not match pascal case naming rules")]
    public class eVRCardReader(
#pragma warning restore IDE1006 // Naming Styles
          IParserTlv parserTlv
        , ILogger<eVRCardReader> logger
        , ILogger<PCSC.CardReader> readerLogger
        , ISCardReader reader
        , ISCardContext context
        , ISCardMonitor monitor)
        : PCSC.CardReader(readerLogger, reader, context, monitor)
    {
        #region Methods

        /// <summary>
        /// Read all elementary files on an eVR Card
        /// </summary>
        /// <param name="readerName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<eVRCardState> Read(string readerName, CancellationToken cancellationToken)
        {
            // recreate the state and elementary files
            var state = new eVRCardState(parserTlv);
            await ConnectReader(readerName);
            state.ATR = await GetATRString();
            await SelectMF();
            foreach (var data in state.ElementaryFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                logger.LogInformation("{name} read", data.Name);
                data.RawData = await ReadElementaryFile(eVRDefinitions.AID, data.Identifier);
                if (data.NeedsParsing)
                {
                    logger.LogInformation("{name} parse", data.Name);
                    data.ParsedData = await parserTlv.Parse(data.RawData);
                }
                logger.LogInformation("{name} construct", data.Name);
                await data.Construct();
            }
            return state;
        }

        #endregion
    }
}
