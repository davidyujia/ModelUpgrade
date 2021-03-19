namespace ModelUpgrade.Core
{
    /// <summary>
    /// IVersionModel
    /// </summary>
    public interface IVersionModel
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <returns></returns>
        string GetId();
        /// <summary>
        /// Gets the name of the model.
        /// </summary>
        /// <returns></returns>
        string GetModelName();
    }
}
