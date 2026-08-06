// See https://aka.ms/new-console-template for more information

using System.Text.Json.Serialization;
using System.Threading.Channels;
using Dropper.Models;
using Dropper.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, new SourceGenerationContext());
});

builder.Services.AddScoped<Channel<Event>>(_ => Channel.CreateBounded<Event>(
    new BoundedChannelOptions(10)
    {
        FullMode = BoundedChannelFullMode.Wait,
        AllowSynchronousContinuations = false,
        SingleReader = false,
        SingleWriter = false
    }));

builder.Services.AddSingleton<EventBroadcaster<Event>>();

var app = builder.Build();

app.MapPost("/events",
    async ([FromBody] Event @event, [FromServices] EventBroadcaster<Event> broadcaster) =>
    {
        await broadcaster.PublishAsync(@event);
        return Results.Ok(@event);
    });

app.MapGet("/events",
    (EventBroadcaster<Event> broadcaster, Channel<Event> channel, CancellationToken ctx) =>
    {
        broadcaster.Subscribe(channel.Writer, ctx);
        return Results.ServerSentEvents(channel.Reader.ReadAllAsync(ctx), eventType: "events");
    });

app.UseDefaultFiles();
app.UseStaticFiles();

await app.RunAsync();

[JsonSerializable(typeof(Event))]
[JsonSerializable(typeof(Event[]))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}