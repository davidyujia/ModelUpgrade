# ModelUpgradeSolution

## How to use

Upgrade your model to latest version.

```cs
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
```

Jump Upgrade your model.

```cs
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
```

Upgrade your store data with primary key to latest version.

```cs
// Create a converter.
var modelSerializer = new MyModelSerializer();
var converter = new ModelConverter<Version3>(modelSerializer, v2UpgradeChain);

// Sample data, it's from database.
var v1DbData = new DataModel(v1Model, modelSerializer.Serialize);

// Parses your saved data to the v3 model.
var v3ModelFromConvert = converter.Parse(v1DbData);

// Parses v3 model to data model for saving.
var v3DbModel = converter.Parse(v3ModelFromConvert);
```

## Before upgrade

Must create your upgrade chains to ready.

```cs
class MyVersion1To2Upgrade : ModelUpgrade<Version2, Version1>
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

class MyVersion2To3Upgrade : ModelUpgrade<Version3, Version2>
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
```
