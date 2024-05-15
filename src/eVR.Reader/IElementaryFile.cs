using BerTlv;

namespace eVR.Reader
{
    /// <summary>
    /// Interface for an Elementary File (EF)
    /// </summary>
    public interface IElementaryFile
    {
        /// <summary>
        /// The heximal representation of the File Identifier
        /// </summary>
        string FileIDHexValue { get; }

        /// <summary>
        /// The name of the Elementary File
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The File Identifier as byte array
        /// </summary>
        byte[] Identifier { get; }

        /// <summary>
        /// The raw data in the Elementary File
        /// </summary>
        byte[] RawData { get; set; }

        /// <summary>
        /// An indication whether the RawData needs to be parsed as TLV structures
        /// </summary>
        bool NeedsParsing { get; }

        /// <summary>
        /// The data, parsed as TLV structures
        /// </summary>
        IEnumerable<KeyValuePair<string, Tlv>> ParsedData { get; set; }

        /// <summary>
        /// Method to construct the specific properties of the specific Elementary File
        /// </summary>
        /// <returns></returns>
        Task Construct();
    }
}
