/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

namespace EVR.TLVParser
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Tag Length Value list.
    /// </summary>
    public class TLVList : List<TLV>
    {
        /// <summary>
        /// Search an occurence of pathPos in list. The string pathPos should have following layout: "x,yy" where
        /// x = a number greater then 0 e.g. 2
        /// yy = a tag name e.g. 30
        /// The list is searched for the given tag (case insensitive). The list can have multiple tags with the same name; the number
        /// indicates which entry has to be returned
        /// </summary>
        /// <param name="list">A list TLV items</param>
        /// <param name="posCommaTagName">The entry to search for example "1,6F"</param>
        /// <returns>The index in the list that contains the entry or -1 if not found</returns>
        private int searchOccurence(TLVList list, string posCommaTagName)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (string.IsNullOrEmpty(posCommaTagName))
            {
                throw new ArgumentException("pathPos should not be null or empty", "pathPos");
            }

            string[] pathPosSplitted = posCommaTagName.Split(new char[] { ',' });
            if (pathPosSplitted.Length != 2)
            {
                throw new ArgumentException(string.Format("Invalid pathPos: {0}", posCommaTagName));
            }

            int occurence = int.Parse(pathPosSplitted[0]);
            if (occurence < 1)
            {
                throw new ArgumentException(string.Format("Invalid position: {0}", occurence));
            }

            string TagName = pathPosSplitted[1];
            if (string.IsNullOrEmpty(TagName))
            {
                throw new ArgumentException(string.Format("Invalid tagname: {0}", TagName));
            }

            bool found = false;
            int i = 0;
            int occurencesFound = 0;

            while (!found && i < list.Count)
            {
                if (string.Compare(list[i].TagName, TagName, true) == 0)
                {
                    occurencesFound++;
                }

                if (occurencesFound == occurence)
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        /// <summary>
        /// Retrieve the TLV structure at the given path. A path is for example "1,6F|2,30|1,31|1,04"
        /// </summary>
        /// <param name="path">The path to follow</param>
        /// <returns>The TLV structure at the given path if found; null otherwise</returns>
        public TLV getTag(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path should not be null or empty", "path");
            }

            string[] pathParts = path.Split(new char[] { '|' });
            int depth = 0;
            bool found = false;
            TLVList currentList = this;

            while (!found && depth < pathParts.Length)
            {
                int pos = searchOccurence(currentList, pathParts[depth]);
                if (pos < 0)
                {
                    // Tag not found at current level
                    return null;
                }

                // Tag is found, but are we at desired level?
                if (depth == pathParts.Length - 1)
                {
                    // Yes; return this entry
                    return currentList[pos];
                }
                else if (currentList[pos].Childs != null)
                {
                    // Not yet at desired level => go down
                    currentList = currentList[pos].Childs;
                    found = false;
                    depth++;
                }
                else
                {
                    // Not at desired level AND current level does not have any childs....
                    return null;
                }
            }

            // Not found
            return null;
        }
    }
}
