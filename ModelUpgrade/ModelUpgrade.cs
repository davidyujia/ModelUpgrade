using System;
using System.Collections.Generic;
using ModelUpgrade.Extensions;

namespace ModelUpgrade
{
    /// <summary>
    /// Base model upgrade chain. (DO NOT INHERITANCE THIS)
    /// </summary>
    public abstract class ModelUpgradeChain
    {
        internal readonly IDictionary<Type, int> EnableUpgradeModelTypeCount;

        internal readonly IDictionary<Type, ModelUpgradeChain> Chains;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpgradeChain"/> class.
        /// </summary>
        /// <param name="previousVersionType">Type of the previous version.</param>
        /// <param name="nextChains">The next chains.</param>
        protected ModelUpgradeChain(Type previousVersionType, ModelUpgradeChain[] nextChains)
        {
            SetGenericType();
            Chains = new Dictionary<Type, ModelUpgradeChain>();
            EnableUpgradeModelTypeCount = new Dictionary<Type, int> { { previousVersionType, 0 } };

            if (nextChains == null)
            {
                return;
            }

            ModelUpgradeExtension.CheckModelUpgradeChain(previousVersionType, nextChains);

            nextChains.ForEach(AddBase);
        }

        private void SetGenericType()
        {
            var types = GetType().BaseType?.GetGenericArguments() ?? Array.Empty<Type>();

            if (types.Length != 2)
            {
                return;
            }

            TargetType = types[1];
            PreviousType = types[0];
        }

        internal Type TargetType { get; private set; }

        internal Type PreviousType { get; private set; }
        
        internal void AddBase(ModelUpgradeChain chain)
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
        
        internal abstract object UpgradeBase(object model);
    }

    /// <summary>
    /// Base model upgrade chain with target version model type. (DO NOT INHERITANCE THIS, ONLY FOR FOOLPROOF)
    /// </summary>
    public abstract class ModelUpgradeChain<TTargetVersion> : ModelUpgradeChain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpgradeChain{TTargetVersion}"/> class.
        /// </summary>
        /// <param name="previousVersionType">Type of the previous version.</param>
        /// <param name="nextChains">The next chains.</param>
        protected ModelUpgradeChain(Type previousVersionType, ModelUpgradeChain[] nextChains) : base(previousVersionType, nextChains) { }
    }

    /// <summary>
    /// Base model upgrade chain
    /// </summary>
    /// <typeparam name="TPreviousVersion">The type of the previous version.</typeparam>
    /// <typeparam name="TTargetVersion">The type of the target version.</typeparam>
    /// <seealso cref="ModelUpgradeChain{TTargetVersion}" />
    public abstract class ModelUpgrade<TPreviousVersion, TTargetVersion> : ModelUpgradeChain<TTargetVersion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpgrade{TPreviousVersion, TTargetVersion}"/> class.
        /// </summary>
        /// <param name="nextChains">The next chains.</param>
        protected ModelUpgrade(ModelUpgradeChain<TPreviousVersion>[] nextChains) : base(typeof(TPreviousVersion), nextChains) { }

        /// <summary>
        /// Upgrade model from previous version to target version />.
        /// </summary>
        /// <param name="model">previous version model.</param>
        /// <returns>target version model</returns>
        protected abstract TTargetVersion UpgradeFunc(TPreviousVersion model);

        internal override object UpgradeBase(object model)
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

        private object UpgradeFromChains(object model)
        {
            var modelType = model.GetType();

            if (!Chains.ContainsKey(modelType))
            {
                throw new Exception($"Can't find chain to convert \"{modelType.FullName}\"");
            }

            var chain = Chains[modelType];

            var result = chain.UpgradeBase(model);

            return UpgradeBase(result);
        }

        /// <summary>
        /// Upgrades the model to target version />.
        /// </summary>
        /// <param name="model">The model which you'd like upgrade.</param>
        /// <returns>target version model</returns>
        public TTargetVersion Upgrade(object model)
        {
            return (TTargetVersion)UpgradeBase(model);
        }

        /// <summary>
        /// Add <see cref="ModelUpgradeChain&lt;TPreviousVersion&gt;"/> chain.
        /// </summary>
        /// <param name="chain">The chain.</param>
        public void Add(ModelUpgradeChain<TPreviousVersion> chain)
        {
            AddBase(chain);
        }
    }
}
