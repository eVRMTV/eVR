using BerTlv;

namespace eVR.Reader.PCSC
{
    /// <summary>
    /// Interface for a TLV parser
    /// </summary>
    public interface IParserTlv
    {
        /// <summary>
        /// Parse a byte array to a collection of keyvaluepairs of string to Tlv. 
        /// The key of each keyvaluepair represents the path of the tags that
        /// leads to the specific TLV structure. The tags in that path are separated by pipes (|).
        /// </summary>
        /// <param name="tlv">The raw byte array</param>
        /// <returns>The parsed tlv structures</returns>
        Task<List<KeyValuePair<string, Tlv>>> Parse(byte[] tlv);
    }
}
