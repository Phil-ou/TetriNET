﻿using System;
using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Server
{
    internal class ExceptionFreeTetriNETCallback : ITetriNETCallback
    {
        private readonly IPlayerManager _playerManager;
        private readonly ITetriNETCallback _callback;

        public ExceptionFreeTetriNETCallback(ITetriNETCallback callback, IPlayerManager playerManager)
        {
            _callback = callback;
            _playerManager = playerManager;
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Log.WriteLine("Exception:"+ex);
                IPlayer player = _playerManager[_callback];
                if (player != null)
                {
                    Log.WriteLine(actionName + ": " + player.Name + " has disconnected");
                    _playerManager.Remove(player);
                    // Caution: recursive call
                    foreach(Player p in _playerManager.Players)
                        p.Callback.OnPublishServerMessage(player.Name + " has disconnected");
                }
            }
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => _callback.OnServerStopped(), "OnServerStopped");
        }

        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            ExceptionFreeAction(() => _callback.OnPlayerRegistered(succeeded, playerId), "OnPlayerRegistered");
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino)
        {
            ExceptionFreeAction(() => _callback.OnGameStarted(firstTetrimino, secondTetrimino), "OnGameStarted");
        }

        public void OnGameFinished()
        {
            ExceptionFreeAction(() => _callback.OnGameFinished(), "OnGameFinished");
        }

        public void OnServerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => _callback.OnServerAddLines(lineCount), "OnServerAddLines");
        }

        public void OnPlayerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => _callback.OnPlayerAddLines(lineCount), "OnPlayerAddLines");
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            ExceptionFreeAction(() => _callback.OnPublishPlayerMessage(playerName, msg), "OnPublishPlayerMessage");
        }

        public void OnPublishServerMessage(string msg)
        {
            ExceptionFreeAction(() => _callback.OnPublishServerMessage(msg), "OnPublishServerMessage");
        }

        public void OnAttackReceived(Attacks attack)
        {
            ExceptionFreeAction(() => _callback.OnAttackReceived(attack), "OnAttackReceived");
        }

        public void OnAttackMessageReceived(string msg)
        {
            ExceptionFreeAction(() => _callback.OnAttackMessageReceived(msg), "OnAttackMessageReceived");
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            ExceptionFreeAction(() => _callback.OnNextTetrimino(index, tetrimino), "OnNextTetrimino");
        }
    }
}