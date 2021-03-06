﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TetriNET.Client.Achievements;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Models;

namespace TetriNET.WPF_WCF_Client.ViewModels.PartyLine
{
    public abstract class ChatEntry
    {
        public ChatColor Color { get; set; }
    }

    public class PlayerMessageEntry : ChatEntry
    {
        public string Message { get; set; }
        public string PlayerName { get; set; }
    }

    public class ServerMessageEntry : ChatEntry
    {
        public string Message { get; set; }
    }

    public abstract class AchievementEntry : ChatEntry
    {
        public IClient Client { get; set; }
        public IAchievement Achievement { get; set; }

        public bool IsEarned
        {
            get
            {
                if (Achievement != null && Client != null && Client.Achievements != null)
                    return Client.Achievements.Any(x => x.Id == Achievement.Id && x.IsAchieved);
                return false;
            }
        }

        public DateTime FirstTimeAchieved
        {
            get
            {
                if (Achievement != null && Client != null && Client.Achievements != null)
                {
                    IAchievement achievement = Client.Achievements.FirstOrDefault(x => x.Id == Achievement.Id && x.IsAchieved);
                    if (achievement != null)
                        return achievement.FirstTimeAchieved;
                }
                return DateTime.MinValue;
            }
        }
    }

    public class SelfAchievementEntry : AchievementEntry
    {
    }

    public class OtherAchievementEntry : AchievementEntry
    {
        public string PlayerName { get; set; }
    }

    public class InvalidAchievementEntry : ChatEntry
    {
        public string PlayerName { get; set; }
        public string AchievementTitle { get; set; }
    }

    public class ChatViewModel : ViewModelBase
    {
        private const int MaxEntries = 500;

        private readonly ObservableCollection<ChatEntry> _chatEntries = new ObservableCollection<ChatEntry>();
        public ObservableCollection<ChatEntry> ChatEntries
        {
            get { return _chatEntries; }
        }

        private string _inputChat;
        public string InputChat
        {
            get { return _inputChat; }
            set
            {
                if (value != null)
                {
                    if (Client != null)
                        Client.PublishMessage(value);
                    _inputChat = ""; // delete msg
                    OnPropertyChanged();
                }
            }
        }

        private bool _isRegistered;
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set
            {
                if (_isRegistered != value)
                {
                    _isRegistered = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isInputFocused;
        public bool IsInputFocused
        {
            get { return _isInputFocused; }
            set
            {
                _isInputFocused = value;
                OnPropertyChanged();
            }
        }

        public ChatViewModel()
        {
            AchievementManager manager = new AchievementManager();
            manager.FindAllAchievements(Assembly.Load("TetriNET.Client.Achievements"));
            IAchievement sniper = manager.Achievements.FirstOrDefault(x => x.Title == "Sniper");
            IAchievement fearMyBrain = manager.Achievements.FirstOrDefault(x => x.Title == "Fear my brain !");

            AddEntry(new PlayerMessageEntry
            {
                PlayerName = "Dummy1",
                Color = ChatColor.Green,
                Message = "Message with player name visible",
            });
            AddEntry(new ServerMessageEntry
            {
                Color = ChatColor.Red,
                Message = "Message without player name",
            });
            AddEntry(new SelfAchievementEntry
            {
                Color = ChatColor.Blue,
                Achievement = sniper,
            });
            AddEntry(new OtherAchievementEntry
            {
                Color = ChatColor.Blue,
                Achievement = fearMyBrain,
                PlayerName = "Dummy2",
            });
            AddEntry(new InvalidAchievementEntry
            {
                Color = ChatColor.Blue,
                AchievementTitle = "Too good for you",
                PlayerName = "Dummy3",
            });
        }

        protected void AddEntry(ChatEntry entry)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                ChatEntries.Add(entry);
                if (ChatEntries.Count > MaxEntries)
                    ChatEntries.RemoveAt(0);
            });
        }

        protected void AddServerMessage(string msg, ChatColor color)
        {
            AddEntry(new ServerMessageEntry
            {
                Message = msg,
                Color = color,
            });
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerAchievementEarned -= OnPlayerAchievementEarned;
            oldClient.OnAchievementEarned -= OnAchievementEarned;
            oldClient.OnPlayerPublishMessage -= OnPlayerPublishMessage;
            oldClient.OnServerPublishMessage -= OnServerPublishMessage;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameOver -= OnGameOver;
            oldClient.OnGamePaused -= OnGamePaused;
            oldClient.OnGameResumed -= OnGameResumed;
            oldClient.OnPlayerLost -= OnPlayerLost;
            oldClient.OnPlayerWon -= OnPlayerWon;
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnPlayerJoined -= OnPlayerJoined;
            oldClient.OnPlayerLeft -= OnPlayerLeft;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerAchievementEarned += OnPlayerAchievementEarned;
            newClient.OnAchievementEarned += OnAchievementEarned;
            newClient.OnPlayerPublishMessage += OnPlayerPublishMessage;
            newClient.OnServerPublishMessage += OnServerPublishMessage;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameOver += OnGameOver;
            newClient.OnGamePaused += OnGamePaused;
            newClient.OnGameResumed += OnGameResumed;
            newClient.OnPlayerLost += OnPlayerLost;
            newClient.OnPlayerWon += OnPlayerWon;
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerJoined += OnPlayerJoined;
            newClient.OnPlayerLeft += OnPlayerLeft;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerUnregistered()
        {
            AddServerMessage("*** You've unregistered successfully", ChatColor.Green);
            IsRegistered = true;
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            string msg;
            if (reason == ConnectionLostReasons.ServerNotFound)
                msg = "*** Server not found";
            else
                msg = "*** Connection lost";
            AddServerMessage(msg, ChatColor.Red);
            IsRegistered = false;
        }

        private void OnPlayerLeft(int playerid, string playerName, LeaveReasons reason)
        {
            string msg;
            switch (reason)
            {
                case LeaveReasons.Spam:
                    msg = String.Format("*** {0} has been BANNED [SPAM]", playerName);
                    break;
                case LeaveReasons.Disconnected:
                    msg = String.Format("*** {0} has disconnected", playerName);
                    break;
                case LeaveReasons.ConnectionLost:
                    msg = String.Format("*** {0} has left [connection lost]", playerName);
                    break;
                case LeaveReasons.Timeout:
                    msg = String.Format("*** {0} has left [timeout]", playerName);
                    break;
                case LeaveReasons.Ban:
                    msg = String.Format("*** {0} has been BANNED", playerName);
                    break;
                case LeaveReasons.Kick:
                    msg = String.Format("*** {0} has been kicked", playerName);
                    break;
                default:
                    msg = String.Format("*** {0} has left {1}", playerName, reason);
                    break;
            }
            AddServerMessage(msg, ChatColor.Green);
        }

        private void OnPlayerJoined(int playerid, string playerName)
        {
            AddServerMessage(String.Format("*** {0} has joined", playerName), ChatColor.Green);
        }

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
            {
                AddServerMessage("*** You've registered successfully", ChatColor.Green);
                IsRegistered = true;
            }
            else
                AddServerMessage("*** You've FAILED registering !!!", ChatColor.Red);
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            AddServerMessage(String.Format("*** {0} has WON", playerName), ChatColor.Orange);
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            AddServerMessage(String.Format("*** {0} has LOST", playerName), ChatColor.Orange);
        }

        private void OnGameResumed()
        {
            AddServerMessage("*** The game has been Resumed", ChatColor.Yellow);
        }

        private void OnGamePaused()
        {
            AddServerMessage("*** The game has been Paused", ChatColor.Yellow);
        }

        private void OnGameOver()
        {
            AddServerMessage("*** You have LOST", ChatColor.Orange);
        }

        private void OnGameFinished()
        {
            AddServerMessage("*** The Game has Ended", ChatColor.Red);
        }

        private void OnGameStarted()
        {
            AddServerMessage("*** The Game has Started", ChatColor.Red);
        }

        private void OnPlayerPublishMessage(string playerName, string msg)
        {
            AddEntry(new PlayerMessageEntry
                {
                    Color = ChatColor.Black,
                    PlayerName = playerName,
                    Message = msg
                });
        }

        private void OnServerPublishMessage(string msg)
        {
            AddServerMessage(msg, ChatColor.Blue);
        }

        private void OnAchievementEarned(IAchievement achievement, bool firstTime)
        {
            if (firstTime)
                AddEntry(new SelfAchievementEntry
                    {
                        Color = ChatColor.Blue,
                        Client = Client,
                        Achievement = achievement
                    });
        }

        private void OnPlayerAchievementEarned(int playerId, string playerName, int achievementId, string achievementTitle)
        {
            IAchievement achievement = Client.Achievements == null ? null : Client.Achievements.FirstOrDefault(x => x.Id == achievementId);

            if (achievement == null)
                AddEntry(new InvalidAchievementEntry
                    {
                        Color = ChatColor.Blue,
                        PlayerName = playerName,
                        AchievementTitle = achievementTitle
                    });
            else
                AddEntry(new OtherAchievementEntry
                    {
                        Color = ChatColor.Blue, 
                        Client = Client,
                        PlayerName = playerName, 
                        Achievement = achievement
                    });
        }

        #endregion
    }

    public class ChatViewModelDesignData : ChatViewModel
    {
        public ChatViewModelDesignData()
        {
            //AchievementManager manager = new AchievementManager();
            //manager.FindAllAchievements(Assembly.Load("TetriNET.Client.Achievements"));
            //IAchievement sniper = manager.Achievements.FirstOrDefault(x => x.Title == "Sniper");
            //IAchievement fearMyBrain = manager.Achievements.FirstOrDefault(x => x.Title == "Fear my brain !");

            AddEntry(new PlayerMessageEntry
                {
                    PlayerName = "Dummy1",
                    Color = ChatColor.Green,
                    Message = "Message with player name visible",
                });
            AddEntry(new ServerMessageEntry
                {
                    Color = ChatColor.Red,
                    Message = "Message without player name",
                });
            //AddEntry(new SelfAchievementEntry
            //    {
            //        Color = ChatColor.Blue,
            //        Achievement = sniper,
            //    });
            //AddEntry(new OtherAchievementEntry
            //    {
            //        Color = ChatColor.Blue,
            //        Achievement = fearMyBrain,
            //        PlayerName = "Dummy2",
            //    });
            AddEntry(new InvalidAchievementEntry
                {
                    Color = ChatColor.Blue,
                    AchievementTitle = "Too good for you",
                    PlayerName = "Dummy3",
                });
        }
    }
}