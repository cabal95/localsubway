using System;
using System.Collections;
using System.Collections.Generic;

namespace BlueBoxMoon.LocalSubway.Http
{
    /// <summary>
    /// A collection that is safe to use with HTTP headers.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String, System.Collections.Generic.IReadOnlyList{System.String}}}" />
    public class HeaderCollection : IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>
    {
        /// <summary>
        /// The internal headers collection.
        /// </summary>
        private readonly Dictionary<string, IList<string>> _headers = new Dictionary<string, IList<string>>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderCollection"/> class.
        /// </summary>
        public HeaderCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderCollection"/> class.
        /// </summary>
        /// <param name="headerLines">The header lines.</param>
        public HeaderCollection( IEnumerable<string> headerLines )
        {
            foreach ( var headerLine in headerLines )
            {
                var pairs = headerLine.Split( new[] { ": " }, StringSplitOptions.None );
                Add( pairs[0], pairs[1] );
            }
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add( string key, string value )
        {
            IList<string> values;

            if ( _headers.ContainsKey( key ) )
            {
                values = _headers[key];
            }
            else
            {
                values = new List<string>();
                _headers[key] = values;
            }

            values.Add( value );
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove( string key )
        {
            if ( _headers.ContainsKey( key ) )
            {
                _headers.Remove( key );
            }
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public IEnumerable<string> Get( string key )
        {
            if ( _headers.ContainsKey( key ) )
            {
                return _headers[key];
            }

            return null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator()
        {
            foreach ( var header in _headers )
            {
                yield return new KeyValuePair<string, IReadOnlyList<string>>( header.Key, ( IReadOnlyList<string> ) header.Value );
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var headers = new List<string>();

            foreach ( var header in _headers )
            {
                foreach ( var value in header.Value )
                {
                    headers.Add( $"{header.Key}: {value}" );
                }
            }

            return string.Join( "\r\n", headers );
        }
    }
}
