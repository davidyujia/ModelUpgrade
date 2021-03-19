using System;

namespace ModelUpgrade.Core
{
    public abstract class ModelUpgradeChain
    {
        internal readonly ModelUpgradeChain LastVersionUpgrade;

        protected ModelUpgradeChain(ModelUpgradeChain lastVersionUpgrade)
        {
            LastVersionUpgrade = lastVersionUpgrade;
        }

        internal abstract IVersionModel Upgrade(IVersionModel model);

        internal void CheckModelUpgradeChain(ModelUpgradeChain modelUpgradeChain, Type targetVersionType)
        {
            if (modelUpgradeChain == null)
            {
                return;
            }

            var lastGenericArguments = modelUpgradeChain.GetType().BaseType?.GetGenericArguments() ?? Array.Empty<Type>();

            if (lastGenericArguments.Length > 0 && lastGenericArguments[0] != targetVersionType)
            {
                throw new ArgumentException($"{modelUpgradeChain.GetType()} can't convert model to \"{targetVersionType.FullName}\".");
            }
        }
    }

    public abstract class ModelUpgrade<TTargetVersion, TPreviousVersion> : ModelUpgradeChain
        where TTargetVersion : IVersionModel
        where TPreviousVersion : IVersionModel
    {
        protected ModelUpgrade(ModelUpgradeChain lastVersionUpgrade) : base(lastVersionUpgrade)
        {
            CheckModelUpgradeChain(lastVersionUpgrade, typeof(TPreviousVersion));
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

            var result = model;

            if (!(result is TPreviousVersion) && LastVersionUpgrade != null)
            {
                result = LastVersionUpgrade.Upgrade(result);
            }

            return result is TPreviousVersion previousVersion ? UpgradeFunc(previousVersion) : model;
        }
    }
}
