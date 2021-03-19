using System;
using System.Linq;

namespace ModelUpgrade.Core
{
    /// <summary>
    /// Helps different version models convert between <see cref="DataModel"/> and the newest version model.
    /// </summary>
    /// <typeparam name="TLatestVersionModel">The type of <see cref="IVersionModel"/>.</typeparam>
    public sealed class ModelConverter<TLatestVersionModel> where TLatestVersionModel : IVersionModel
    {
        private readonly ModelSerializer _modelSerializer;
        private readonly ModelUpgradeChain _modelUpgrade;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelConverter{TLatestVersionModel}" /> class.
        /// </summary>
        /// <param name="modelSerializer">The model upgrade.</param>
        /// <param name="modelUpgrade">The model upgrade.</param>
        public ModelConverter(ModelSerializer modelSerializer, ModelUpgradeChain modelUpgrade)
        {
            _modelSerializer = modelSerializer;
            _modelUpgrade = modelUpgrade;
        }

        /// <summary>
        /// Parses <see cref="IVersionModel" /> to <see cref="DataModel" />.
        /// </summary>
        /// <param name="model"><see cref="IVersionModel" /></param>
        /// <returns></returns>
        public DataModel Parse(IVersionModel model)
        {
            if (model == null)
            {
                return null;
            }

            model = UpgradeToLatest(model);

            return new DataModel
            {
                Id = model.GetId(),
                Data = _modelSerializer.Serialize(model),
                ModelName = model.GetModelName()
            };
        }

        /// <summary>
        /// Parses <see cref="DataModel"/> to <see cref="IVersionModel"/>.
        /// </summary>
        /// <param name="model"><see cref="DataModel"/></param>
        /// <returns></returns>
        public TLatestVersionModel Parse(DataModel model)
        {
            var newModel = GetLatest(model);

            var obj = _modelSerializer.Deserialize<TLatestVersionModel>(newModel.Data);

            return obj;
        }

        /// <summary>
        /// Upgrades your old version model to the newest version model.
        /// </summary>
        /// <typeparam name="T"><see cref="IVersionModel"/></typeparam>
        /// <param name="model">The old version model.</param>
        /// <returns></returns>
        public TLatestVersionModel Upgrade<T>(T model) where T : IVersionModel
        {
            var dataModel = new DataModel(model, _modelSerializer.Serialize);

            return Parse(dataModel);
        }

        private readonly Lazy<Type[]> _versionTypes = new Lazy<Type[]>(() =>
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IVersionModel).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToArray();
        });
        
        private DataModel GetLatest(DataModel model)
        {
            var modelType = _versionTypes.Value.FirstOrDefault(x => string.Equals(x.Name, model.ModelName, StringComparison.CurrentCultureIgnoreCase));

            if (modelType == null)
            {
                throw new Exception($"Can't find IVersionModel type: \'{model.ModelName}\'");
            }

            var getConverterMethod = typeof(ModelSerializer).GetMethod(nameof(ModelSerializer.Deserialize));

            if (getConverterMethod == null)
            {
                throw new Exception("Can't find Method 'IDataExtension.Deserialize'");
            }

            var method = getConverterMethod.MakeGenericMethod(modelType);

            if (method == null)
            {
                throw new Exception("Can't find Generics Method 'IDataExtension.Deserialize'");
            }

            if (!(method.Invoke(_modelSerializer, new object[] { model.Data }) is IVersionModel versionModel))
            {
                throw new Exception("Converted model's type is not IVersionModel");
            }

            var upgradedModel = UpgradeToLatest(versionModel);

            var modelData = _modelSerializer.Serialize(upgradedModel);

            return new DataModel
            {
                Id = model.Id,
                Data = modelData,
                ModelName = upgradedModel.GetModelName()
            };
        }

        private TLatestVersionModel UpgradeToLatest(IVersionModel model)
        {
            var result = _modelUpgrade.Upgrade(model);

            if (!(result is TLatestVersionModel))
            {
                throw new Exception($"{model.GetType().FullName} can't upgrade to {typeof(TLatestVersionModel).FullName}, please check your ModelUpgradeChain is complete.");
            }

            return (TLatestVersionModel)result;
        }
    }
}
