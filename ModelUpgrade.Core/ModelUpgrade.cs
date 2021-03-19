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
    }

    public abstract class ModelUpgrade<TTargetVersion, TPreviousVersion> : ModelUpgradeChain
        where TTargetVersion : IVersionModel
        where TPreviousVersion : IVersionModel
    {
        protected ModelUpgrade(ModelUpgradeChain lastVersionUpgrade) : base(lastVersionUpgrade)
        {
            CheckLastVersionUpgrade(lastVersionUpgrade);
        }

        private static void CheckLastVersionUpgrade(ModelUpgradeChain lastVersionUpgrade)
        {
            if (lastVersionUpgrade == null)
            {
                return;
            }

            var lastGenericArguments = lastVersionUpgrade?.GetType().BaseType?.GetGenericArguments() ?? Array.Empty<Type>();

            if (lastGenericArguments.Length > 0 && lastGenericArguments[0] != typeof(TPreviousVersion))
            {
                throw new ArgumentException($"{lastVersionUpgrade.GetType()} can't convert model to \"{typeof(TPreviousVersion).FullName}\".");
            }
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
