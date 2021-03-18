using System;
using System.Text.Json;
using ModelUpgrade.Core;

namespace SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a converter.
            var modelUpgrade = new MyModelUpgrade();
            var converter = new ModelConverter<Version3>(modelUpgrade);

            // Sample data, it's from database.
            var dbData = new DataModel(new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
            }, modelUpgrade.Serialize);

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

    class MyModelUpgrade : ModelUpgrade.Core.ModelUpgrade
    {
        public T Deserialize<T>(string s) where T : IVersionModel
        {
            return JsonSerializer.Deserialize<T>(s);
        }

        public string Serialize(IVersionModel model)
        {
            return JsonSerializer.Serialize((object)model);
        }

        public IVersionModel Upgrade<T>(T model) where T : IVersionModel
        {
            return model switch
            {
                Version1 v1 => V1ToV2(v1),
                Version2 v2 => V2ToV3(v2),
                _ => throw new Exception()
            };
        }

        private static Version2 V1ToV2(Version1 v1)
        {
            return new Version2
            {
                Id = v1.Uid,
                ProjectName = v1.Name
            };
        }

        private static Version3 V2ToV3(Version2 v2)
        {
            return new Version3
            {
                ProjectId = v2.Id,
                ProjectName = v2.ProjectName
            };
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
