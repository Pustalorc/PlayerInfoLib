# PlayerInfoLib

Player Info Library for Unturned using OpenMod. This aims to replace the old RocketMod 4 version that this repository is forked on.


## Current Features

`/investigate <player>`

Allows to investigate and get information from a specific player.


## Database Stored Data

The database currently stores the following values:

- Steam 64 ID (Id)
- Steam Name (SteamName)
- In-Game Display Name (CharacterName)
- Steam Profile Picture Hash (ProfilePictureHash) [You can grab a hash and get the profile picture by putting it in the following URL: https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/00/{HASH}.jpg]
- Last In-Game Group ID (LastQuestGroupId)
- Last Selected Steam Group (SteamGroup)
- Last Selected Steam Group Name (SteamGroupName)
- Hardware ID (Hwid)
- IP Address (Ip)
- Total playtime (TotalPlaytime)
- Last login to any network servers (LastLoginGlobal)
- Last joined server ID (ServerId)

Note: You need a Steam Web API Key in order to get any data within the ProfilePictureHash column. See configuration for how to get it.

For server data, it only stores the following values:
- Server entry ID (Id)
- Local Instance Name (Instance)
- Server Public Name (Name)

##Installation

- `openmod install Pustalorc.PlayerInfoLib`
- Configure the plugin.
