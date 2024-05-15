using BerTlv;

namespace eVR.Reader.PCSC
{
    /// <summary>
    /// Class used to parse byte arrays as TLV structures
    /// </summary>
    public class ParserTlv
        : IParserTlv
    {
        #region Interface - IParserTlv

        /// <summary>
        /// Parse a byte array to a collection of keyvaluepairs of string to Tlv. 
        /// The key of each keyvaluepair represents the path of the tags that
        /// leads to the specific TLV structure. The tags in that path are separated by pipes (|).
        /// </summary>
        /// <param name="tlv">The raw byte array</param>
        /// <returns>The parsed tlv structures</returns>
        public Task<List<KeyValuePair<string, Tlv>>> Parse(byte[] tlv)
        {   
            return Task.Run(() =>
            {
                if (tlv == null || tlv == Array.Empty<byte>())
                {
                    return [];
                }
                var parsed = Tlv.Parse(tlv);
                return this.Parse(string.Empty, parsed);
            });            
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Recursive function used to parse TLV structures
        /// </summary>
        /// <param name="hexTag"></param>
        /// <param name="list"></param>
        /// <returns></returns>

        private List<KeyValuePair<string, Tlv>> Parse(string hexTag, ICollection<Tlv> list)
        {
            var l = new List<KeyValuePair<string, Tlv>>();
            foreach (var entry in list)
            {
                var tag = $"{hexTag}|{entry.HexTag?.PadLeft(2, '0')}";
                l.Add(new KeyValuePair<string, Tlv>(tag, entry));              
                if(entry.Children.Count != 0)
                {
                    l = [.. l, .. Parse(tag, entry.Children)];
                }
            }
            return l;
        }

        #endregion
    }

    /// <summary>
    /// Extensions for parsed TLV structures
    /// </summary>
    public static class ParserTlvExtensions
    {
        /// <summary>
        /// Get a TLV structure by a path of tags separated by pipes (|)
        /// </summary>
        /// <param name="list">The collection of TLV structures</param>
        /// <param name="selector">The path of tags separated by pipes (|)</param>
        /// <param name="occurrence">The occurence of the path (default 0)</param>
        /// <returns>The TLV structure to be found</returns>
        public static Tlv? Tag(this IEnumerable<KeyValuePair<string, Tlv>> list, string selector, int occurrence = 0)
        {  
            if (!selector.StartsWith('|'))
            {
                selector = $"|{selector}";
            }
            if(occurrence == 0)
            {
                return list.FirstOrDefault(e => e.Key == selector).Value;
            }
            var entries = list.Where(e => e.Key == selector);
            if (entries.Count() >= occurrence + 1)
            {
                return entries.Skip(occurrence).Take(1).First().Value;
            }
            return null;
        }

        /// <summary>
        /// Get all TLV structures by a path of tags separated by pipes (|)
        /// </summary>
        /// <param name="list">The collection of TLV structures</param>
        /// <param name="selector">The path of tags separated by pipes (|)</param>
        /// <returns>All TLV structures that match the selector</returns>
        public static IEnumerable<Tlv> Tags(this IEnumerable<KeyValuePair<string, Tlv>> list, string selector)
        {
            if (!selector.StartsWith('|'))
            {
                selector = $"|{selector}";
            }
            return list.Where(e => e.Key == selector).Select(e => e.Value);
        }
    }
}
