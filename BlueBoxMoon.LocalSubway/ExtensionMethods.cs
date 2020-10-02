using System;
using System.Collections.Generic;

using BlueBoxMoon.LocalSubway.Http;

namespace BlueBoxMoon.LocalSubway
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Searches a byte buffer for a range of bytes.
        /// </summary>
        /// <param name="haystack">The haystack to search in.</param>
        /// <param name="needle">The needle to be searched for.</param>
        /// <param name="offset">The offset to start searching from.</param>
        /// <param name="size">The number of bytes to search.</param>
        /// <returns></returns>
        public static int IndexOf( this byte[] haystack, byte[] needle, int offset, int size )
        {
            int needleIndex = 0;

            for ( int i = offset; i < offset + size; i++ )
            {
                if ( needle[needleIndex] == haystack[i] )
                {
                    needleIndex += 1;
                    if ( needleIndex == needle.Length )
                    {
                        return i - needle.Length + 1;
                    }
                }
                else
                {
                    needleIndex = 0;
                }
            }

            return -1;
        }

        public static bool AsBoolean( this string s )
        {
            if ( s == null)
            {
                return false;
            }

            return bool.TryParse( s, out var result ) && result;
        }

        public static int AsInteger( this string s )
        {
            if ( s == null )
            {
                return 0;
            }

            return int.TryParse( s, out var result ) ? result : 0;
        }

        public static Guid AsGuid( this string s )
        {
            if ( s == null )
            {
                return Guid.Empty;
            }

            return Guid.TryParse( s, out var result ) ? result : Guid.Empty;
        }
    }
}
