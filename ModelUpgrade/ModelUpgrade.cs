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
        private readonly IDictionary<Type, int> _enableUpgradeModelTypeCount;

        internal IDictionary<Type, int> GetEnableUpgradeModelTypeCount()
        {
            return _enableUpgradeModelTypeCount;
        }

        internal readonly IDictionary<Type, ModelUpgradeChain> Chains;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpgradeChain"/> class.
        /// </summary>
        /// <param name="previousVersionType">Type of the previous version.</param>
        /// <param name="nextChains">The next chains.</param>
        protected ModelUpgradeChain(Type previousVersionType, params ModelUpgradeChain[] nextChains)
        {
            Chains = new Dictionary<Type, ModelUpgradeChain>();
            _enableUpgradeModelTypeCount = new Dictionary<Type, int> { { previousVersionType, 0 } };

            if (nextChains == null)
            {
                return;
            }

            foreach (var chain in nextChains)
            {
                AddBase(chain);
            }

            ModelUpgradeExtension.CheckModelUpgradeChain(previousVersionType, nextChains);
        }

        internal void AddBase(ModelUpgradeChain chain)
        {
            var chainEnableUpgradeModelTypeCount = chain.GetEnableUpgradeModelTypeCount();

            foreach (var typeCountPair in chainEnableUpgradeModelTypeCount)
            {
                SetTypeCount(chain, typeCountPair.Key, typeCountPair.Value);
            }
        }

        private void SetTypeCount(ModelUpgradeChain chain, Type targetType, int targetLength)
        {
            var typeCount = targetLength + 1;

            if (!_enableUpgradeModelTypeCount.ContainsKey(targetType))
            {
                _enableUpgradeModelTypeCount.Add(targetType, typeCount);
                Chains.Add(targetType, chain);
                return;
            }

            if (_enableUpgradeModelTypeCount[targetType] <= typeCount)
            {
                return;
            }

            _enableUpgradeModelTypeCount[targetType] = typeCount;
            Chains[targetType] = chain;
        }

        internal abstract object UpgradeBase(object model);
    }

    /// <summary>
    /// Base model upgrade chain with target version model type. (DO NOT INHERITANCE THIS)
    /// </summary>
    public abstract class ModelUpgradeChain<TTargetVersion> : ModelUpgradeChain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpgradeChain{TTargetVersion}"/> class.
        /// </summary>
        /// <param name="previousVersionType">Type of the previous version.</param>
        /// <param name="nextChains">The next chains.</param>
        protected ModelUpgradeChain(Type previousVersionType, ModelUpgradeChain[] nextChains) : base(previousVersionType, nextChains) { }

        /// <summary>
        /// Upgrades the model to target version />.
        /// </summary>
        /// <param name="model">The model which you'd like upgrade.</param>
        /// <returns>target version model</returns>
        public abstract TTargetVersion Upgrade(object model);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPreviousVersion">The type of the previous version.</typeparam>
    /// <typeparam name="TTargetVersion">The type of the target version.</typeparam>
    /// <seealso cref="ModelUpgrade.ModelUpgradeChain{TTargetVersion}" />
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
                case TPreviousVersion previousVersion1:
                    return UpgradeFunc(previousVersion1);
            }

            var modelType = model.GetType();

            if (!Chains.ContainsKey(modelType))
            {
                throw new Exception($"Can't find chain to convert \"{modelType.FullName}\"");
            }

            var chain = Chains[modelType];

            var result = chain.UpgradeBase(model);

            if (result is TPreviousVersion previousVersion)
            {
                return UpgradeFunc(previousVersion);
            }

            throw new Exception($"Can't convert \"{model.GetType().FullName}\"");
        }

        /// <summary>
        /// Upgrades the model to target version />.
        /// </summary>
        /// <param name="model">The model which you'd like upgrade.</param>
        /// <returns>target version model</returns>
        public override TTargetVersion Upgrade(object model)
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
