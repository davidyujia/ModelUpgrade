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
                var chainEnableUpgradeModelTypeCount = chain.GetEnableUpgradeModelTypeCount();

                foreach (var typeCountPair in chainEnableUpgradeModelTypeCount)
                {
                    var typeCount = typeCountPair.Value + 1;

                    if (!_enableUpgradeModelTypeCount.ContainsKey(typeCountPair.Key))
                    {
                        _enableUpgradeModelTypeCount.Add(typeCountPair.Key, typeCount);
                        JumpNextChains.Add(typeCountPair.Key, chain);
                        continue;
                    }

                    if (_enableUpgradeModelTypeCount[typeCountPair.Key] <= typeCount)
                    {
                        continue;
                    }

                    _enableUpgradeModelTypeCount[typeCountPair.Key] = typeCount;
                    JumpNextChains[typeCountPair.Key] = chain;
                }
            }

            ModelUpgradeExtension.CheckModelUpgradeChain(previousVersionType, nextChains);
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
