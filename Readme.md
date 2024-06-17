# Empyrion Chat Discord Bridge

## Installation
Sie können diesen Mod direkt mit dem MOD-Manager von EWA (Empyrion Web Access) laden. <br/>
Ohne den EWA funktioniert der Mod nur innerhalb des EmpyrionModHost

## DiscordBot

Eine gute Beschreibung zum Erstellen eines DiscordBots findet man hier https://www.writebots.com/discord-bot-token/

Das Erstellen geschieht über die Developers Seite von Discord https://discordapp.com/developers/applications/

Der Bot benötigt NUR folgende Rechte

![](Screenshots\BotPermissions.png)

!! WICHTIG !! Damit der Bot auch den Inhalt der Chatmeldungen lesen darf muss noch folgende Einstellung "MESSAGE CONTENT INTENT" erlaubt werden
![](Screenshots\BotMessageContent.png)

Auch sollte der Bot in den Servereinstellungen auf die Kanäle beschränkt werden welche er benötigt. Hier z.B. 

![](Screenshots\DiscordChannelsAccess.png)
für die Kanäle welche in der Konfiguration der Mod im Savegame hinterlegt sind

```
{
  "LogLevel": "Message",
  "DiscordLogLevel": "Error",
  "ChatCommandPrefix": "\\/",
  "AutosavePlayerIntervalMS": 30000,
  "DiscordBotToken": "..........Your....Secret... Bot.... Token.........",
  "HideChatsStartsWith": [
    "/",
    "\\",
    "cb:",
    "am:"
  ],
  "AdminChannel": {
    "DiscordChannelName": "egs-admin-chat",
    "ID": 0,
    "FractionAbbrev": "AST"
  },
  "GlobalChannel": {
    "DiscordChannelName": "egs-server",
    "ID": 0,
    "FractionAbbrev": null
  },
  "FractionChannels": [
    {
      "DiscordChannelName": "egs-ast-fraction",
      "ID": 0,
      "FractionAbbrev": "AST"
    }
  ]
}
```

Um den globalen Chat einzurichten muss nun einfach über den Discord Kanal (hier "egs-server") ein Chat abgesetzt werden. Die Mod trägt dann automatisch die "ID" des
Kanals ein welcher für die Chats, welche aus dem Spiel gesendet werden, benötigt wird. 

Für die Admin und Fraktionschats gilt:

Man muss InGame ein Chat über den globalen Kanal machen damit die Mod den Spieler kennt
![](Screenshots\InitChatMod.png)
und danach über den jeweiligen Discord Kanal einen Chat absetzten damit die Mod die benötigte ID ermitteln kann.
![](Screenshots\InitBackChatDiscord.png)
![](Screenshots\InitBackChatMod.png)

Nun ist die Discord Bridge vollständig eingerichtet und kann verwendet werden.

## Besondere Kanäle
- **GlobalChannel**\
  Chats von und zu dem globalen Kanal in Empyrion
- **AdminChannel**\
  Chat zu einer Adminfraktion in Empyrion. In dem Discord Chat werden alle Globalen- und Fraktionschats ebenfalls eingetragen\
  Zu diesen Kanal sollten nur die Admins zugang haben
- **FractionChannels**\
  Chat von und zu dem fraktions Kanal der jeweiligen Fraktion. Diese ist jeweils mit ihrem Kürzel in "FractionAbbrev" anzugeben\
  Zu diesen Kanal sollten nur die Mitglieder der Fraktion zugang haben

## InGame Empyrion
Über den Chat in Empyrion kann der Status der Kanäle und ob sie eingerichtet sind, ermittelt werden 
- **\discord help**\
  ![](Screenshots\Help.png)

## Konfiguration
- **LogLevel:** Debug,**Message**,Error
- **DiscordLogLevel:** Critical,Error,Warning,**Info**,Verbose,Debug
- **ChatCommandPrefix** Zeichen mit denen das Chatkommando der Mod beginnen muss
- **AutosavePlayerIntervalMS** Das Zeitintervall in dem sich die Mod die bekannten Spieler in der Konfiguration speichert um sie offline wieder zu erkennen
- **DiscordBotToken** Das Token des Bots
- **HideChatsStartsWith** Empyrion Chats welche mit einem dieser Texte beginnen werden nicht nach Discord übertragen

***

English-Version:

---

# Empyrion Chat Discord Bridge

## Installation
You can load this mod directly with the MOD Manager from EWA (Empyrion Web Access).
Without the EWA, the mod only works within the EmpyrionModHost

## DiscordBot

A good description of how to create a DiscordBot can be found here https://www.writebots.com/discord-bot-token/

It is created via the developers page of Discord https://discordapp.com/developers/applications/

The bot ONLY needs the following rights

![](Screenshots\BotPermissions.png)

!! IMPORTANT !! So that the bot can also read the content of the chat messages, the following setting "MESSAGE CONTENT INTENT" must be allowed
![](Screenshots\BotMessageContent.png)

The bot should also be restricted to the channels it needs in the server settings. Here, for example

![](Screenshots\DiscordChannelsAccess.png)
for the channels that are stored in the mod configuration in the savegame

```
{
  "LogLevel": "Message",
  "DiscordLogLevel": "Error",
  "ChatCommandPrefix": "\\/",
  "AutosavePlayerIntervalMS": 30000,
  "DiscordBotToken": "..........Your....Secret... Bot.... Token.........",
  "HideChatsStartsWith": [
    "/",
    "\\",
    "cb:",
    "am:"
  ],
  "AdminChannel": {
    "DiscordChannelName": "egs-admin-chat",
    "ID": 0,
    "FractionAbbrev": "AST"
  },
  "GlobalChannel": {
    "DiscordChannelName": "egs-server",
    "ID": 0,
    "FractionAbbrev": null
  },
  "FractionChannels": [
    {
      "DiscordChannelName": "egs-ast-fraction",
      "ID": 0,
      "FractionAbbrev": "AST"
    }
  ]
}
```

To set up the global chat, you simply need to send a chat via the Discord channel (here "egs-server"). The mod then automatically enters the "ID" of the channel, which is required for the chats that are sent from the game.

The following applies to the admin and faction chats:

You need to send a chat in-game via the global channel so that the mod knows the player
![](Screenshots\InitChatMod.png)
and then send a chat via the respective Discord channel so that the mod can determine the required ID.
![](Screenshots\InitBackChatDiscord.png)
![](Screenshots\InitBackChatMod.png)

The Discord Bridge is now fully set up and can be used.

## Special channels
- **GlobalChannel**\
Chats from and to the global channel in Empyrion
- **AdminChannel**\
Chat to an admin faction in Empyrion. All global and faction chats are also entered in the Discord chat. Only the admins should have access to this channel. - **FractionChannels** Chat from and to the faction channel of the respective faction. This must be specified with its abbreviation in "FractionAbbrev"\
Only members of the faction should have access to this channel

## InGame Empyrion
The status of the channels and whether they are set up can be determined via the chat in Empyrion

- **\discord help**\
![](Screenshots\Help.png)

## Configuration
- **LogLevel:** Debug,**Message**,Error
- **DiscordLogLevel:** Critical,Error,Warning,**Info**,Verbose,Debug
- **ChatCommandPrefix** Characters with which the mod's chat command must begin
- **AutosavePlayerIntervalMS** The time interval in which the mod saves the known players in the configuration in order to recognize them offline
- **DiscordBotToken** The bot's token
- **HideChatsStartsWith** Empyrion chats that begin with one of these texts are not transferred to Discord