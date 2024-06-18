using Eleon.Modding;
using EmpyrionNetAPIAccess;
using EmpyrionNetAPIDefinitions;
using EmpyrionNetAPITools;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using EmpyrionNetAPITools.Extensions;
using static EmpyrionChatDiscordBridge.ChatBotBridgeConfiguration;


namespace EmpyrionChatDiscordBridge
{
    public class ChatDiscordBridge : EmpyrionModBase
    {
        enum ChatType
        {
            Global = 3,
            Faction = 5,
            Private = 1,
        }

        public ChatDiscordBridge()
        {
            EmpyrionConfiguration.ModName = "EmpyrionChatDiscordBridge";
        }

        public ModGameAPI DediAPI { get; private set; }
        public ConfigurationManager<ChatBotBridgeConfiguration> Configuration { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }
        public List<FactionInfo> Fractions { get; private set; }

        ConcurrentDictionary<int, ChatPlayerInfo> PlayerCacheById { get; } = new ConcurrentDictionary<int, ChatPlayerInfo>();
        public bool DiscordClientDisconnected { get; private set; }

        public override void Initialize(ModGameAPI dediAPI)
        {
            DediAPI = dediAPI;

            try
            {
                Log($"**EmpyrionChatDiscordBridge: loaded");

                LoadConfiguration();
                LogLevel = Configuration.Current.LogLevel;
                ChatCommandManager.CommandPrefix = Configuration.Current.ChatCommandPrefix;

                ChatCommands.Add(new ChatCommand(@"discord help", (I, A) => DisplayHelp  (I.playerId), "display help"));

                InitDiscordConnection().GetAwaiter().GetResult();

                Event_ChatMessage += (msg) => ChatDiscordBridge_Event_ChatMessage(msg).GetAwaiter().GetResult();
                API_Exit          += SavePlayerInfos;

                TaskTools.Intervall(Configuration.Current.AutosavePlayerIntervalMS, SavePlayerInfos);
            }
            catch (Exception Error)
            {
                Log($"**EmpyrionChatDiscordBridge Error: {Error} {string.Join(" ", Environment.GetCommandLineArgs())}", LogLevel.Error);
            }

        }

        private void SavePlayerInfos()
        {
            Configuration.Save();
        }

        private async Task InitDiscordConnection()
        {
            DiscordClientDisconnected = true;
            if (string.IsNullOrEmpty(Configuration.Current.DiscordBotToken)) return;

            DiscordClientDisconnected = false;

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
                LogLevel = Configuration.Current.DiscordLogLevel

            };
            DiscordClient = new DiscordSocketClient(config);

            DiscordClient.Log             += DiscordClient_Log;
            DiscordClient.Ready           += DiscordClient_Ready;
            DiscordClient.MessageReceived += DiscordClient_MessageReceived;

            DiscordClient.Disconnected += DiscordClient_Disconnected;

            await DiscordClient.LoginAsync(TokenType.Bot, Configuration.Current.DiscordBotToken);
            await DiscordClient.StartAsync();
        }

        private Task DiscordClient_Disconnected(Exception arg)
        {
            Log($"DiscordClient_Disconnected:{arg}", LogLevel.Error);

            DiscordClientDisconnected = true;

            return Task.CompletedTask;
        }

        private async Task ChatDiscordBridge_Event_ChatMessage(ChatInfo msg)
        {
            if (!string.IsNullOrEmpty(Configuration.Current.HideChatsStartsWith.FirstOrDefault(t => msg.msg.StartsWith(t, StringComparison.InvariantCultureIgnoreCase)))) return;

            Log($"**ChatDiscordBridge_Event_ChatMessage: {msg}", LogLevel.Debug);

            SocketTextChannel channel = null;

            var player = await GetPlayerInfo(msg.playerId);

            var fraction = await GetFraction(msg.recipientFactionId);

            if (msg.type == (byte)ChatType.Global) channel = await DiscordClient.GetChannelAsync(Configuration.Current.GlobalChannel?.ID ?? 0) as SocketTextChannel;
            if (msg.type == (byte)ChatType.Faction)
            {
                if (fraction.factionId != 0) channel = await DiscordClient.GetChannelAsync(Configuration.Current.FractionChannels.FirstOrDefault(C => C.FractionAbbrev == fraction.abbrev)?.ID ?? 0) as SocketTextChannel;

                if (channel == null) Log($"No fraction channel found for {fraction.abbrev}", LogLevel.Debug);
            }

            await channel?.SendMessageAsync($"{player.Name}: {msg.msg}");

            if (Configuration.Current.AdminChannel.ID != 0)
            {
                channel = await DiscordClient.GetChannelAsync(Configuration.Current.AdminChannel?.ID ?? 0) as SocketTextChannel;
                await channel?.SendMessageAsync($"{player.Name} [{fraction.abbrev}: {msg.msg}");
            }
        }

        private async Task DiscordClient_MessageReceived(SocketMessage arg)
        {
            if(string.IsNullOrEmpty(Configuration.Current.DiscordBotToken)) return;

            if(DiscordClientDisconnected) await InitDiscordConnection();

            Log($"MSG:{arg.Author.GlobalName}/{arg.Author.Username} {arg.Channel} {arg.Content}", LogLevel.Debug);

            if (!Configuration.Current.PlayerCacheByName.TryGetValue(arg.Author.GlobalName, out var player) && !Configuration.Current.PlayerCacheByName.TryGetValue(arg.Author.Username, out player)) return;

            if (arg.Channel.Name == Configuration.Current.GlobalChannel.DiscordChannelName || arg.Channel.Id == Configuration.Current.GlobalChannel.ID)
            {
                if (Configuration.Current.GlobalChannel.ID != arg.Channel.Id)
                {
                    Configuration.Current.GlobalChannel.ID = arg.Channel.Id;
                    Configuration.Save();
                }

                await Request_SendChatMessage(new Eleon.MessageData
                {
                    Channel            = Eleon.MsgChannel.Global,
                    SenderNameOverride = player.Name,
                    SenderType         = Eleon.SenderType.Player,
                    Text               = arg.Content,
                });
            }
            else if (arg.Channel.Name == Configuration.Current.AdminChannel.DiscordChannelName || arg.Channel.Id == Configuration.Current.AdminChannel.ID)
            {
                if (Configuration.Current.AdminChannel.ID != arg.Channel.Id)
                {
                    Configuration.Current.AdminChannel.ID = arg.Channel.Id;
                    Configuration.Save();
                }

                var fraction = Fractions?.FirstOrDefault(F => F.abbrev == Configuration.Current.AdminChannel.FractionAbbrev);
                if (fraction == null || !fraction.HasValue)
                {
                    Fractions = (await Request_Get_Factions(new Id(1))).factions;
                    fraction = Fractions.FirstOrDefault(F => F.abbrev == Configuration.Current.AdminChannel.FractionAbbrev);
                }

                if(fraction.Value.factionId == 0)
                {
                    Log($"No admin fraction channel found for {fraction.Value.abbrev}", LogLevel.Debug);
                    return; 
                }

                await Request_SendChatMessage(new Eleon.MessageData
                {
                    Channel            = Eleon.MsgChannel.Faction,
                    RecipientFaction   = new FactionData { Group = FactionGroup.Faction, Id = fraction.Value.factionId },
                    SenderNameOverride = player.Name,
                    SenderType         = Eleon.SenderType.ServerPrio,
                    Text               = arg.Content,
                    Arg1               = null,
                    Arg2               = null,
                });
            }
            else
            {
                var channel = Configuration.Current.FractionChannels.FirstOrDefault(C => C.DiscordChannelName == arg.Channel.Name || C.ID == arg.Channel.Id);

                if(channel == null)
                {
                    Log($"No fraction channel found for {arg.Channel}", LogLevel.Debug);
                    return;
                }

                if (channel.ID != arg.Channel.Id)
                {
                    channel.ID = arg.Channel.Id;
                    Configuration.Save();
                }

                var fraction = Fractions?.FirstOrDefault(F => F.abbrev == channel.FractionAbbrev);
                if (fraction == null || !fraction.HasValue)
                {
                    Fractions = (await Request_Get_Factions(new Id(1))).factions;
                    fraction = Fractions.FirstOrDefault(F => F.abbrev == channel.FractionAbbrev);
                }

                if (fraction.Value.factionId == 0)
                {
                    Log($"No fraction channel found for {fraction.Value.abbrev}", LogLevel.Debug);
                    return;
                }

                await Request_SendChatMessage(new Eleon.MessageData
                {
                    Channel            = Eleon.MsgChannel.Faction,
                    RecipientFaction   = new FactionData { Group = FactionGroup.Faction, Id = player.FactionId },
                    SenderNameOverride = player.Name,
                    SenderType         = Eleon.SenderType.ServerPrio,
                    Text               = arg.Content,
                    Arg1               = null,
                    Arg2               = null,
                });
            }

        }

        private async Task<FactionInfo> GetFraction(int factionId)
        {
            var fraction = Fractions?.FirstOrDefault(F => F.factionId == factionId);
            if (fraction == null || !fraction.HasValue)
            {
                Fractions = (await Request_Get_Factions(new Id(1))).factions;
                fraction = Fractions?.FirstOrDefault(F => F.factionId == factionId);
            }

            return fraction.Value;
        }

        private async Task<ChatPlayerInfo> GetPlayerInfo(int playerId)
        {
            if (!PlayerCacheById.TryGetValue(playerId, out var player))
            {
                var gamePlayer = await Request_Player_Info(playerId.ToId());
                player = new ChatPlayerInfo
                {
                    Name      = gamePlayer.playerName,
                    EntityId  = gamePlayer.entityId,
                    FactionId = gamePlayer.factionId,
                };
                PlayerCacheById                        .AddOrUpdate(playerId,    player, (k, p) => player);
                Configuration.Current.PlayerCacheByName.AddOrUpdate(player.Name, player, (k, p) => player);
            }

            return player;
        }

        private Task DiscordClient_Log(LogMessage arg)
        {
            Log($"DiscordLog:{arg}", LogLevel.Message);

            return Task.CompletedTask;
        }

        private Task DiscordClient_Ready()
        {
            Log($"{DiscordClient.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        private void LoadConfiguration()
        {
            Configuration = new ConfigurationManager<ChatBotBridgeConfiguration>
            {
                ConfigFilename = Path.Combine(EmpyrionConfiguration.SaveGameModPath, @"ChatDiscordBridge.json")
            };

            Configuration.Load();
            Configuration.Save();
        }

        private async Task DisplayHelp(int playerId)
        {
            var P = await Request_Player_Info(playerId.ToId());

            var help = new StringBuilder();

            help.AppendLine($"Discord: {(DiscordClientDisconnected ? "[c][ff0000]offline[-][/c]" : "[c][00ff00]online[-][/c]")}\n");

            if (!string.IsNullOrEmpty(Configuration.Current.GlobalChannel?.DiscordChannelName)) help.AppendLine($"Global chat: [c][00ffff]{Configuration.Current.GlobalChannel.DiscordChannelName}[-][/c] {CheckChannel(Configuration.Current.GlobalChannel.ID)}");
            var fraction = await GetFraction(P.factionId);
            if (fraction.factionId != 0) {
                var fractionChannel = Configuration.Current.FractionChannels.FirstOrDefault(C => C.FractionAbbrev == fraction.abbrev);
                if (fractionChannel != null) help.AppendLine($"Fraction chat: [c][00ffff]{fractionChannel.DiscordChannelName}[-][/c] {CheckChannel(fractionChannel.ID)}");
            }

            if(P.permission >= 3 && !string.IsNullOrEmpty(Configuration.Current.AdminChannel?.DiscordChannelName)) help.AppendLine($"Admin chat: [c][00ffff]{Configuration.Current.AdminChannel.DiscordChannelName}[-][/c] {CheckChannel(Configuration.Current.AdminChannel.ID)}");

            await DisplayHelp(playerId, help.ToString());
        }

        private string CheckChannel(ulong ID)
        {
            try
            {
                return DiscordClient.GetChannelAsync(ID).GetAwaiter().GetResult() == null ? "[c][ff0000]offline[-][/c]" : "[c][00ff00]online[-][/c]";
            }
            catch { 
                return "[c][00ff00]online[-][/c]";
            }
        }
    }
}
