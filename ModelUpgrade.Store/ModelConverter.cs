using System;
using System.Linq;
using ModelUpgrade.Core;
using ModelUpgrade.Core.Extensions;

namespace ModelUpgrade.Store
{
    /// <summary>
    /// Helps different version models convert between <see cref="DataModel"/> and the newest version model.
    /// </summary>
    /// <typeparam name="TLatestVersionModel">The type of <see cref="IVersionStoreModel"/>.</typeparam>
    public sealed class ModelConverter<TLatestVersionModel> where TLatestVersionModel : IVersionStoreModel
    {
        private readonly IModelSerializer _modelSerializer;
        private readonly ModelUpgradeChain _modelUpgrade;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelConverter{TLatestVersionModel}" /> class.
        /// </summary>
        /// <param name="modelSerializer">The model upgrade.</param>
        /// <param name="modelUpgrade">The model upgrade.</param>
        public ModelConverter(IModelSerializer modelSerializer, ModelUpgradeChain modelUpgrade)
        {
            _modelSerializer = modelSerializer;
            _modelUpgrade = modelUpgrade;

            ModelUpgradeExtension.CheckModelUpgradeChain(typeof(TLatestVersionModel), modelUpgrade);
        }

        /// <summary>
        /// Parses <see cref="IVersionStoreModel" /> to <see cref="DataModel" />.
        /// </summary>
        /// <param name="model"><see cref="IVersionStoreModel" /></param>
        /// <returns></returns>
        public DataModel Parse(IVersionStoreModel model)
        {
            if (model == null)
            {
                return null;
            }

            model = Upgrade(model);

            return new DataModel
            {
                Id = model.GetId(),
                Data = _modelSerializer.Serialize(model),
                ModelName = model.GetModelName()
            };
        }

        /// <summary>
        /// Parses <see cref="DataModel"/> to <see cref="IVersionStoreModel"/>.
        /// </summary>
        /// <param name="model"><see cref="DataModel"/></param>
        /// <returns></returns>
        public TLatestVersionModel Parse(DataModel model)
        {
            var newModel = Upgrade(model);

            var obj = _modelSerializer.Deserialize<TLatestVersionModel>(newModel.Data);

            return obj;
        }

        /// <summary>
        /// Upgrades your old version model to the newest version model.
        /// </summary>
        /// <typeparam name="T"><see cref="IVersionStoreModel"/></typeparam>
        /// <param name="model">The old version model.</param>
        /// <returns></returns>
        public TLatestVersionModel Upgrade<T>(T model) where T : IVersionModel
        {
            if (model is TLatestVersionModel currentVersion)
            {
                return currentVersion;
            }

            var result = _modelUpgrade.Upgrade(model);

            if (!(result is TLatestVersionModel))
            {
                throw new Exception($"{model.GetType().FullName} can't upgrade to {typeof(TLatestVersionModel).FullName}, please check your ModelUpgradeChain is complete and \"{result.GetType().FullName} is inheritance \"{typeof(IVersionStoreModel).FullName}\" \".");
            }

            return (TLatestVersionModel)result;
        }

        private readonly Lazy<Type[]> _versionTypes = new Lazy<Type[]>(() =>
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IVersionModel).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToArray();
        });

        private DataModel Upgrade(DataModel model)
        {
            var modelType = _versionTypes.Value.FirstOrDefault(x => string.Equals(x.Name, model.ModelName, StringComparison.CurrentCultureIgnoreCase));

            if (modelType == null)
            {
                throw new Exception($"Can't find IVersionModel type: \"{model.ModelName}\"");
            }

            var getConverterMethod = typeof(IModelSerializer).GetMethod(nameof(IModelSerializer.Deserialize));

            if (getConverterMethod == null)
            {
                throw new Exception("Can't find method \"IModelSerializer.Deserialize\"");
            }

            var method = getConverterMethod.MakeGenericMethod(modelType);

            if (method == null)
            {
                throw new Exception("Can't find generics method \"IModelSerializer.Deserialize\"");
            }

            if (!(method.Invoke(_modelSerializer, new object[] { model.Data }) is IVersionModel versionModel))
            {
                throw new Exception("Converted model's type is not IVersionModel");
            }

            var upgradedModel = Upgrade(versionModel);

            var modelData = _modelSerializer.Serialize(upgradedModel);

            return new DataModel
            {
                Id = model.Id,
                Data = modelData,
                ModelName = upgradedModel.GetModelName()
            };
        }
    }
}
