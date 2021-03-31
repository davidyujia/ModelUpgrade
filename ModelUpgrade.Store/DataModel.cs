using System;

namespace ModelUpgrade.Store
{
    /// <summary>
    /// Data for saving
    /// </summary>
    public sealed class DataModel
    {
        public DataModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataModel"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parseFunc">The parse function.</param>
        public DataModel(IVersionModel model, Func<IVersionModel, string> parseFunc)
        {
            Id = model.GetId();
            Data = parseFunc(model);
            ModelName = model.GetModelName();
        }

        public string Id { get; set; }

        public string Data { get; set; }

        public string ModelName { get; set; }
    }
}
