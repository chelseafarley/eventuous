using static Eventuous.Tests.Redis.Fixtures.IntegrationFixture;
using static Eventuous.Tests.Redis.Store.Helpers;

namespace Eventuous.Tests.Redis.Store;

[Collection("Sequential")]
public class ReadEvents {
    [Fact]
    public async Task ShouldReadOne() {
        var evt        = CreateEvent();
        var streamName = GetStreamName();
        await AppendEvent(streamName, evt, ExpectedStreamVersion.NoStream);

        var result = await Instance.EventReader.ReadEvents(streamName, StreamReadPosition.Start, 100, default);

        result.Length.Should().Be(1);
        result[0].Payload.Should().BeEquivalentTo(evt);
    }

    [Fact]
    public async Task ShouldReadMany() {
        // ReSharper disable once CoVariantArrayConversion
        var events     = CreateEvents(20).ToArray();
        var streamName = GetStreamName();
        await AppendEvents(streamName, events, ExpectedStreamVersion.NoStream);

        var result = await Instance.EventReader.ReadEvents(streamName, StreamReadPosition.Start, 100, default);

        var actual = result.Select(x => x.Payload);
        actual.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task ShouldReadTail() {
        // ReSharper disable once CoVariantArrayConversion
        var streamName = GetStreamName();

        var events1  = CreateEvents(10).ToArray();
        var appended = await AppendEvents(streamName, events1, ExpectedStreamVersion.NoStream);
        var position = appended.GlobalPosition;

        var events2 = CreateEvents(10).ToArray();
        await AppendEvents(streamName, events2, ExpectedStreamVersion.Any);

        var result = await Instance.EventReader.ReadEvents(streamName, new StreamReadPosition((long)position), 100, default);

        var actual = result.Select(x => x.Payload);
        actual.Should().BeEquivalentTo(events2);
    }

    [Fact]
    public async Task ShouldReadHead() {
        // ReSharper disable once CoVariantArrayConversion
        var events     = CreateEvents(20).ToArray();
        var streamName = GetStreamName();
        await AppendEvents(streamName, events, ExpectedStreamVersion.NoStream);

        var result = await Instance.EventReader.ReadEvents(streamName, StreamReadPosition.Start, 10, default);

        var expected = events.Take(10);
        var actual   = result.Select(x => x.Payload);
        actual.Should().BeEquivalentTo(expected);
    }
}
