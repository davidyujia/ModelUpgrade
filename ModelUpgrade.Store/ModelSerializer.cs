namespace ModelUpgrade.Store
{
    /// <summary>
    /// Model serializer interface
    /// </summary>
    public interface IModelSerializer
    {
        /// <summary>
        /// Deserializes the string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">string</param>
        /// <returns></returns>
        T Deserialize<T>(string s);

        /// <summary>
        /// Serializes the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        string Serialize(object model);
    }
}
