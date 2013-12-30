/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

// -----------------------------------------------------------------------
// <copyright file="TraceSourceExtensions.cs" company="RDW">
// Copyright RDW 2012.
// </copyright>
// -----------------------------------------------------------------------
namespace EVR.Utils
{
    using System.Diagnostics;

    /// <summary>
    /// class TraceSourceExtensions
    /// </summary>
    public static class TraceSourceExtensions
    {
        /// <summary>
        /// Write a informational trace message to a tracesource.
        /// </summary>
        /// <param name="traceSource">The trace source to write to</param>
        /// <param name="format">The format of the string</param>
        /// <param name="args">The arguments to format the string</param>
        public static void TraceI(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceInformation(format, args);
        }

        /// <summary>
        /// Write a verbose trace message to a tracesource.
        /// </summary>
        /// <param name="traceSource">The trace source to write to</param>
        /// <param name="format">The format of the string</param>
        /// <param name="args">The arguments to format the string</param>
        public static void TraceV(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }

        /// <summary>
        /// Write a warning trace message to a tracesource.
        /// </summary>
        /// <param name="traceSource">The trace source to write to</param>
        /// <param name="format">The format of the string</param>
        /// <param name="args">The arguments to format the string</param>
        public static void TraceW(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        /// <summary>
        /// Write a error trace message to a tracesource.
        /// </summary>
        /// <param name="traceSource">The trace source to write to</param>
        /// <param name="format">The format of the string</param>
        /// <param name="args">The arguments to format the string</param>
        public static void TraceE(this TraceSource traceSource, string format, params object[] args)
        {
            traceSource.TraceEvent(TraceEventType.Error, 0, format, args);
        }
    }
}
