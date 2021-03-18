using System;
using System.Collections.Generic;
using System.Text;

namespace ModelUpgrade.Core
{
    /// <summary>
    /// Model upgrade interface
    /// </summary>
    public abstract class ModelUpgrade
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

        /// <summary>
        /// Upgrades the specified model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public IVersionModel Upgrade<T>(T model) where T : IVersionModel
        {
            return null;
        }


    }
}
