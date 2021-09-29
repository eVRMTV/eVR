/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

namespace EVR.ReadVehicleLicenseCardDemo 
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    
    /// <summary>
    /// Translator Class
    /// </summary>
    public class Translator
    {
        /// <summary>
        /// constant TranslationFile
        /// </summary>
        const string AppSettingTranslationsFile = "TranslationsFile";
        
        /// <summary>
        /// Dictionary translations
        /// </summary>
        private static Dictionary<string, string> translations = new Dictionary<string, string>();
        
        /// <summary>
        /// Initializes static members of the <see cref="Translator"/> class
        /// </summary>
        static Translator()
        {
            int linenr = 0;

            string translationsFile = ConfigurationManager.AppSettings[AppSettingTranslationsFile];
            if (string.IsNullOrEmpty(translationsFile))
            {
                translationsFile = "translations.txt";
            }

            using (TextReader tr = new StreamReader(translationsFile))
            {
                string line = null;

                while ((line = tr.ReadLine()) != null)
                {
                    linenr++;

                    string[] fields = line.Split('\t');

                    if (fields.Length != 2)
                    {
                        throw new ArgumentException(string.Format("Invalid line \"{0}\": \"{1}\"", linenr, line));
                    }

                    /*
                     * Add key if it does not already exist; overwrite the key if it already exists (ability to merge translation files)
                     */
                    string result;

                    if (translations.TryGetValue(fields[0], out result))
                    {
                        translations[fields[0]] = fields[1];
                    }
                    else
                    {
                        translations.Add(fields[0], fields[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Translate function
        /// </summary>
        /// <param name="text">test string</param>
        /// <returns>translated string</returns>
        public static string Translate(string text)
        {
            string result = string.Empty;

            if (!translations.TryGetValue(text, out result))
            {
                // Text not found => no translation, return original text
                return text;
            }
            else
            {
                // Text found for translation => return translation
                return result;
            }
        }
    }
}
