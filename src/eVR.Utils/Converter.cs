/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

namespace MTVReader
{
    using System;

    public class Converter
    {
        public static string BitToString(int bit)
        {
            return string.Format("{0:X2} ", bit);
        }

        public static int ByteArrayToInt(byte[] arr)
        {
            int result = 0;
            byte[] arrAsRightByteArray = MakeRightByteArray(arr);

            if (arrAsRightByteArray.Length == 1)
            {
                byte[] tmp = new byte[] { 0x00, arrAsRightByteArray[0] };
                result = BitConverter.ToUInt16(tmp, 0);
            }
            else if (arrAsRightByteArray.Length == 2)
            {
                result = BitConverter.ToUInt16(arrAsRightByteArray, 0);
            }
            else if (arrAsRightByteArray.Length == 3)
            {
                result = arrAsRightByteArray[2] << 16 | arrAsRightByteArray[1] << 8 | arrAsRightByteArray[0];
            }
            else
            {
                result = BitConverter.ToInt32(arrAsRightByteArray, 0);
            }
            return result;
        }

        private static byte[] MakeRightByteArray(byte[] arr)
        {
            byte[] result = new byte[arr.Length];

            arr.CopyTo(result, 0);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }
            return result;
        }

        public static byte[] IntToByteArray(int nr, int maxLenght)
        {
            byte[] arr = BitConverter.GetBytes(nr);
            return ByteArrayGetLastBytes(MakeRightByteArray(arr), maxLenght);
        }

        private static byte[] ByteArrayGetLastBytes(byte[] arr, int totalBytes)
        {
            int endIndex = arr.Length;
            int startIndex = endIndex - totalBytes;
            byte[] result = new byte[totalBytes];
            for (int i = 0; i < totalBytes; i++)
            {
                result[i] = arr[startIndex + i];
            }
            return result;
        }
    }
}
