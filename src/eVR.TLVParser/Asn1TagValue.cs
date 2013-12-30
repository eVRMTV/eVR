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
    /// <summary>
    /// Define ASN.1 tag constants.
    /// </summary>
    /// 
    public enum Asn1TagValue : byte
    {
        /// <summary>
        /// Tag mask constant value.
        /// </summary>
        TAG_MASK = 0x1F,

        /// <summary>
        /// Constant value.
        /// </summary>
        BOOLEAN = 0x01,

        /// <summary>
        /// Constant value.
        /// </summary>
        INTEGER = 0x02,

        /// <summary>
        /// Constant value.
        /// </summary>
        BIT_STRING = 0x03,

        /// <summary>
        /// Constant value.
        /// </summary>
        OCTET_STRING = 0x04,

        /// <summary>
        /// Constant value.
        /// </summary>
        TAG_NULL = 0x05,

        /// <summary>
        /// Constant value.
        /// </summary>
        OBJECT_IDENTIFIER = 0x06,

        /// <summary>
        /// Constant value.
        /// </summary>
        OBJECT_DESCRIPTOR = 0x07,

        /// <summary>
        /// Constant value.
        /// </summary>
        EXTERNAL = 0x08,

        /// <summary>
        /// Constant value.
        /// </summary>
        REAL = 0x09,

        /// <summary>
        /// Constant value.
        /// </summary>
        ENUMERATED = 0x0a,

        /// <summary>
        /// Constant value.
        /// </summary>
        UTF8_STRING = 0x0c,

        /// <summary>
        /// Relative object identifier.
        /// </summary>
        RELATIVE_OID = 0x0d,

        /// <summary>
        /// Constant value.
        /// </summary>
        SEQUENCE = 0x10,

        /// <summary>
        /// Constant value.
        /// </summary>
        SET = 0x11,

        /// <summary>
        /// Constant value.
        /// </summary>
        NUMERIC_STRING = 0x12,

        /// <summary>
        /// Constant value.
        /// </summary>
        PRINTABLE_STRING = 0x13,

        /// <summary>
        /// Constant value.
        /// </summary>
        T61_STRING = 0x14,

        /// <summary>
        /// Constant value.
        /// </summary>
        VIDEOTEXT_STRING = 0x15,

        /// <summary>
        /// Constant value.
        /// </summary>
        IA5_STRING = 0x16,

        /// <summary>
        /// Constant value.
        /// </summary>
        UTC_TIME = 0x17,

        /// <summary>
        /// Constant value.
        /// </summary>
        GENERALIZED_TIME = 0x18,

        /// <summary>
        /// Constant value.
        /// </summary>
        GRAPHIC_STRING = 0x19,

        /// <summary>
        /// Constant value.
        /// </summary>
        VISIBLE_STRING = 0x1a,

        /// <summary>
        /// Constant value.
        /// </summary>
        GENERAL_STRING = 0x1b,

        /// <summary>
        /// Constant value.
        /// </summary>
        UNIVERSAL_STRING = 0x1C,

        /// <summary>
        /// Constant value.
        /// </summary>
        BMPSTRING = 0x1E, /* 30: Basic Multilingual Plane/Unicode string */
    }
}
