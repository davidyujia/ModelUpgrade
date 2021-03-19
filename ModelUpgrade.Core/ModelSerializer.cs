using System;
using System.Collections.Generic;
using System.Text;

namespace ModelUpgrade.Core
{
    /// <summary>
    /// Model upgrade
    /// </summary>
    public abstract class ModelSerializer
    {
        /// <summary>
        /// Deserializes the string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">string</param>
        /// <returns></returns>
        public abstract T Deserialize<T>(string s) where T : IVersionModel;

        /// <summary>
        /// Serializes the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public abstract string Serialize(IVersionModel model);
    }
}
