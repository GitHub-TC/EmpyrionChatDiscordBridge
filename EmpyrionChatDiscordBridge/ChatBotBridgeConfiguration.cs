using EmpyrionNetAPIDefinitions;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;
using Discord;
using System.Collections.Concurrent;

namespace EmpyrionChatDiscordBridge
{
    public class ChatBotBridgeConfiguration
    {
        public class ChatPlayerInfo
        {
            public string Name { get; set; }
            public int EntityId { get; set; }
            public int FactionId { get; set; }
        }
        public class ChatBotChannel {
            public string DiscordChannelName { get; set; }
            public ulong ID { get; set; }
            public string FractionAbbrev { get; set; }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel LogLevel { get; set; } = LogLevel.Message;
        [JsonConverter(typeof(StringEnumConverter))]
        public LogSeverity DiscordLogLevel { get; set; } = LogSeverity.Info;
        public string ChatCommandPrefix { get; set; } = "\\/";
        public int AutosavePlayerIntervalMS { get; set; } = 30000;
        public string DiscordBotToken { get; set; }

        public string[] HideChatsStartsWith { get; set; } = new string[] { "/", "\\", "cb:", "am:" };

        public ChatBotChannel AdminChannel { get; set; } = new ChatBotChannel();
        public ChatBotChannel GlobalChannel { get; set; } = new ChatBotChannel();
        public List<ChatBotChannel> FractionChannels { get; set; } = new List<ChatBotChannel>();

        public ConcurrentDictionary<string, ChatPlayerInfo> PlayerCacheByName { get; set; } = new ConcurrentDictionary<string, ChatPlayerInfo>();
    }
}