using System.Text.Json;
using ModelUpgrade.Core;

namespace SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a model upgrade chain, this chain must from oldest version to latest version.
            var v1Upgrade = new Version1Upgrade(null);
            var v2Upgrade = new Version2Upgrade(v1Upgrade);

            // Create a converter.
            var modelSerializer = new MyModelSerializer();
            var converter = new ModelConverter<Version3>(modelSerializer, v2Upgrade);

            // Sample data, it's from database.
            var dbData = new DataModel(new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
            }, modelSerializer.Serialize);

            // Parses your saved data to the v3 model.
            var v3 = converter.Parse(dbData);

            // Parses v3 model to data model for saving.
            var v3DbModel = converter.Parse(v3);

            var v3DbModelFormV1Model = converter.Upgrade(new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
            });

        }
    }

    class Version1Upgrade : ModelUpgrade<Version2, Version1>
    {
        public Version1Upgrade(ModelUpgradeChain lastVersionUpgrade) : base(lastVersionUpgrade)
        {
        }

        protected override Version2 UpgradeFunc(Version1 model)
        {
            return new Version2
            {
                Id = model.Uid,
                ProjectName = model.Name
            };
        }
    }

    class Version2Upgrade : ModelUpgrade<Version3, Version2>
    {
        public Version2Upgrade(ModelUpgradeChain lastVersionUpgrade) : base(lastVersionUpgrade)
        {
        }

        protected override Version3 UpgradeFunc(Version2 model)
        {
            return new Version3
            {
                ProjectId = model.Id,
                ProjectName = model.ProjectName
            };
        }
    }

    class MyModelSerializer : ModelSerializer
    {
        public override T Deserialize<T>(string s)
        {
            return JsonSerializer.Deserialize<T>(s);
        }

        public override string Serialize(IVersionModel model)
        {
            return JsonSerializer.Serialize((object)model);
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
