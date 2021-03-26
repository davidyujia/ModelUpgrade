using System;
using System.Linq;

namespace ModelUpgrade.Core.Extensions
{
    public static class ModelUpgradeExtension
    {
        public static void CheckModelUpgradeChain(Type previousVersionType, params ModelUpgradeChain[] modelUpgradeChains)
        {
            if (modelUpgradeChains == null || !modelUpgradeChains.Any())
            {
                return;
            }

            var genericArguments = modelUpgradeChains.Select(modelUpgradeChain => modelUpgradeChain?.GetType().BaseType?.GetGenericArguments() ?? Array.Empty<Type>()).ToArray();

            if (genericArguments.Any(lastGenericArguments => lastGenericArguments.Length > 0 && lastGenericArguments[0] != previousVersionType))
            {
                throw new ArgumentException($"{modelUpgradeChains.GetType().FullName} can't convert model to \"{previousVersionType.FullName}\".");
            }
        }
    }
}
