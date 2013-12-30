/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

// -----------------------------------------------------------------------
// <copyright file="Helper.cs" company="RDW">
// RDW
// </copyright>
// -----------------------------------------------------------------------
namespace EVR.ReadVehicleLicenseCardDemo
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper class.
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// date too NL Date method
        /// </summary>
        /// <param name="dateEEJJMMDD">dateEEJJMMDD parameter</param>
        /// <returns>Date to NL string</returns>
        public static string EEYYMMDDToNLDate(string dateEEJJMMDD)
        {
            int result;

            // Is empty
            if (string.IsNullOrEmpty(dateEEJJMMDD))
            {
                return "-";
            }

            // Correct length
            if (dateEEJJMMDD.Length != 8)
            {
                return dateEEJJMMDD;
            }

            // An integer
            if (!int.TryParse(dateEEJJMMDD, out result))
            {
                return dateEEJJMMDD;
            }

            // Ok; parse it
            return dateEEJJMMDD.Substring(6, 2) + "-" + dateEEJJMMDD.Substring(4, 2) + "-" + dateEEJJMMDD.Substring(0, 4);
        }

        /// <summary>
        /// Split string in max length chunks.
        /// </summary>
        /// <param name="s">The string to split</param>
        /// <param name="maxlen">The max length of each chunk</param>
        /// <returns>A list of string of max length</returns>
        public static List<string> BreakLines(string s, int maxlen)
        {
            List<string> result = new List<string>();

            if (s == null)
            {
                return result;
            }

            while (s.Length > 0)
            {
                result.Add(s.Substring(0, s.Length >= maxlen ? maxlen : s.Length));
                s = s.Substring(s.Length >= maxlen ? maxlen : s.Length);
            }

            return result;
        }

        /// <summary>
        /// For every word in the string that is longer dan 'l' a space is inserted at position l
        /// </summary>
        /// <param name="sentence">The input sentence</param>
        /// <param name="l">The max length of a word in the string</param>
        /// <returns>The adjusted string</returns>
        public static string AddSpaceAtWordLength(string sentence, int l)
        {
            List<string> maxed = new List<string>();
            string[] pieces = sentence.Split(' ');

            foreach (var piece in pieces)
            {
                if (piece.Length > l)
                {
                    string s = piece;
                    while (s.Length > 0)
                    {
                        int min = Math.Min(s.Length, l);

                        maxed.Add(s.Substring(0, min));
                        s = s.Substring(min);
                    }
                }
                else
                {
                    maxed.Add(piece);
                }
            }

            return string.Join(" ", maxed);
        }
        
        /// <summary>
        /// Split a string on spaces at a length of at most MaxLength characters
        /// </summary>
        /// <param name="sentence">The sentence to split</param>
        /// <param name="maxLength">The max length of a split part</param>
        /// <returns>The split parts</returns>
        public static List<string> StringSplitWrap(string sentence, int maxLength)
        {
            List<string> parts = new List<string>();

            string[] pieces = AddSpaceAtWordLength(sentence, maxLength).Split(' ');

            StringBuilder tempString = new StringBuilder(string.Empty);

            foreach (var piece in pieces)
            {
                if (piece.Length + tempString.Length > maxLength)
                {
                    parts.Add(tempString.ToString());
                    tempString.Clear();
                }

                tempString.Append((tempString.Length == 0 ? string.Empty : " ") + piece);
            }

            if (tempString.Length > 0)
            {
                parts.Add(tempString.ToString());
            }

            return parts;
        }

        /// <summary>
        /// List method
        /// </summary>
        /// <param name="adresLine">Adres Line string</param>
        /// <param name="maxAdresLen">MaxAdresLen integer</param>
        /// <returns>List object of strings</returns>
        public static List<string> ToAdres(string adresLine, int maxAdresLen)
        {
            List<string> parts = new List<string>();
            Regex r = new Regex(@"(?<adres>.*)\s*(?<postcode>\d{4,4}\s[a-zA-Z]{2,2})\s*(?<wpl>.*)");
            Match m = r.Match(adresLine);

            if (m.Success)
            {
                string postcode = m.Groups["postcode"].ToString();

                if (m.Groups["adres"] != null)
                {
                    string adres = m.Groups["adres"].ToString();
                    parts.Add(adres.Substring(0, Math.Min(adres.Length, maxAdresLen)));
                    if (adres.Length > maxAdresLen)
                    {
                        postcode = adres.Substring(maxAdresLen) + " " + postcode;
                    }
                }

                if (m.Groups["wpl"] != null)
                {
                    postcode += "  " + m.Groups["wpl"].ToString();
                }

                parts.Add(postcode);
                return parts;
            }
            else
            {
                // Could not parse string into postcode and wpl; split into 2 parts of MaxAdresLen
                parts.Add(adresLine.Substring(0, Math.Min(adresLine.Length, maxAdresLen)));
                if (adresLine.Length > maxAdresLen)
                {
                    string line = adresLine.Substring(maxAdresLen);

                    parts.Add(line.Substring(0, Math.Min(line.Length, maxAdresLen)));
                }

                return parts;
            }
        }
    }
}
