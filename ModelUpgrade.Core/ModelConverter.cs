using System;
using System.Linq;

namespace ModelUpgrade.Core
{
    /// <summary>
    /// Helps different version models convert between <see cref="DataModel"/> and the newest version model.
    /// </summary>
    /// <typeparam name="TNewestModel">The type of <see cref="IVersionModel"/>.</typeparam>
    public sealed class ModelConverter<TNewestModel>
        where TNewestModel : IVersionModel
    {
        private readonly IModelUpgrade _modelUpgrade;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelConverter{TNewestModel}"/> class.
        /// </summary>
        /// <param name="modelUpgrade">The model upgrade.</param>
        public ModelConverter(IModelUpgrade modelUpgrade)
        {
            _modelUpgrade = modelUpgrade;
        }

        /// <summary>
        /// Parses <see cref="IVersionModel"/> to <see cref="DataModel"/>.
        /// </summary>
        /// <param name="model"><see cref="IVersionModel"/></param>
        /// <returns></returns>
        public DataModel Parse(IVersionModel model)
        {
            if (model == null)
            {
                return null;
            }

            model = UpgradeToNewest(model);

            return new DataModel
            {
                Id = model.GetId(),
                Data = _modelUpgrade.Serialize(model),
                ModelName = model.GetModelName()
            };
        }

        /// <summary>
        /// Parses <see cref="DataModel"/> to <see cref="IVersionModel"/>.
        /// </summary>
        /// <param name="model"><see cref="DataModel"/></param>
        /// <returns></returns>
        public TNewestModel Parse(DataModel model)
        {
            var newModel = GetNewest(model);

            var obj = _modelUpgrade.Deserialize<TNewestModel>(newModel.Data);

            return obj;
        }

        /// <summary>
        /// Upgrades your old version model to the newest version model.
        /// </summary>
        /// <typeparam name="T"><see cref="IVersionModel"/></typeparam>
        /// <param name="model">The old version model.</param>
        /// <returns></returns>
        public TNewestModel Upgrade<T>(T model) where T : IVersionModel
        {
            var dataModel = new DataModel(model, _modelUpgrade.Serialize);

            return Parse(dataModel);
        }

        private readonly Lazy<Type[]> _versionTypes = new Lazy<Type[]>(() =>
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IVersionModel).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToArray();
        });

        private DataModel GetNewest(DataModel model)
        {
            var modelType = _versionTypes.Value.FirstOrDefault(x => string.Equals(x.Name, model.ModelName, StringComparison.CurrentCultureIgnoreCase));

            if (modelType == null)
            {
                throw new Exception($"Can't find IVersionModel type: \'{model.ModelName}\'");
            }

            var getConverterMethod = typeof(IModelUpgrade).GetMethod(nameof(IModelUpgrade.Deserialize));

            if (getConverterMethod == null)
            {
                throw new Exception("Can't find Method 'IDataExtension.Deserialize'");
            }

            var method = getConverterMethod.MakeGenericMethod(modelType);

            if (method == null)
            {
                throw new Exception("Can't find Generics Method 'IDataExtension.Deserialize'");
            }

            if (!(method.Invoke(_modelUpgrade, new object[] { model.Data }) is IVersionModel versionModel))
            {
                throw new Exception("Converted model's type is not IVersionModel");
            }

            var upgradedModel = UpgradeToNewest(versionModel);

            var modelData = _modelUpgrade.Serialize(upgradedModel);

            return new DataModel
            {
                Id = model.Id,
                Data = modelData,
                ModelName = upgradedModel.GetModelName()
            };
        }

        private TNewestModel UpgradeToNewest(IVersionModel model)
        {
            while (!(model is TNewestModel))
            {
                model = _modelUpgrade.Upgrade(model);
            }

            return (TNewestModel)model;
        }
    }
}
