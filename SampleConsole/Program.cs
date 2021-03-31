using System.Text.Json;
using ModelUpgrade.Core;
using ModelUpgrade.Store;

namespace SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Upgrade();

            JumpUpgrade();

            DbModelUpgrade();
        }

        static void Upgrade()
        {
            // Create a model upgrade chain, this chain must from oldest version to latest version.
            var v1Upgrade = new MyVersion1To2Upgrade();
            var v2Upgrade = new MyVersion2To3Upgrade(v1Upgrade);

            // Sample data.
            var v1Model = new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
            };

            // Upgrade sample to latest version
            var v3Model = v2Upgrade.Upgrade(v1Model);

            // Create a converter.
            var modelSerializer = new MyModelSerializer();
            var converter = new ModelConverter<Version3>(modelSerializer, v2Upgrade);

            // Sample data, it's from database.
            var v1DbData = new DataModel(v1Model, modelSerializer.Serialize);

            // Parses your saved data to the v3 model.
            var v3ModelFromConvert = converter.Parse(v1DbData);

            // Parses v3 model to data model for saving.
            var v3DbModel = converter.Parse(v3ModelFromConvert);
        }

        static void JumpUpgrade()
        {
            // Sample data.
            var v1Model = new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
            };

            var v2Model = new Version2
            {
                Id = "TestV2",
                ProjectName = "Test2",
            };

            // Create a model upgrade chain, this chain must from oldest version to latest version.
            var v1JumpToV4Upgrade = new MyVersion1To4Upgrade();

            var v2Upgrade = new MyVersion2To3Upgrade();
            var v3Upgrade = new MyVersion3To4Upgrade(v2Upgrade);

            var v4Upgrade = new MyVersion4To5Upgrade(v3Upgrade, v1JumpToV4Upgrade);

            // Also can use this method to add chains
            v4Upgrade.Add(v1JumpToV4Upgrade);

            // v1 model jump upgrade to v5
            var v1ToV5Model = v4Upgrade.Upgrade(v1Model);

            // v2 model upgrade to v5
            var v2ToV5Model = v4Upgrade.Upgrade(v2Model);
        }

        static void DbModelUpgrade()
        {
            // Create a model upgrade chain, this chain must from oldest version to latest version.
            var v1Upgrade = new MyVersion1To2Upgrade();
            var v2Upgrade = new MyVersion2To3Upgrade(v1Upgrade);

            // Sample data.
            var v1Model = new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
            };

            // Create a converter.
            var modelSerializer = new MyModelSerializer();
            var converter = new ModelConverter<Version3>(modelSerializer, v2Upgrade);

            // Sample data, it's from database.
            var v1DbData = new DataModel(v1Model, modelSerializer.Serialize);

            // Parses your saved data to the v3 model.
            var v3ModelFromConvert = converter.Parse(v1DbData);

            // Parses v3 model to data model for saving.
            var v3DbModel = converter.Parse(v3ModelFromConvert);
        }
    }

    class MyVersion1To2Upgrade : ModelUpgrade<Version1, Version2>
    {
        protected override Version2 UpgradeFunc(Version1 model) => new Version2
        {
            Id = model.Uid,
            ProjectName = model.Name
        };

        public MyVersion1To2Upgrade() : base(null)
        {
        }
    }

    class MyVersion2To3Upgrade : ModelUpgrade<Version2, Version3>
    {
        protected override Version3 UpgradeFunc(Version2 model) => new Version3
        {
            ProjectId = model.Id,
            ProjectName = model.ProjectName
        };

        public MyVersion2To3Upgrade(params ModelUpgradeChain<Version2>[] nextChains) : base(nextChains)
        {
        }
    }

    class MyVersion3To4Upgrade : ModelUpgrade<Version3, Version4>
    {
        protected override Version4 UpgradeFunc(Version3 model) => new Version4
        {
            ProjectId = model.ProjectId,
            ProjectName = model.ProjectName
        };

        public MyVersion3To4Upgrade(params ModelUpgradeChain<Version3>[] nextChains) : base(nextChains)
        {
        }
    }

    class MyVersion4To5Upgrade : ModelUpgrade<Version4, Version5>
    {
        protected override Version5 UpgradeFunc(Version4 model) => new Version5
        {
            ProjectId = model.ProjectId,
            ProjectName = model.ProjectName
        };

        public MyVersion4To5Upgrade(params ModelUpgradeChain<Version4>[] nextChains) : base(nextChains)
        {
        }
    }

    class MyVersion1To4Upgrade : ModelUpgrade<Version1, Version4>
    {
        protected override Version4 UpgradeFunc(Version1 model) => new Version4
        {
            ProjectId = model.Uid,
            ProjectName = model.Name
        };

        public MyVersion1To4Upgrade() : base(null)
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

    class Version4 : IVersionModel
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

    class Version5 : IVersionModel
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
