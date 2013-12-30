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
    using System.Collections.Generic;
    using System.IO;

    public class TLV
    {
        public byte[] Tag
        {
            get;
            set;
        }

        public byte[] Value
        {
            get;
            set;
        }

        public bool isConstructed
        {
            get;
            set;
        }

        public bool isIndefiniteLength
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }

        public TLVList Childs
        {
            get;
            set;
        }

        public string TagName
        {
            get
            {
                return string.Format("{0:X2}", this.ByteArrayToInt(this.Tag));
            }
        }

        public TLV this[int i]
        {
            get
            {
                return this.Childs[i];
            }
        }

        private static byte MULTI_BYTE_TAG_MASK = 0x1F;             // 0001 1111    (For first tag-byte)
        private static byte MULTI_BYTE_TAG_2_MASK = 0x80;           // 1000 0000    (For subsequent tag-bytes)
        private static byte CONSTRUCTED_DATAOBJECT_MASK = 0x20;     // 0010 0000    (Object primitive or constructed)

        public static TLVList Parse(Stream s)
        {
            return TLV.Parse(s, false);
        }

        public static TLVList Parse(Stream s, bool unpackSequenceValues)
        {
            TLVList l = new TLVList();

            while (s.Position < s.Length)
            {
                TLV t = new TLV(s);
                t.Childs = TLV.ParseTagList(t.Value, unpackSequenceValues);
                l.Add(t);
            }

            return l;
        }

        private TLV(Stream s)
        {
            this.Tag = GetTag(s);

            this.isConstructed = (this.Tag[0] & CONSTRUCTED_DATAOBJECT_MASK) == CONSTRUCTED_DATAOBJECT_MASK;

            bool isIndefiniteLength = false;
            this.Length = DerLengthDecode(s, ref isIndefiniteLength);
            this.isIndefiniteLength = isIndefiniteLength;

            if (this.Length > 0)
            {
                this.Value = new byte[this.Length];
                s.Read(this.Value, 0, (int)this.Length);
            }
        }

        private static TLVList ParseTagList(byte[] data, bool unpackSequenceValues)
        {
            TLVList tagList = new TLVList();
            MemoryStream ms = new MemoryStream(data);

            while (ms.Position < ms.Length)
            {
                TLV tlv = new TLV(ms);

                if (tlv.Value != null && tlv.Value.Length > 0)
                {
                    Asn1TagValue asn1Tag;

                    if ((tlv.Value[0] & (byte)Asn1TagClassValue.CLASS_MASK) != 0)
                    {
                        asn1Tag = (Asn1TagValue)(tlv.Value[0] & (byte)Asn1TagClassValue.CLASS_MASK);
                    }
                    else
                    {
                        asn1Tag = (Asn1TagValue)(tlv.Value[0] & (byte)Asn1TagValue.TAG_MASK);
                    }
                    if (tlv.isConstructed || (unpackSequenceValues && asn1Tag == Asn1TagValue.SEQUENCE))
                    {
                        tlv.Childs = TLV.ParseTagList(tlv.Value, unpackSequenceValues);
                    }
                    tagList.Add(tlv);
                }
            }
            return tagList;
        }

        private byte[] GetTag(Stream bt)
        {
            // 1st tag-byte
            List<byte> tag = new List<byte>();
            bool isNextByteTag = false;
            tag.Add((byte)bt.ReadByte());

            isNextByteTag = ((tag[0] & MULTI_BYTE_TAG_MASK) == MULTI_BYTE_TAG_MASK);

            // Subsequent tag-bytes
            while (isNextByteTag && bt.Position < bt.Length)
            {
                tag.Add((byte)bt.ReadByte());
                isNextByteTag = ((tag[tag.Count - 1] & MULTI_BYTE_TAG_2_MASK) == MULTI_BYTE_TAG_2_MASK);
            }

            return tag.ToArray();
        }

        private int DerLengthDecode(Stream bt, ref bool isIndefiniteLength)
        {
            isIndefiniteLength = false;
            int length = 0;
            byte b;

            b = (byte)bt.ReadByte();
            if ((b & 0x80) == 0)
            {
                length = b;
            }
            else
            {
                long lengthBytes = b & 0x7f;
                if (lengthBytes == 0)
                {
                    isIndefiniteLength = true;
                    return -2; // Indefinite length.
                }
                length = 0;
                while (lengthBytes-- > 0)
                {
                    if ((length >> (8 * (4 - 1))) > 0) // 4: sizeof(long)
                    {
                        return -1; // Length overflow.
                    }
                    b = (byte)bt.ReadByte();
                    length = (length << 8) | b;
                }
            }
            return length;
        }

        private int ByteArrayToInt(byte[] data)
        {
            int result = 0;
            int lengthBytes = 0;

            while (lengthBytes < data.Length)
            {
                if ((result >> (8 * (4 - 1))) > 0) // 4: sizeof(long)
                {
                    return -1; // Length overflow.
                }
                byte b = (byte)data[lengthBytes++];
                result = (result << 8) | b;
            }

            return result;
        }
    }
}
