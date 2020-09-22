using System.Text.Json;

namespace BlueBoxMoon.LocalSubway.Server
{
    /// <summary>
    /// A JSON converted based on System.Text.Json
    /// </summary>
    /// <seealso cref="BlueBoxMoon.LocalSubway.IJsonConverter" />
    public class JsonConverter : IJsonConverter
    {
        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public T DeserializeObject<T>( string json )
        {
            return JsonSerializer.Deserialize<T>( json );
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns></returns>
        public string SerializeObject( object @object )
        {
            return JsonSerializer.Serialize( @object );
        }
    }
}
