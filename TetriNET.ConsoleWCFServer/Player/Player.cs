﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFServer.Player
{
    public sealed class Player : IPlayer
    {
        public Player(string name, ITetriNETCallback callback)
        {
            Name = name;
            Callback = callback;
            PieceIndex = 0;
            LastActionToClient = DateTime.Now;
            LastActionFromClient = DateTime.Now;
            TimeoutCount = 0;
            State = PlayerStates.Registered;
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                LastActionToClient = DateTime.Now;
            }
            catch (CommunicationObjectAbortedException)
            {
                if (OnConnectionLost != null)
                    OnConnectionLost(this);
            }
            catch (Exception ex)
            {
                Logger.Log.WriteLine(Logger.Log.LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                if (OnConnectionLost != null)
                    OnConnectionLost(this);
            }
        }

        #region IPlayer

        public event PlayerConnectionLostHandler OnConnectionLost;

        public string Name { get; private set; }
        public int PieceIndex { get; set; }
        public byte[] Grid { get; set; }
        //
        public ITetriNETCallback Callback { get; private set; }
        //
        public PlayerStates State { get; set; }
        public DateTime LossTime { get; set; }

        // Heartbeat management
        public DateTime LastActionToClient { get; private set; } // used to check if heartbeat is needed
        // Timeout management
        public DateTime LastActionFromClient { get; private set; }
        public int TimeoutCount { get; private set; }

        public void ResetTimeout()
        {
            TimeoutCount = 0;
            LastActionFromClient = DateTime.Now;
        }

        public void SetTimeout()
        {
            TimeoutCount++;
            LastActionFromClient = DateTime.Now;
        }

        #endregion

        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            ExceptionFreeAction(() => Callback.OnHeartbeatReceived(), "OnHeartbeatReceived");
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => Callback.OnServerStopped(), "OnServerStopped");
        }

        public void OnPlayerRegistered(RegistrationResults result, int playerId, bool gameStarted)
        {
            ExceptionFreeAction(() => Callback.OnPlayerRegistered(result, playerId, gameStarted), "OnPlayerRegistered");
        }

        public void OnPlayerJoined(int playerId, string name)
        {
            ExceptionFreeAction(() => Callback.OnPlayerJoined(playerId, name), "OnPlayerJoined");
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLeft(playerId, name, reason), "OnPlayerLeft");
        }

        public void OnPlayerLost(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLost(playerId), "OnPlayerLost");
        }

        public void OnPlayerWon(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerWon(playerId), "OnPlayerWon");
        }

        public void OnGameStarted(Pieces firstPiece, Pieces secondPiece, Pieces thirdPiece, GameOptions options)
        {
            ExceptionFreeAction(() => Callback.OnGameStarted(firstPiece, secondPiece, thirdPiece, options), "OnGameStarted");
        }

        public void OnGameFinished()
        {
            ExceptionFreeAction(() => Callback.OnGameFinished(), "OnGameFinished");
        }

        public void OnGamePaused()
        {
            ExceptionFreeAction(() => Callback.OnGamePaused(), "OnGamePaused");
        }

        public void OnGameResumed()
        {
            ExceptionFreeAction(() => Callback.OnGameResumed(), "OnGameResumed");
        }

        public void OnServerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnServerAddLines(lineCount), "OnServerAddLines");
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnPlayerAddLines(specialId, playerId, lineCount), "OnPlayerAddLines");
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishPlayerMessage(playerName, msg), "OnPublishPlayerMessage");
        }

        public void OnPublishServerMessage(string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishServerMessage(msg), "OnPublishServerMessage");
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            ExceptionFreeAction(() => Callback.OnSpecialUsed(specialId, playerId, targetId, special), "OnSpecialUsed");
        }

        public void OnNextPiece(int index, Pieces piece)
        {
            ExceptionFreeAction(() => Callback.OnNextPiece(index, piece), "OnNextPiece");
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            ExceptionFreeAction(() => Callback.OnGridModified(playerId, grid), "OnGridModified");
        }

        public void OnServerMasterChanged(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnServerMasterChanged(playerId), "OnServerMasterChanged");
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            ExceptionFreeAction(() => Callback.OnWinListModified(winList), "OnWinListModified");
        }

        #endregion
    }
}
