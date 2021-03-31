using System.Text.Json;
using ModelUpgrade.Core;
using ModelUpgrade.Store;

namespace SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a model upgrade chain, this chain must from oldest version to latest version.
            var v1UpgradeChain = new Version1Upgrade();
            var v2UpgradeChain = new Version2Upgrade(v1UpgradeChain);

            // Sample data.
            var v1Model = new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
            };

            // Upgrade sample to latest version
            var v3Model = v2UpgradeChain.Upgrade(v1Model);

            // Create a converter.
            var modelSerializer = new MyModelSerializer();
            var converter = new ModelConverter<Version3>(modelSerializer, v2UpgradeChain);

            // Sample data, it's from database.
            var v1DbData = new DataModel(v1Model, modelSerializer.Serialize);

            // Parses your saved data to the v3 model.
            var v3ModelFromConvert = converter.Parse(v1DbData);

            // Parses v3 model to data model for saving.
            var v3DbModel = converter.Parse(v3ModelFromConvert);
        }
    }

    class Version1Upgrade : ModelUpgrade<Version2, Version1>
    {
        protected override Version2 UpgradeFunc(Version1 model) => new Version2
        {
            Id = model.Uid,
            ProjectName = model.Name
        };

        public Version1Upgrade() : base(null)
        {
        }
    }

    class Version2Upgrade : ModelUpgrade<Version3, Version2>
    {
        protected override Version3 UpgradeFunc(Version2 model) => new Version3
        {
            ProjectId = model.Id,
            ProjectName = model.ProjectName
        };

        public Version2Upgrade(params ModelUpgradeChain[] nextChains) : base(nextChains)
        {
        }
    }

    class MyModelSerializer : IModelSerializer
    {
        public T Deserialize<T>(string s)
        {
            return JsonSerializer.Deserialize<T>(s);
        }

        public string Serialize(object model)
        {
            return JsonSerializer.Serialize(model);
        }
    }

    class Version1 : IVersionModel
    {
        public string GetId()
        {
            return Uid;
        }

        public string GetModelName()
        {
            return GetType().Name;
        }

        public string Uid { get; set; }
        public string Name { get; set; }
    }

    class Version2 : IVersionModel
    {
        public string GetId()
        {
            return Id;
        }

        public string GetModelName()
        {
            return GetType().Name;
        }
        public string Id { get; set; }
        public string ProjectName { get; set; }
    }

    class Version3 : IVersionModel
    {
        public string GetId()
        {
            return ProjectId;
        }

        public string GetModelName()
        {
            return GetType().Name;
        }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
}
