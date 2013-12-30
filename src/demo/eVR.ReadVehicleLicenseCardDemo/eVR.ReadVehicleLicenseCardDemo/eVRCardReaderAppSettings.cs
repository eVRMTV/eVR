/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

// -----------------------------------------------------------------------
// <copyright file="eVRCardReaderAppSettings.cs" company="RDW">
// RDW
// </copyright>
// -----------------------------------------------------------------------
namespace EVR.ReadVehicleLicenseCardDemo
{
    using System.Configuration;
    using System.Diagnostics;
    
    /// <summary>
    /// EVRCardReaderAppSettings class to read app.config
    /// </summary>
    public static class EVRCardReaderAppSettings
    {
        /// <summary>
        /// Gets a CardAccessDelay
        /// </summary>
        public static int CardAccessDelay
        {
            get
            {
                string strCardAccessDelay = ConfigurationManager.AppSettings["CardAccessDelay"];
                int cardAccessDelay = 500;

                if (!string.IsNullOrEmpty(strCardAccessDelay))
                {
                    if (!int.TryParse(strCardAccessDelay, out cardAccessDelay))
                    {
                        cardAccessDelay = 500;
                    }
                }

                return cardAccessDelay;
            }
        }
     
        /// <summary>
        /// Gets a value indicating whether CRLCheckEnabled
       /// </summary>
        public static bool CRLCheckEnabled
        {
            get
            {
                string strCRLCheckEnabled = ConfigurationManager.AppSettings["CRLCheckEnabled"];
                bool crlCheckEnabled = false;

                if (!string.IsNullOrEmpty(strCRLCheckEnabled))
                {
                    if (!bool.TryParse(strCRLCheckEnabled, out crlCheckEnabled))
                    {
                        crlCheckEnabled = false;
                    }
                }

                return crlCheckEnabled;
            }
        }
       
        /// <summary>
        /// Gets a CSCAFilename
        /// </summary>
        public static string CSCAFilename
        {
            get
            {
                return ConfigurationManager.AppSettings["CSCAFileName"];
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether ATRCheck
        /// </summary>
        public static bool ATRCheck
        {
            get
            {
                string strATRCheck = ConfigurationManager.AppSettings["ATRCheck"];
                bool atrCheck = false;

                if (!string.IsNullOrEmpty(strATRCheck))
                {
                    if (!bool.TryParse(strATRCheck, out atrCheck))
                    {
                        atrCheck = false;
                    }
                }
                
                return atrCheck;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether CardManagerDisabledCheck
        /// </summary>
        public static bool CardManagerDisabledCheck
        {
            get
            {
                string strCardManagerDisabledCheck = ConfigurationManager.AppSettings["CardManagerDisabledCheck"];
                bool cardManagerDisabledCheck = false;

                if (!string.IsNullOrEmpty(strCardManagerDisabledCheck))
                {
                    if (!bool.TryParse(strCardManagerDisabledCheck, out cardManagerDisabledCheck))
                    {
                        cardManagerDisabledCheck = false;
                    }
                }

                return cardManagerDisabledCheck;
            }
        }
    }
}
