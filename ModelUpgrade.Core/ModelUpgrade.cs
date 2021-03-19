using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelUpgrade.Core
{
    public abstract class ModelUpgradeChain
    {
        internal readonly ModelUpgradeChain[] NextChains;

        protected ModelUpgradeChain(Type previousVersionType, ModelUpgradeChain mainNextChain, params ModelUpgradeChain[] jumpNextChains)
        {
            var list = new List<ModelUpgradeChain> { mainNextChain };

            if (jumpNextChains != null)
            {
                list.AddRange(jumpNextChains);
            }

            list.Reverse();

            NextChains = list.ToArray();

            CheckModelUpgradeChain(previousVersionType, jumpNextChains);
        }

        internal abstract IVersionModel Upgrade(IVersionModel model);

        internal void CheckModelUpgradeChain(Type targetVersionType, params ModelUpgradeChain[] modelUpgradeChains)
        {
            if (modelUpgradeChains == null || !modelUpgradeChains.Any())
            {
                return;
            }

            var genericArguments = modelUpgradeChains.Select(modelUpgradeChain => modelUpgradeChain?.GetType().BaseType?.GetGenericArguments() ?? Array.Empty<Type>()).ToArray();

            if (genericArguments.Any(lastGenericArguments => lastGenericArguments.Length > 0 && lastGenericArguments[0] != targetVersionType))
            {
                throw new ArgumentException($"{modelUpgradeChains.GetType()} can't convert model to \"{targetVersionType.FullName}\".");
            }
        }
    }

    public abstract class ModelUpgrade<TTargetVersion, TPreviousVersion> : ModelUpgradeChain
        where TTargetVersion : IVersionModel
        where TPreviousVersion : IVersionModel
    {
        protected ModelUpgrade(ModelUpgradeChain mainNextChain, params ModelUpgradeChain[] jumpNextChains) : base(typeof(TPreviousVersion), mainNextChain, jumpNextChains)
        {
        }

        /// <summary>
        /// Upgrades <see cref="TPreviousVersion"/> to <see cref="TTargetVersion"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected abstract TTargetVersion UpgradeFunc(TPreviousVersion model);

        internal override IVersionModel Upgrade(IVersionModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model is TPreviousVersion previousVersion)
            {
                return UpgradeFunc(previousVersion);
            }

            foreach (var next in NextChains)
            {
                var result = model;

                result = next.Upgrade(result);

                if (result is TPreviousVersion previousVersion1)
                {
                    return UpgradeFunc(previousVersion1);
                }
            }

            return model;
        }
    }
}
