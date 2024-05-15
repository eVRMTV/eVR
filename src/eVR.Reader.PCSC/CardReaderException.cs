using PCSC;
using PCSC.Utils;
using System;
using System.Runtime.Serialization;

namespace eVR.Reader.PCSC
{
    /// <summary>
    /// Class used for exceptions in the CardReader
    /// </summary>
    [Serializable]
    public class CardReaderException(string message)
                : Exception(message)
    {
        #region Constructors

        public CardReaderException(SCardError error)
            : this(SCardHelper.StringifyError(error))
        {
        }

        public CardReaderException(SCardError error, string message)
            : this($"{SCardHelper.StringifyError(error)}: {message}")
        {
        }

        #endregion
    }
}
