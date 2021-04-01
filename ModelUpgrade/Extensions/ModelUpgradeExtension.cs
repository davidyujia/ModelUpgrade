using System;
using System.Linq;

namespace ModelUpgrade.Extensions
{
    /// <summary>
    /// Model upgrade extension
    /// </summary>
    public static class ModelUpgradeExtension
    {
        /// <summary>
        /// Checks the model upgrade chain.
        /// </summary>
        /// <param name="previousVersionType">Type of the previous version.</param>
        /// <param name="modelUpgradeChains">The model upgrade chains.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public static void CheckModelUpgradeChain(Type previousVersionType, params ModelUpgradeChain[] modelUpgradeChains)
        {
            if (modelUpgradeChains == null || !modelUpgradeChains.Any())
            {
                return;
            }

            var genericArguments = modelUpgradeChains.Select(modelUpgradeChain => modelUpgradeChain?.GetType().BaseType?.GetGenericArguments() ?? Array.Empty<Type>()).ToArray();

            if (genericArguments.Any(lastGenericArguments => lastGenericArguments.Length > 1 && lastGenericArguments[1] != previousVersionType))
            {
                throw new ArgumentException($"{modelUpgradeChains.GetType().FullName} can't convert model to \"{previousVersionType.FullName}\".");
            }
        }
    }
}
