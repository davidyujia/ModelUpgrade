using System;
using System.Collections.Generic;
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

        internal readonly Dictionary<Type, ModelUpgradeChain> JumpNextChains;

        protected ModelUpgradeChain(Type previousVersionType, params ModelUpgradeChain[] nextChains)
        {
            JumpNextChains = new Dictionary<Type, ModelUpgradeChain>();
            _enableUpgradeModelTypeCount = new Dictionary<Type, int> { { previousVersionType, 0 } };

            if (nextChains == null)
            {
                return;
            }

            foreach (var chain in nextChains)
            {
                Add(chain);
            }

            ModelUpgradeExtension.CheckModelUpgradeChain(previousVersionType, nextChains);
        }

        public void Add(ModelUpgradeChain chain)
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
                JumpNextChains.Add(targetType, chain);
                return;
            }

            if (_enableUpgradeModelTypeCount[targetType] <= typeCount)
            {
                return;
            }

            _enableUpgradeModelTypeCount[targetType] = typeCount;
            JumpNextChains[targetType] = chain;
        }

        public abstract IVersionModel Upgrade(IVersionModel model);
    }

    public abstract class ModelUpgrade<TTargetVersion, TPreviousVersion> : ModelUpgradeChain
        where TTargetVersion : IVersionModel
        where TPreviousVersion : IVersionModel
    {
        protected ModelUpgrade(ModelUpgradeChain[] nextChains) : base(typeof(TPreviousVersion), nextChains)
        {
        }

        /// <summary>
        /// Upgrades <see cref="TPreviousVersion"/> to <see cref="TTargetVersion"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected abstract TTargetVersion UpgradeFunc(TPreviousVersion model);

        public override IVersionModel Upgrade(IVersionModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model is TPreviousVersion previousVersion1)
            {
                return UpgradeFunc(previousVersion1);
            }

            var modelType = model.GetType();

            if (!JumpNextChains.ContainsKey(modelType))
            {
                return model;
            }

            var chain = JumpNextChains[modelType];

            var upgradedModel = chain.Upgrade(model);

            if (upgradedModel is TPreviousVersion previousVersion2)
            {
                return UpgradeFunc(previousVersion2);
            }

            return model;
        }
    }
}
