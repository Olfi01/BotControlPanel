﻿using BotControlPanel.Bots.AchBotInlineKeyboards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControlPanel.Bots
{
    #region Features to add
    /*
     ----------------------------------------------------------------------------------------------------
     + Check possible achievements with missing player achievements and number/roles of available players
     + 
     + 
     + 
     ----------------------------------------------------------------------------------------------------
     */
    #endregion
    public class WerewolfAchievementsBotPlus : FlomBot
    {
        #region Custom "Game" class
        class Game
        {
            #region Pre-declared stuff, such as variables and constants
            public Message pinmessage { get; set; }
            public Dictionary<long, string> names = new Dictionary<long, string>();
            public state gamestate { get; set; }
            private TelegramBotClient client;
            public Dictionary<long, roles> role = new Dictionary<long, roles>();
            public Dictionary<roles, string> rolestring = getRolestringDict();
            public long GroupId { get; }

            private const string joinMessageText = "<b>Join this game!</b>\n\nPin this message and remember "
                + "to press start when the roles are assigned and the game begins. <b>DON'T PRESS START BEFORE THE ROLES ARE ASSIGNED!</b>";
            private const string runMessageText = "<b>Game running!</b>\n\nPress stop <b>ONCE THE GAME STOPPED!</b>";
            private const string stoppedMessageText = "<b>This game is finished!</b>";
            private string playerlist;
            #endregion

            public Game(TelegramBotClient cl, long groupid, Message pin)
            {
                client = cl;
                GroupId = groupid;
                pinmessage = pin;
                UpdatePlayerlist();
            }

            public enum state
            {
                Joining,
                Running,
                Stopped
            }

            public enum roles
            {
                Villager,
                Werewolf,
                Drunk,
                Seer,
                Cursed,
                Harlot,
                Beholder,
                Gunner,
                Traitor,
                GuardianAngel,
                Detective,
                ApprenticeSeer,
                Cultist,
                CultistHunter,
                WildChild,
                Fool,
                Mason,
                Doppelgänger,
                Cupid,
                Hunter,
                SerialKiller,
                Tanner,
                Mayor,
                Prince,
                Sorcerer,
                ClumsyGuy,
                Blacksmith,
                AlphaWolf,
                WolfCub,
                SeerFool, // Used if not sure whether seer or fool
                Dead,
                Unknown
            }

            public static Dictionary<roles, string> getRolestringDict()
            {
                Dictionary<roles, string> dict = new Dictionary<roles, string>();
                dict.Add(roles.AlphaWolf, "Alpha Wolf 🐺⚡️");
                dict.Add(roles.ApprenticeSeer, "App Seer 🙇");
                dict.Add(roles.Beholder, "Beholder 👁");
                dict.Add(roles.Blacksmith, "Blacksmith ⚒");
                dict.Add(roles.ClumsyGuy, "Clumsy Guy 🤕");
                dict.Add(roles.Cultist, "Cultist 👤");
                dict.Add(roles.CultistHunter, "Cult Hunter 💂");
                dict.Add(roles.Cupid, "Cupid 🏹");
                dict.Add(roles.Cursed, "Cursed 😾");
                dict.Add(roles.Detective, "Detective 🕵️");
                dict.Add(roles.Doppelgänger, "Doppelgänger 🎭");
                dict.Add(roles.Drunk, "Drunk 🍻");
                dict.Add(roles.Fool, "Fool 🃏");
                dict.Add(roles.GuardianAngel, "Guardian Angel 👼");
                dict.Add(roles.Gunner, "Gunner 🔫");
                dict.Add(roles.Harlot, "Harlot 💋");
                dict.Add(roles.Hunter, "Hunter 🎯");
                dict.Add(roles.Mason, "Mason 👷");
                dict.Add(roles.Mayor, "Mayor 🎖");
                dict.Add(roles.Prince, "Prince 👑");
                dict.Add(roles.Seer, "Seer 👳");
                dict.Add(roles.SerialKiller, "Serial Killer 🔪");
                dict.Add(roles.Sorcerer, "Sorcerer 🔮");
                dict.Add(roles.Tanner, "Tanner 👺");
                dict.Add(roles.Traitor, "Traitor 🖕");
                dict.Add(roles.Villager, "Villager 👱");
                dict.Add(roles.Werewolf, "Werewolf 🐺");
                dict.Add(roles.WildChild, "Wild Child 👶");
                dict.Add(roles.WolfCub, "Wolf Cub 🐶");
                dict.Add(roles.SeerFool, "Seer OR Fool 👳🃏");

                dict.Add(roles.Dead, "DEAD 💀");
                dict.Add(roles.Unknown, "No role detected yet");
                return dict;
            }

            public bool AddPlayer(User newplayer)
            {
                if (!names.ContainsKey(newplayer.Id) && gamestate == state.Joining)
                {
                    names.Add(newplayer.Id, newplayer.FirstName.Remove(15));
                    UpdatePlayerlist();
                    return true;                    
                }
                return false;
            }

            public void Start()
            {
                gamestate = state.Running;
            }

            public void Stop()
            {
                gamestate = state.Stopped;
            }

            public bool RemovePlayer(User oldplayer)
            {
                if(names.ContainsKey(oldplayer.Id))
                {
                    names.Remove(oldplayer.Id);
                    UpdatePlayerlist();
                    return true;
                }
                return false;
            }

            public void UpdatePlayerlist()
            {
                playerlist = "<b>LYNCHORDER:</b>\n";

                foreach(var p in names.Keys)
                {
                    if(gamestate == state.Joining) playerlist += names[p] + "\n";
                    else if (gamestate == state.Running)
                    {
                        if (role.ContainsKey(p))
                        {
                            if(role[p] != roles.Dead) playerlist += "<b>" + names[p] + "</b>: " + rolestring[role[p]] + "\n";
                        }
                        else playerlist += "<b>" + names[p] + "</b>: " + rolestring[roles.Unknown] + "\n";
                    }
                }

                playerlist += "\n\nDEAD PLAYERS 💀:";

                if (gamestate == state.Running) foreach (var p in names.Keys.Where(x => role.ContainsKey(x) && role[x] == roles.Dead))
                {
                        playerlist += "\n" + names[p];
                }

                if (gamestate == state.Running)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, runMessageText
                        + "\n\n" + playerlist, parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboardStop.Get(GroupId)).Wait();
                else if (gamestate == state.Joining)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, joinMessageText
                        + "\n\n" + playerlist, parseMode: ParseMode.Html,
                        replyMarkup: InlineKeyboardStart.Get(GroupId)).Wait();
                else if (gamestate == state.Stopped)
                    client.EditMessageTextAsync(pinmessage.Chat.Id, pinmessage.MessageId, stoppedMessageText,
                        parseMode: ParseMode.Html).Wait();
            }
        }
        #endregion

        #region Variables
        private Dictionary<long, Game> games = new Dictionary<long, Game>();
        private Dictionary<string, Game.roles> roleAliases = new Dictionary<string, Game.roles>();
        List<long> justCalledStop = new List<long>();
        #endregion
        #region Constants
        public override string Name { get; } = "Werewolf Achievements Bot";
        private const string basePath = "C:\\Olfi01\\BotControlPanel\\AchievementsBot\\";
        private const string aliasesPath = basePath + "aliases.dict";
        private const string version = "2.5";
        private readonly List<long> allowedgroups = new List<long>() { -1001070844778, -1001078561643 };
        private readonly List<long> adminIds = new List<long>() { 267376056, 295152997 };
        #region Default Aliases
        private readonly List<string> defaultAliases = new List<string>()
        {
            "alphawolf",
            "apprenticeseer",
            "beholder",
            "blacksmith",
            "clumsyguy",
            "cultist",
            "cultisthunter",
            "cupid",
            "cursed",
            "detective",
            "doppelgänger",
            "drunk",
            "fool",
            "guardianangel",
            "gunner",
            "harlot",
            "hunter",
            "mason",
            "mayor",
            "prince",
            "seer",
            "seerfool",
            "serialkiller",
            "sorcerer",
            "tanner",
            "traitor",
            "villager",
            "werewolf",
            "wildchild",
            "wolfcub"
        };
        #endregion
        #endregion
        #region Constructor
        public WerewolfAchievementsBotPlus(string token) : base(token)
        {
            try
            {
                client.OnCallbackQuery += Client_OnCallbackQuery;
            }
            catch { }
        }
        #endregion

        #region Callback Query Handler
        private void Client_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            try
            {
                string data = e.CallbackQuery.Data;
                #region Callback Query Start
                if (data.StartsWith("start_"))
                {
                    long id = Convert.ToInt64(data.Substring(6));
                    if (games.ContainsKey(id))
                    {
                        if (games[id].names.Count >= 5 || id == allowedgroups[0]) // player limit disabled for test group
                        {
                            games[id].Start();
                            games[id].UpdatePlayerlist();
                            client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Game is now considered running.").Wait();
                            client.SendTextMessageAsync(id, $"<b>{e.CallbackQuery.From.FirstName}</b> has considered the game started!", parseMode: ParseMode.Html).Wait();
                        }
                        else
                        {
                            client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Too less players to start the game!").Wait();
                            client.SendTextMessageAsync(id, $"<b>{e.CallbackQuery.From.FirstName}</b> tried to start the game but there are too less players yet", parseMode: ParseMode.Html);
                        }
                    }
                    else
                    {
                        client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Did not find that game.").Wait();
                    }
                }
                #endregion
                #region Callback Query Stop
                else if (data.StartsWith("stop_"))
                {
                    long id = Convert.ToInt64(data.Substring(5));
                    if (games.ContainsKey(id))
                    {
                        if (justCalledStop.Contains(e.CallbackQuery.From.Id))
                        {
                            games[id].Stop();
                            games[id].UpdatePlayerlist();
                            games.Remove(id);
                            client.AnswerCallbackQueryAsync(e.CallbackQuery.Id,
                                "The game is now considered stopped.").Wait();
                            client.SendTextMessageAsync(id, $"<b>{e.CallbackQuery.From.FirstName}</b> has considered the game stopped!", parseMode: ParseMode.Html);
                            justCalledStop.Remove(e.CallbackQuery.From.Id);
                        }
                        else
                        {
                            justCalledStop.Add(e.CallbackQuery.From.Id);
                            client.AnswerCallbackQueryAsync(e.CallbackQuery.Id,
                                "Press this button again if the game really has stopped already.").Wait();
                            Timer t = new Timer(new TimerCallback
                                (
                                    delegate
                                    {
                                        justCalledStop.Remove(e.CallbackQuery.From.Id);
                                    }
                                ), null, 10 * 1000, Timeout.Infinite);
                        }
                    }
                    else
                    {
                        client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Did not find that game.").Wait();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                client.SendTextMessageAsync(adminIds[0], "Error in achievements callback: " + ex.Message
                    + "\n" + ex.StackTrace);
                client.SendTextMessageAsync(adminIds[1], "Error in achievements callback: " + ex.Message
                    + "\n" + ex.StackTrace);
            }
        }
        #endregion
        #region Update Handler
        protected override void Client_OnUpdate(object sender, Telegram.Bot.Args.UpdateEventArgs e)
        {
            try
            {
                if (e.Update.Type == UpdateType.MessageUpdate && e.Update.Message.Chat.Type != ChatType.Private && !allowedgroups.Contains(e.Update.Message.Chat.Id))
                {
                    client.LeaveChatAsync(e.Update.Message.Chat.Id).Wait();
                    return;
                }
                if (e.Update.Type == UpdateType.MessageUpdate)
                {
                    if (e.Update.Message.Type == MessageType.TextMessage &&
                        (e.Update.Message.Chat.Type == ChatType.Group ||
                        e.Update.Message.Chat.Type == ChatType.Supergroup))
                    {
                        var text = e.Update.Message.Text;
                        var msg = e.Update.Message;

                        #region Commands only
                        switch (text.ToLower().Replace("@werewolfbot", "").Replace('!', '/').Replace("@werewolfwolfachievementbot", ""))
                        {
                            case "/startgame":
                            case "/startchaos":
                                if (games.ContainsKey(msg.Chat.Id))
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "It seems there is already a game running in here! Stop that before you start a new one!").Wait();
                                }
                                else
                                {
                                    if (games.ContainsKey(msg.Chat.Id))
                                    {

                                        if (games[msg.Chat.Id].gamestate == Game.state.Joining && !games[msg.Chat.Id].AddPlayer(msg.From))
                                        {
                                            client.SendTextMessageAsync(msg.Chat.Id, "Failed to add <b>" + msg.From.FirstName + "</b> to the players!", parseMode: ParseMode.Html).Wait();
                                        }
                                    }
                                    else
                                    {
                                        Task<Message> t = client.SendTextMessageAsync(msg.Chat.Id, "Initializing new game...");
                                        t.Wait();
                                        var gamemessage = t.Result;
                                        var game = new Game(client, msg.Chat.Id, gamemessage);
                                        games.Add(msg.Chat.Id, game);
                                        games[msg.Chat.Id].AddPlayer(msg.From);
                                    }
                                }
                                return;

                            case "/join":
                                if (games.ContainsKey(msg.Chat.Id) && games[msg.Chat.Id].gamestate == Game.state.Joining)
                                {
                                    if (!games[msg.Chat.Id].AddPlayer(msg.From))
                                    {
                                        client.SendTextMessageAsync(msg.Chat.Id, "Failed to add <b>" + msg.From.FirstName + "</b> to the players!", parseMode: ParseMode.Html).Wait();
                                    }
                                }
                                else
                                {
                                    client.SendTextMessageAsync(msg.Chat.Id, "It seems there is no game running in your group, or it can't be joined at the moment.").Wait();
                                }
                                return;

                            case "/stopgame":
                                if (games.ContainsKey(msg.Chat.Id))
                                {
                                    if (justCalledStop.Contains(msg.From.Id))
                                    {
                                        games[msg.Chat.Id].Stop();
                                        games[msg.Chat.Id].UpdatePlayerlist();
                                        games.Remove(msg.Chat.Id);
                                        client.SendTextMessageAsync(msg.Chat.Id, $"<b>{msg.From.FirstName}</b> has considered the game stopped!", parseMode: ParseMode.Html);
                                        justCalledStop.Remove(msg.From.Id);
                                    }
                                    else
                                    {
                                        justCalledStop.Add(msg.From.Id);
                                        client.SendTextMessageAsync(msg.Chat.Id, 
                                            "Use this command again if the game really has stopped already.").Wait();
                                        Timer t = new Timer(new TimerCallback
                                            (
                                                delegate
                                                {
                                                    justCalledStop.Remove(msg.From.Id);
                                                }
                                            ), null, 10 * 1000, Timeout.Infinite);
                                    }
                                }
                                return;

                            case "/flee":
                            case "/dead":
                                if (games.ContainsKey(msg.Chat.Id))
                                {
                                    Game g = games[msg.Chat.Id];

                                    User dead = msg.ReplyToMessage != null && g.names.Keys.Contains(msg.ReplyToMessage.From.Id)
                                            ? msg.ReplyToMessage.From
                                            : (
                                                g.names.Keys.Contains(msg.From.Id)
                                                    ? msg.From
                                                    : null
                                              );
                                    if (dead == null) return;

                                    switch (g.gamestate)
                                    {
                                        case Game.state.Joining:
                                            if (!g.RemovePlayer(dead))
                                            {
                                                client.SendTextMessageAsync(msg.Chat.Id, "Failed to remove player <b>" + dead.FirstName + "</b> from the game.", parseMode: ParseMode.Html).Wait();
                                            }
                                            break;

                                        case Game.state.Running:
                                            g.role.Remove(dead.Id);
                                            g.role.Add(dead.Id, Game.roles.Dead);
                                            g.UpdatePlayerlist();
                                            break;
                                    }

                                }
                                return;
                            case "/addalias":
                                client.SendTextMessageAsync(msg.Chat.Id,
                                    "You need to write an alias behind this in the following format:\n"
                                    + "Alias Role").Wait();
                                return;
                            case "/ping":
                                client.SendTextMessageAsync(msg.Chat.Id, "PENG!").Wait();
                                return;

                            case "/version":
                                client.SendTextMessageAsync(msg.Chat.Id, $"Werewolf Achievements Manager Version {version}").Wait();
                                return;

                            case "/listalias":
                                var rolestrings = Game.getRolestringDict();
                                var listalias = "<b>ALL ALIASSES OF ALL ROLES:</b>\n";
                                foreach (var thisrole in rolestrings.Keys)
                                {
                                    listalias += "\n\n<b>" + rolestrings[thisrole] + "</b>";

                                    foreach (var alias in roleAliases.Where(x => x.Value == thisrole))
                                    {
                                        listalias += "\n" + alias.Key;
                                    }
                                }
                                client.SendTextMessageAsync(msg.Chat.Id, listalias, parseMode: ParseMode.Html).Wait();
                                return;
                        }
                        #endregion

                        #region addalias und delalias
                        if (text.StartsWith("/addalias"))
                        {
                            if (adminIds.Contains(msg.From.Id))
                            {
                                if (text.Split(' ').Count() == 3)
                                {
                                    string alias = text.Split(' ')[1].ToLower();
                                    string roleS = text.Split(' ')[2];
                                    Game.roles role = GetRoleByAlias(roleS);
                                    if (role == Game.roles.Unknown)
                                    {
                                        client.SendTextMessageAsync(msg.Chat.Id, "The role was not recognized! Adding alias failed!").Wait();
                                    }
                                    else if (!roleAliases.Keys.Contains(alias))
                                    {
                                        roleAliases.Add(alias, role);
                                        writeAliasesFile();
                                        client.SendTextMessageAsync(msg.Chat.Id, $"Alias <i>{alias}</i> successfully added for role <b>{role}</b>.", parseMode: ParseMode.Html).Wait();
                                    }
                                    else
                                    {
                                        roleAliases[alias] = role;
                                        writeAliasesFile();
                                        client.SendTextMessageAsync(msg.Chat.Id, $"Alias <i>{alias}</i> successfully updated for role <b>{role}</b>.", parseMode: ParseMode.Html).Wait();
                                    }

                                }
                            }
                            else client.SendTextMessageAsync(msg.Chat.Id, "You are not a bot admin!");

                        }

                        if (text.StartsWith("/delalias"))
                        {
                            if (adminIds.Contains(msg.From.Id))
                            {
                                if (text.Split(' ').Count() == 2)
                                {
                                    string alias = text.Split(' ')[1].ToLower();

                                    if (roleAliases.ContainsKey(alias))
                                    {
                                        roleAliases.Remove(alias);
                                        writeAliasesFile();
                                        client.SendTextMessageAsync(msg.Chat.Id, $"Alias <i>{alias}</i> was successfully removed!", parseMode: ParseMode.Html).Wait();
                                    }
                                    else
                                    {
                                        client.SendTextMessageAsync(msg.Chat.Id, $"Couldn't find Alias <i>{alias}</i>!", parseMode: ParseMode.Html).Wait();
                                    }
                                }
                                else client.SendTextMessageAsync(msg.Chat.Id, "Failed: Wrong command syntax. Syntax: /delalias <alias>").Wait();
                            }
                            else client.SendTextMessageAsync(msg.Chat.Id, "You are not a bot admin!");
                        }
                        #endregion

                        #region The heavy part: checking for each and every alias
                        if (games.ContainsKey(msg.Chat.Id))
                        {
                            if (games[msg.Chat.Id].gamestate == Game.state.Running)
                            {
                                Game g = games[msg.Chat.Id];

                                long player = 0;
                                if (msg.ReplyToMessage != null)
                                {
                                    if (g.names.Keys.Contains(msg.ReplyToMessage.From.Id)) player = msg.ReplyToMessage.From.Id;
                                }
                                else if (g.names.Keys.Contains(msg.From.Id))
                                {
                                    player = msg.From.Id;
                                }

                                if (player == 0) return;

                                List<string> Keys = roleAliases.Keys.ToList();
                                foreach (string s in defaultAliases)
                                {
                                    Keys.Add(s);
                                }

                                if (!g.role.ContainsKey(player) && Keys.Contains(text.ToLower()))
                                {
                                    var role = GetRoleByAlias(text.ToLower());
                                    if (role != Game.roles.Unknown)
                                    {
                                        g.role.Add(player, GetRoleByAlias(text.ToLower()));
                                        g.UpdatePlayerlist();
                                    }
                                }
                                else if (text.ToLower().StartsWith("now ") && Keys.Contains(text.ToLower().Substring(4)))
                                {
                                    var role = GetRoleByAlias(text.ToLower().Substring(4));
                                    if (role != Game.roles.Unknown)
                                    {
                                        var oldRole = g.role[player];
                                        if (oldRole != role)
                                        {
                                            g.role[player] = role;
                                            g.UpdatePlayerlist();
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                client.SendTextMessageAsync(adminIds[0], "Error in Achievements Bot: " +
                    ex.InnerException + "\n" + ex.Message + "\n" + ex.StackTrace).Wait();
                client.SendTextMessageAsync(adminIds[1], "Error in Achievements Bot: " +
                    ex.InnerException + "\n" + ex.Message + "\n" + ex.StackTrace).Wait();
            }
        }
        #endregion

        #region Get Role By Alias
        private Game.roles GetRoleByAlias(string alias)
        {
            if (roleAliases.ContainsKey(alias)) return roleAliases[alias];
            else
            {
                switch (alias)
                {
                    case "alphawolf":
                        return Game.roles.AlphaWolf;

                    case "apprenticeseer":
                        return Game.roles.ApprenticeSeer;

                    case "beholder":
                        return Game.roles.Beholder;

                    case "blacksmith":
                        return Game.roles.Blacksmith;

                    case "clumsyguy":
                        return Game.roles.ClumsyGuy;

                    case "cultist":
                        return Game.roles.Cultist;

                    case "cultisthunter":
                        return Game.roles.CultistHunter;

                    case "cupid":
                        return Game.roles.Cupid;

                    case "cursed":
                        return Game.roles.Cursed;

                    case "detective":
                        return Game.roles.Detective;

                    case "doppelgänger":
                        return Game.roles.Doppelgänger;

                    case "drunk":
                        return Game.roles.Drunk;

                    case "fool":
                        return Game.roles.Fool;

                    case "guardianangel":
                        return Game.roles.GuardianAngel;

                    case "gunner":
                        return Game.roles.Gunner;

                    case "harlot":
                        return Game.roles.Harlot;

                    case "hunter":
                        return Game.roles.Hunter;

                    case "mason":
                        return Game.roles.Mason;

                    case "mayor":
                        return Game.roles.Mayor;

                    case "prince":
                        return Game.roles.Prince;

                    case "seer":
                        return Game.roles.Seer;

                    case "seerfool":
                        return Game.roles.SeerFool;

                    case "serialkiller":
                        return Game.roles.SerialKiller;

                    case "sorcerer":
                        return Game.roles.Sorcerer;

                    case "tanner":
                        return Game.roles.Tanner;

                    case "traitor":
                        return Game.roles.Traitor;

                    case "villager":
                        return Game.roles.Villager;

                    case "werewolf":
                        return Game.roles.Werewolf;

                    case "wildchild":
                        return Game.roles.WildChild;

                    case "wolfcub":
                        return Game.roles.WolfCub;

                    default:
                        return Game.roles.Unknown;
                }
            }
        }
        #endregion

        #region File Methods
        private void writeAliasesFile()
        {
            if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
            System.IO.File.WriteAllText(aliasesPath, JsonConvert.SerializeObject(roleAliases));
        }

        private void getAliasesFromFile()
        {
            if (System.IO.File.Exists(aliasesPath))
            {
                roleAliases = JsonConvert.DeserializeObject<Dictionary<string, Game.roles>>(
                    System.IO.File.ReadAllText(aliasesPath));
            }
            else
            {
                if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
                System.IO.File.Create(aliasesPath);
            }
            if (roleAliases == null) roleAliases = new Dictionary<string, Game.roles>();
        }
        #endregion

        #region Control Methods
        #region Start Bot
        public override bool StartBot()
        {
            getAliasesFromFile();
            return base.StartBot();
        }
        #endregion
        #endregion
    }
}
