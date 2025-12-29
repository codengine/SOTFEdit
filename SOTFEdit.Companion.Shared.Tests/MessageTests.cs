using FluentAssertions;
using NUnit.Framework;
using SOTFEdit.Companion.Shared.Messages;
using Tynamix.ObjectFiller;

namespace SOTFEdit.Companion.Shared.Tests;

[TestFixture]
public class MessageTests
{
    [SetUp]
    public void SetUp()
    {
        MessagePackInitializer.Initialize();
    }

    [Test]
    public void TestCompanionAddPoiMessage()
    {
        var message = new Filler<CompanionAddPoiMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestDumpScenesMessage()
    {
        var message = new Filler<CompanionDumpScenesMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().NotBeNull();
    }

    [Test]
    public void TestCompanionPoiListMessage()
    {
        var message = new Filler<CompanionPoiListMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionPoiMessage()
    {
        var message = new Filler<CompanionPoiMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionPoiMessage_Roundtrip_WithExplicitValues()
    {
        var message = new CompanionPoiMessage(
            "Test",
            "Desc",
            1.5f,
            -2.25f,
            3f,
            12345,
            "path.jpg");

        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionPoiListMessage_Roundtrip_WithExplicitValues()
    {
        var message = new CompanionPoiListMessage(
            PoiGroupType.Actors,
            [
                new CompanionPoiMessage("A", null, 1f, 2f, 3f, 4, null),
                new CompanionPoiMessage("B", "b", -1f, -2f, -3f, 5, "s.png")
            ]);

        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionPosCollectionMessage()
    {
        var message = new Filler<CompanionPosCollectionMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionPosMessage()
    {
        var message = new Filler<CompanionPosMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionRequestPoiUpdateMessage()
    {
        var message = new Filler<CompanionRequestPoiUpdateMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionTeleportMessage()
    {
        var message = new Filler<CompanionTeleportMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionNetworkPlayerUpdateMessage()
    {
        var message = new Filler<CompanionNetworkPlayerUpdateMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }

    [Test]
    public void TestCompanionChangeUpdateFrequencyMessage()
    {
        var message = new Filler<CompanionSettingsMessage>().Create();
        var copy = CompanionMessageTestUtils.Copy(message);
        copy.Should().BeEquivalentTo(message);
    }
}