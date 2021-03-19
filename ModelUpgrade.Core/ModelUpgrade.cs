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
        }

        protected abstract TTargetVersion UpgradeFunc(TPreviousVersion model);

        internal override IVersionModel Upgrade(IVersionModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            IVersionModel result = null;

            if (!(model is TPreviousVersion) && LastVersionUpgrade != null)
            {
                result = LastVersionUpgrade.Upgrade(model);
            }

            return result is TPreviousVersion previousVersion ? UpgradeFunc(previousVersion) : model;
        }
    }
}
