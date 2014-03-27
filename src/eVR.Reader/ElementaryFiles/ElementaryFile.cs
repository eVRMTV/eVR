/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

namespace EVR.Reader
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using EVR.TLVParser;
    using EVR.Utils;
    using System.Text;

    public abstract class ElementaryFile
    {
        private static TraceSource TS = new TraceSource("ElementaryFile");
        private static TraceSwitch TW = new TraceSwitch("ElementaryFileSwitch", "Elementary file trace");

        public CardReader cardReader
        {
            get;
            private set;
        }

        public byte[] AID
        {
            get;
            private set;
        }

        public byte[] FileID
        {
            get;
            private set;
        }

        public byte[] Value
        {
            get;
            private set;
        }

        private TLVList tagListEF
        {
            get;
            set;
        }

        public abstract Encoding CharacterSetEncoding
        {
            get;
            set;
        }

        public ElementaryFile(byte[] AID, CardReader cardReader, byte[] FileID)
        {
            TS.TraceI("Constructing ElementaryFile with AID \"{0}\" and FileID \"{1}\".", Helper.ByteArrayToString(AID), Helper.ByteArrayToString(FileID));
            this.AID = AID;
            this.cardReader = cardReader;
            this.FileID = FileID;

            try
            {
                this.Value = cardReader.ReadFile(AID, FileID);
            }
            catch (CardReaderException ex)
            {
                throw new ElementaryFileException(FileID, "Error reading EF.", ex);
            }

            if (this.Value == null)
            {
                throw new ElementaryFileException(FileID, string.Format("Error reading EF: AID = \"{0}\", FileID = \"{1}\".", Helper.ByteArrayToString(AID), Helper.ByteArrayToString(FileID)));
            }

            this.tagListEF = TLV.Parse(new MemoryStream(this.Value));
            TS.TraceI("Number of elements in EF: \"{0}\".", this.tagListEF.Count);
            TS.TraceI("ElementaryFile constructed.");
        }

        public TLV GetTag(string path)
        {
            TLV tag = this.tagListEF.getTag(path);

            return tag;
        }

        public bool Transmit(byte[] sendBuffer, ref byte[] recvBuffer)
        {
            return cardReader.Transmit(sendBuffer, ref recvBuffer);
        }

        protected string DecodeString(string tlvPath)
        {
            if (string.IsNullOrEmpty(tlvPath))
            {
                throw new ArgumentException("Argument should not be empty", "tlvPath");
            }

            TLV tag = this.GetTag(tlvPath);

            return (tag == null) ? string.Empty : Helper.DecodeString(tag.Value, this.CharacterSetEncoding);
        }

        protected string DecodeString(TLV tag)
        {
            return (tag == null) ? string.Empty : Helper.DecodeString(tag.Value, this.CharacterSetEncoding);
        }

        protected string DecodeBinary(string tlvPath)
        {
            if (string.IsNullOrEmpty(tlvPath))
            {
                throw new ArgumentException("Argument should not be empty", "tlvPath");
            }

            TLV tag = this.GetTag(tlvPath);

            return (tag == null) ? null : Helper.DecodeBinairy(tag.Value);
        }

        protected string DecodeBinary(TLV tag)
        {
            return (tag == null) ? null : Helper.DecodeBinairy(tag.Value);
        }
    }
}
