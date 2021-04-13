using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModelUpgrade.Tests
{
    [TestClass]
    public class ModelUpgradeUnitTest
    {
        [TestMethod]
        public void UpgradeVersionTest()
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

            Assert.AreEqual(v1Model.Uid, v3Model.ProjectId);
        }

        [TestMethod]
        public void JumpUpgradeVersionTest()
        {
            // Sample data.
            var v1Model = new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
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

            Assert.AreEqual(v1Model.Uid, v1ToV5Model.ProjectId);
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

        public MyVersion2To3Upgrade(params ModelUpgradeBase<Version2>[] nextChains) : base(nextChains)
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

        public MyVersion3To4Upgrade(params ModelUpgradeBase<Version3>[] nextChains) : base(nextChains)
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

        public MyVersion4To5Upgrade(params ModelUpgradeBase<Version4>[] nextChains) : base(nextChains)
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

    class Version1
    {
        public string Uid { get; set; }
        public string Name { get; set; }
    }

    class Version2
    {
        public string Id { get; set; }
        public string ProjectName { get; set; }
    }

    class Version3
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
    }

    class Version4
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
    }

    class Version5
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
}
