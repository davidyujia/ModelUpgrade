using System;
using System.Collections.Generic;
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

            var exceptions = modelUpgradeChains
                .Where(chain => chain.TargetType != previousVersionType)
                .Select(chain => new ArgumentException($"\"{chain.GetType().FullName}\" can't convert model to \"{previousVersionType.FullName}\"."))
                .ToList();

            if (exceptions.Any())
            {
                throw new AggregateException($"Has chain(s) can't convert model to \"{previousVersionType.FullName}\".", exceptions);
            }
        }

        internal static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }
    }
}
