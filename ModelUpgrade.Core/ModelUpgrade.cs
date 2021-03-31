using System;
using System.Collections.Generic;
using System.Linq;
using ModelUpgrade.Core.Extensions;

namespace ModelUpgrade.Core
{
    public abstract class ModelUpgradeChain
    {
        private readonly Dictionary<Type, int> _enableUpgradeModelTypeCount;

        internal Dictionary<Type, int> GetEnableUpgradeModelTypeCount()
        {
            return _enableUpgradeModelTypeCount;
        }

        internal readonly Dictionary<Type, ModelUpgradeChain> Chains;

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

        //public abstract IVersionModel Upgrade(IVersionModel model);

        internal abstract object UpgradeBase(object model);
    }

    public abstract class ModelUpgradeChain<TTargetVersion> : ModelUpgradeChain
    {
        protected ModelUpgradeChain(Type previousVersionType, ModelUpgradeChain[] nextChains) : base(previousVersionType, nextChains) { }

        public abstract TTargetVersion Upgrade(object model);
    }

    public abstract class ModelUpgrade<TPreviousVersion, TTargetVersion> : ModelUpgradeChain<TTargetVersion>
    {
        protected ModelUpgrade(ModelUpgradeChain<TPreviousVersion>[] nextChains) : base(typeof(TPreviousVersion), nextChains as ModelUpgradeChain[]) { }

        /// <summary>
        /// Upgrade model from <see cref="TPreviousVersion"/> to <see cref="TTargetVersion"/>.
        /// </summary>
        /// <param name="model"><see cref="TPreviousVersion"/> model.</param>
        /// <returns></returns>
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
        /// Upgrades the model to <see cref="TTargetVersion"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">model</exception>
        /// <exception cref="Exception">Can't convert \"{model.GetType().FullName}\"</exception>
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
