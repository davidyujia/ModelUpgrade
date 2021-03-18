using System;
using System.Collections.Generic;
using System.Text;

namespace ModelUpgrade.Core
{
    public abstract class UpgradeHandler<T> where T : IVersionModel
    {
        private readonly UpgradeHandler<IVersionModel> _nextHandler;

        protected UpgradeHandler(UpgradeHandler<IVersionModel> nextHandler = null)
        {
            _nextHandler = nextHandler;
        }

        protected abstract IVersionModel UpgradeFunc(T model);

        public IVersionModel Upgrade(T model) => model.GetType() == typeof(T) ? UpgradeFunc(model) : _nextHandler?.Upgrade(model);
    }
}
