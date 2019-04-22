﻿// This file isn't generated, but this comment is necessary to exclude it from StyleCop analysis.
// <auto-generated/>

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Microsoft.Diagnostics.Runtime
{
    /// <summary>
    /// Thrown when we fail to read memory from the target process.
    /// </summary>
    internal class MemoryReadException : IOException
    {
        /// <summary>
        /// The address of memory that could not be read.
        /// </summary>
        public ulong Address { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The address of memory that could not be read.</param>
        public MemoryReadException(ulong address)
            : base(string.Format("Could not read memory at {0:x}.", address))
        {
        }
    }
}
