using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SOTFEdit.Model;
using SOTFEdit.Model.Actors;
using SOTFEdit.Model.Map.Static;

namespace SOTFEdit.Tests;

[TestFixture]
public class ModelJsonSerializationTests
{
    [Test]
    public void Position_deserializes_from_expected_json_shape()
    {
        const string json = "{\"x\":1.25,\"y\":-2.5,\"z\":3}";

        var pos = JsonConvert.DeserializeObject<Position>(json);

        pos.Should().NotBeNull();
        pos.X.Should().BeApproximately(1.25f, 0.0001f);
        pos.Y.Should().BeApproximately(-2.5f, 0.0001f);
        pos.Z.Should().BeApproximately(3f, 0.0001f);
    }

    [Test]
    public void Position_serializes_with_lowercase_json_property_names()
    {
        var pos = new Position(1.25f, -2.5f, 3f);

        var obj = JObject.Parse(JsonConvert.SerializeObject(pos));

        obj.Property("x").Should().NotBeNull();
        obj.Property("y").Should().NotBeNull();
        obj.Property("z").Should().NotBeNull();
        obj.Property("X").Should().BeNull();
        obj.Property("Y").Should().BeNull();
        obj.Property("Z").Should().BeNull();

        obj["x"]!.Value<float>().Should().BeApproximately(1.25f, 0.0001f);
        obj["y"]!.Value<float>().Should().BeApproximately(-2.5f, 0.0001f);
        obj["z"]!.Value<float>().Should().BeApproximately(3f, 0.0001f);
    }

    [Test]
    public void Area_deserializes_from_data_json_shape()
    {
        const string json = "{\"name\":\"Cave\",\"areaMask\":2,\"graphMask\":1}";

        var area = JsonConvert.DeserializeObject<Area>(json);

        area.Should().NotBeNull();
        area.Name.Should().Be("Cave");
        area.AreaMask.Should().Be(2);
        area.GraphMask.Should().Be(1);
    }

    [Test]
    public void ElementProfile_deserializes_string_enum_category()
    {
        const string json = "{\"id\":6,\"goldPlatedId\":381,\"category\":\"LogPlank\"}";

        var profile = JsonConvert.DeserializeObject<ElementProfile>(json);

        profile.Should().NotBeNull();
        profile.Id.Should().Be(6);
        profile.GoldPlatedId.Should().Be(381);
        profile.Category.Should().Be(ElementProfileCategory.LogPlank);
    }

    [Test]
    public void ScrewStructure_defaults_nullable_booleans_when_missing()
    {
        const string json = "{\"category\":\"Test\",\"id\":123,\"buildCost\":5,\"icon\":\"test.png\"}";

        var structure = JsonConvert.DeserializeObject<ScrewStructure>(json);

        structure.Should().NotBeNull();
        structure.Category.Should().Be("Test");
        structure.Id.Should().Be(123);
        structure.BuildCost.Should().Be(5);
        structure.Icon.Should().Be("test.png");

        structure.CanFinish.Should().BeTrue();
        structure.CanEdit.Should().BeTrue();
        structure.ShowOnMap.Should().BeTrue();
        structure.IsWeaponHolder.Should().BeFalse();
    }

    [Test]
    public void Configuration_deserializes_from_github_project_and_computes_urls()
    {
        const string json = "{\"githubProject\":\"codengine/SOTFEdit\"}";

        var config = JsonConvert.DeserializeObject<Configuration>(json);

        config.Should().NotBeNull();
        config.LatestTagUrl.Should().Be("https://api.github.com/repos/codengine/SOTFEdit/releases/latest");
        config.ChangelogUrl.Should().Be("https://raw.githubusercontent.com/codengine/SOTFEdit/master/CHANGELOG.md");
    }

    [Test]
    public void Configuration_serializes_computed_urls()
    {
        var config = new Configuration("codengine/SOTFEdit");

        var obj = JObject.Parse(JsonConvert.SerializeObject(config));

        obj.Property("LatestTagUrl").Should().NotBeNull();
        obj.Property("ChangelogUrl").Should().NotBeNull();
        obj.Property("githubProject").Should().BeNull();
    }

    [Test]
    public void ActorType_deserializes_from_data_json_shape()
    {
        const string json =
            "{\"id\":1,\"classification\":\"cannibal\",\"gender\":\"male\",\"image\":\"rabbit.jpg\",\"icon\":\"rabbit.png\"}";

        var actorType = JsonConvert.DeserializeObject<ActorType>(json);

        actorType.Should().NotBeNull();
        actorType.Id.Should().Be(1);
        actorType.Classification.Should().Be("cannibal");
        actorType.Gender.Should().Be("male");
        actorType.Image.Should().Be("rabbit.jpg");
        actorType.Icon.Should().Be("rabbit.png");
    }

    [Test]
    public void GenericSetting_roundtrips_via_json()
    {
        var setting = new GenericSetting("test", GenericSetting.DataType.Boolean, "some.path")
        {
            BoolValue = true,
            IntValue = 42,
            StringValue = "hello",
            SelectedItem = null,
            MinInt = 1,
            MaxInt = 100
        };

        var copy = JsonConvert.DeserializeObject<GenericSetting>(JsonConvert.SerializeObject(setting));

        copy.Should().NotBeNull();
        copy.Name.Should().Be("test");
        copy.Type.Should().Be(GenericSetting.DataType.Boolean);
        copy.DataPath.Should().Be("some.path");
        copy.BoolValue.Should().BeTrue();
        copy.IntValue.Should().Be(42);
        copy.StringValue.Should().Be("hello");
        copy.MinInt.Should().Be(1);
        copy.MaxInt.Should().Be(100);
    }

    [Test]
    public void Teleport_deserializes_and_ToPosition_uses_coordinates_and_area_mask()
    {
        const string json = "{\"x\":-1157.58,\"y\":66.96,\"z\":17.78,\"areaMask\":1048576}";

        var teleport = JsonConvert.DeserializeObject<Teleport>(json);
        teleport.Should().NotBeNull();

        var areaMaskManager = new AreaMaskManager([new Area("Cave", 1048576, 1)]);
        var pos = teleport.ToPosition(areaMaskManager);

        pos.X.Should().BeApproximately(-1157.58f, 0.01f);
        pos.Y.Should().BeApproximately(66.96f, 0.01f);
        pos.Z.Should().BeApproximately(17.78f, 0.01f);
        pos.Area.AreaMask.Should().Be(1048576);
        pos.Area.Name.Should().Be("Cave");
    }

    [Test]
    public void RawItemPoi_deserializes_nested_Teleport()
    {
        const string json =
            "{\"description\":\"On bar stand top\",\"x\":-1157.58,\"y\":17.78,\"teleport\":{\"x\":-1157.58,\"y\":66.96,\"z\":17.78,\"areaMask\":1048576}}";

        var poi = JsonConvert.DeserializeObject<RawItemPoi>(json);

        poi.Should().NotBeNull();
        poi.Description.Should().Be("On bar stand top");
        poi.X.Should().BeApproximately(-1157.58f, 0.01f);
        poi.Y.Should().BeApproximately(17.78f, 0.01f);

        var areaMaskManager = new AreaMaskManager([new Area("Cave", 1048576, 1)]);
        var teleportPos = poi.Teleport.ToPosition(areaMaskManager);
        teleportPos.Area.AreaMask.Should().Be(1048576);
    }

    [Test]
    public void RawPoi_deserializes_optional_Teleport()
    {
        const string json =
            "{\"title\":\"Some POI\",\"description\":\"desc\",\"x\":1,\"y\":2,\"teleport\":{\"x\":3,\"y\":4,\"z\":5,\"areaMask\":1048576}}";

        var poi = JsonConvert.DeserializeObject<RawPoi>(json);

        poi.Should().NotBeNull();
        poi.Title.Should().Be("Some POI");
        poi.Teleport.Should().NotBeNull();

        var areaMaskManager = new AreaMaskManager([new Area("Cave", 1048576, 1)]);
        var teleportPos = poi.Teleport!.ToPosition(areaMaskManager);
        teleportPos.X.Should().BeApproximately(3f, 0.01f);
        teleportPos.Y.Should().BeApproximately(4f, 0.01f);
        teleportPos.Z.Should().BeApproximately(5f, 0.01f);
        teleportPos.Area.AreaMask.Should().Be(1048576);
    }

    [Test]
    public void GameData_deserializes_from_shipped_data_json()
    {
        var jsonPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "data", "data.json");
        File.Exists(jsonPath).Should().BeTrue($"Expected test content file at '{jsonPath}'");

        var json = File.ReadAllText(jsonPath);
        var data = JsonConvert.DeserializeObject<GameData>(json);

        data.Should().NotBeNull();
        data.Items.Should().NotBeNull();
        data.AreaManager.Should().NotBeNull();
        data.ActorTypes.Should().NotBeEmpty();
        data.ScrewStructures.Should().NotBeEmpty();
        data.ElementProfiles.Should().NotBeEmpty();
    }

    [Test]
    public void RawItemPoiCollection_deserializes_from_shipped_item_pois_json()
    {
        var jsonPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "data", "item_pois.json");
        File.Exists(jsonPath).Should().BeTrue($"Expected test content file at '{jsonPath}'");

        var json = File.ReadAllText(jsonPath);
        var collection = JsonConvert.DeserializeObject<RawItemPoiCollection>(json);

        collection.Should().NotBeNull();
        collection.Items.Should().NotBeEmpty();
        collection.Items.First().Value.Pois.Should().NotBeEmpty();
        collection.Items.First().Value.Pois[0].Teleport.Should().NotBeNull();
    }

    [Test]
    public void RawPoiGroups_deserialize_from_shipped_pois_json()
    {
        var jsonPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "data", "pois.json");
        File.Exists(jsonPath).Should().BeTrue($"Expected test content file at '{jsonPath}'");

        var json = File.ReadAllText(jsonPath);
        var groups = JsonConvert.DeserializeObject<Dictionary<string, RawPoiGroup>>(json);

        groups.Should().NotBeNull();
        groups.Count.Should().BeGreaterThan(0);
    }
}