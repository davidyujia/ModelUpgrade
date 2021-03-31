# ModelUpgradeSolution

```cs
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
```
