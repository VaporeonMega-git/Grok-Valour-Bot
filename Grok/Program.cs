using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using Grok;

Env.Load();

var token = Environment.GetEnvironmentVariable("BOT_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("TOKEN environment variable not set.");
    return;
}

var client = new ValourClient("https://api.valour.gg/");
client.SetupHttpClient();

var loginResult = await client.InitializeUser(token);
if (!loginResult.Success)
{
    Console.WriteLine($"Login Failed: {loginResult.Message}");
    return;
}

// await client.PlanetService.JoinPlanetAsync(42061742971289601, "");
// await client.PlanetService.JoinPlanetAsync(12215159187308544);
// await client.PlanetService.LeavePlanetAsync(await client.PlanetService.FetchPlanetAsync(12215159187308544));

await client.BotService.JoinAllChannelsAsync();

var channelCache = new Dictionary<long, Channel>();

foreach (var planet in client.PlanetService.JoinedPlanets)
{
    foreach (var channel in planet.Channels)
    {
        channelCache[channel.Id] = channel;
        Console.WriteLine($"Cached: {channel.Id}");
    }
}

Console.WriteLine($"Logged in as {client.Me.Name} (ID: {client.Me.Id})");

var bannedUserIDs = new List<long> {};

client.MessageService.MessageReceived += async (message) =>
{
    string content = message.Content ?? "";
    long channelId = message.ChannelId;
    var member = await message.FetchAuthorMemberAsync();
    var planetId = message.PlanetId;

    if (content is null) {
        return;
    }

    if (message.AuthorUserId == client.Me.Id) {
        return;
    }

    if (bannedUserIDs.Contains(message.AuthorUserId)) {
        return;
    }
    
    if (planetId != null) {
        var planet = await client.PlanetService.FetchPlanetAsync(planetId.Value);
        var selfMember = await client.PlanetService.FetchMemberByUserAsync(client.Me.Id, planet.Id);

        if (Utils.StartsWithAny(content, "«@m-" + selfMember.Id.ToString() + "» github", "«@m-" + selfMember.Id.ToString() + "»  github"))
        {
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» You can see my source code here: https://github.com/VaporeonMega-git/Grok-Valour-Bot");
            return;
        };

        if (Utils.StartsWithAny(content, "«@m-" + selfMember.Id.ToString() + "»"))
        {
            var responses = new Dictionary<string, int>
            {
                { "yeah", 10000 },
                { "nah", 10000 },
                { "probably", 10000 },
                { "outlook not good", 10000 },
                { "maybe", 10000 },
                { "i doubt it", 10000 },
                { "for sure", 10000 },
                { "if so i'd kms", 10 },
                { "if not i'd kms", 10 },
                { "there's probably like a 67% chance", 67},
                { "oh my god do you ever shut up", 10 },
                { "i literally could not care less", 500 },
                { "ohhh elon~ I- Oh, didn't see you there, user. Pretend you saw nothing.", 1},
                { "who cares", 500 }
            };

            var reply = $"«@m-{member.Id}» " + Utils.RandomString(responses);
            await Utils.SendReplyAsync(channelCache, channelId, reply);
        }
    }
};

Console.WriteLine("Listening for messages...");
await Task.Delay(Timeout.Infinite);