using System;
using System.Collections.Generic;
using ModelUpgrade.Extensions;

namespace ModelUpgrade
{
    /// <summary>
    /// Base model upgrade chain with target version model type. (DO NOT INHERITANCE THIS, ONLY FOR FOOLPROOF)
    /// </summary>
    public abstract class ModelUpgradeBase<TTargetVersion>
    {
        internal readonly IDictionary<Type, int> EnableUpgradeModelTypeCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpgradeBase{TTargetVersion}" /> class.
        /// </summary>
        /// <param name="previousVersionType">Type of the previous version.</param>
        internal ModelUpgradeBase(Type previousVersionType)
        {
            EnableUpgradeModelTypeCount = new Dictionary<Type, int> { { previousVersionType, 0 } };
        }

        internal abstract TTargetVersion UpgradeBase(object model);
    }

    /// <summary>
    /// Base model upgrade chain
    /// </summary>
    /// <typeparam name="TPreviousVersion">The type of the previous version.</typeparam>
    /// <typeparam name="TTargetVersion">The type of the target version.</typeparam>
    /// <seealso cref="ModelUpgradeBase{TTargetVersion}" />
    public abstract class ModelUpgrade<TPreviousVersion, TTargetVersion> : ModelUpgradeBase<TTargetVersion>
    {
        internal readonly IDictionary<Type, ModelUpgradeBase<TPreviousVersion>> Chains;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpgrade{TPreviousVersion, TTargetVersion}"/> class.
        /// </summary>
        /// <param name="nextChains">The next chains.</param>
        protected ModelUpgrade(ModelUpgradeBase<TPreviousVersion>[] nextChains) : base(typeof(TPreviousVersion))
        {
            Chains = new Dictionary<Type, ModelUpgradeBase<TPreviousVersion>>();

            nextChains?.ForEach(Add);
        }

        /// <summary>
        /// Upgrade model from previous version to target version />.
        /// </summary>
        /// <param name="model">previous version model.</param>
        /// <returns>target version model</returns>
        protected abstract TTargetVersion UpgradeFunc(TPreviousVersion model);

        internal sealed override TTargetVersion UpgradeBase(object model)
        {
            switch (model)
            {
                case null:
                    throw new ArgumentNullException(nameof(model));
                case TTargetVersion targetVersion:
                    return targetVersion;
                case TPreviousVersion previousVersion:
                    return UpgradeFunc(previousVersion);
            }

            return UpgradeFromChains(model);
        }

        private TTargetVersion UpgradeFromChains(object model)
        {
            var modelType = model.GetType();

            if (!Chains.ContainsKey(modelType))
            {
                throw new Exception($"Can't find chain to convert \"{modelType.FullName}\".");
            }

            var chain = Chains[modelType];

            var result = chain.UpgradeBase(model);

            return UpgradeFunc(result);
        }

        /// <summary>
        /// Upgrades the model to target version />.
        /// </summary>
        /// <param name="model">The model which you'd like upgrade.</param>
        /// <returns>target version model</returns>
        public TTargetVersion Upgrade(object model)
        {
            return UpgradeBase(model);
        }

        /// <summary>
        /// Add <see cref="ModelUpgradeBase{TTargetVersion}"/> chain.
        /// </summary>
        /// <param name="chain">The chain.</param>
        public void Add(ModelUpgradeBase<TPreviousVersion> chain)
        {
            chain.EnableUpgradeModelTypeCount.ForEach(typeCountItem =>
            {
                var typeCount = typeCountItem.Value + 1;

                if (!EnableUpgradeModelTypeCount.ContainsKey(typeCountItem.Key))
                {
                    EnableUpgradeModelTypeCount.Add(typeCountItem.Key, typeCount);
                    Chains.Add(typeCountItem.Key, chain);
                    return;
                }

                if (EnableUpgradeModelTypeCount[typeCountItem.Key] <= typeCount)
                {
                    return;
                }

                EnableUpgradeModelTypeCount[typeCountItem.Key] = typeCount;
                Chains[typeCountItem.Key] = chain;
            });
        }
    }
}
