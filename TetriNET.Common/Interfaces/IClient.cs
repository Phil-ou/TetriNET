﻿using System.Collections.Generic;
using TetriNET.Common.GameDatas;

namespace TetriNET.Common.Interfaces
{
    public delegate void ClientPlayerRegisteredHandler(bool succeeded, int playerId);

    public delegate void ClientRoundStartedHandler();
    public delegate void ClientRoundFinishedHandler();
    public delegate void ClientStartGameHandler();
    public delegate void ClientFinishGameHandler();
    public delegate void ClientPauseGameHandler();
    public delegate void ClientResumeGameHandler();
    public delegate void ClientGameOverHandler();
    
    public delegate void ClientRedrawHandler();
    public delegate void ClientRedrawBoardHandler(int playerId, IBoard board);
    public delegate void ClientTetriminoMovingHandler();
    public delegate void ClientTetriminoMovedHandler();
    public delegate void ClientWinListModifiedHandler(List<WinEntry> winList);
    public delegate void ClientServerMasterModifiedHandler(bool isServerMaster);
    public delegate void ClientPlayerLostHandler(int playerId, string playerName);
    public delegate void ClientPlayerWonHandler(int playerId, string playerName);
    public delegate void ClientPlayerJoinedHandler(int playerId, string playerName);
    public delegate void ClientPlayerLeftHandler(int playerId, string playerName);
    public delegate void ClientPlayerPublishMessageHandler(string playerName, string msg);
    public delegate void ClientServerPublishMessageHandler(string msg);
    public delegate void ClientInventoryChangedHandler();
    public delegate void ClientLinesClearedChangedHandler();
    public delegate void ClientLevelChangedHandler();
    public delegate void ClientSpecialUsedHandler(string playerName, string targetName, int specialId, Specials special);
    public delegate void ClientPlayerAddLines(string playerName, int specialId, int count);

    public interface IClient
    {
        string Name { get; }
        int PlayerId { get; }
        int MaxPlayersCount { get; }
        ITetrimino CurrentTetrimino { get; }
        ITetrimino NextTetrimino { get; }
        IBoard Board { get; }
        List<Specials> Inventory { get; }
        int LinesCleared { get; }
        int Level { get; }
        bool IsGamePaused { get; }
        bool IsGameStarted { get; }
        int InventorySize { get; }

        IBoard GetBoard(int playerId);
        bool IsPlaying(int playerId);

        event ClientRoundStartedHandler OnRoundStarted;
        event ClientRoundFinishedHandler OnRoundFinished;
        event ClientStartGameHandler OnGameStarted;
        event ClientFinishGameHandler OnGameFinished;
        event ClientPauseGameHandler OnGamePaused;
        event ClientResumeGameHandler OnGameResumed;
        event ClientGameOverHandler OnGameOver;

        // UI
        event ClientRedrawHandler OnRedraw;
        event ClientRedrawBoardHandler OnRedrawBoard;
        event ClientTetriminoMovingHandler OnTetriminoMoving;
        event ClientTetriminoMovedHandler OnTetriminoMoved;
        event ClientPlayerRegisteredHandler OnPlayerRegistered;
        event ClientWinListModifiedHandler OnWinListModified;
        event ClientServerMasterModifiedHandler OnServerMasterModified;
        event ClientPlayerLostHandler OnPlayerLost;
        event ClientPlayerWonHandler OnPlayerWon;
        event ClientPlayerJoinedHandler OnPlayerJoined;
        event ClientPlayerLeftHandler OnPlayerLeft;
        event ClientPlayerPublishMessageHandler OnPlayerPublishMessage;
        event ClientServerPublishMessageHandler OnServerPublishMessage;
        event ClientInventoryChangedHandler OnInventoryChanged;
        event ClientLinesClearedChangedHandler OnLinesClearedChanged;
        event ClientLevelChangedHandler OnLevelChanged;
        event ClientSpecialUsedHandler OnSpecialUsed;
        event ClientPlayerAddLines OnPlayerAddLines;

        // Client->Server command
        void Register(string name);
        void StartGame();
        void StopGame();
        void PauseGame();
        void ResumeGame();
        void ResetWinList();
        void ChangeOptions(GameOptions options);
        void KickPlayer(int playerId);
        void BanPlayer(int playerId);

        // Game controller
        void Drop();
        void MoveDown();
        void MoveLeft();
        void MoveRight();
        void RotateClockwise();
        void RotateCounterClockwise();
        void DiscardFirstSpecial();
        bool UseSpecial(int targetId);

        //
        void Dump();
    }
}