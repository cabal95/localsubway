namespace BlueBoxMoon.LocalSubway
{
    /// <summary>
    /// Defines the methods that are needed to serialize and deserialize
    /// JSON objects.
    /// </summary>
    public interface IJsonConverter
    {
        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns></returns>
        string SerializeObject( object @object );

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        T DeserializeObject<T>( string json );
    }
}
