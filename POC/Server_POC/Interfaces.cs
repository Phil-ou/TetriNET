﻿using System;
using System.Collections.Generic;
using TetriNET.Common;

namespace POC.Server_POC
{
    public delegate void PlayerDisconnectedHandler(IPlayer player);

    public interface IPlayer : ITetriNETCallback
    {
        event PlayerDisconnectedHandler OnDisconnected;

        string Name { get; }
        int TetriminoIndex { get; set; }
        DateTime LastAction { get; set; }
        ITetriNETCallback Callback { get; } // Should never be used by anything else then IPlayerManager and IPlayer
    }

    public interface IPlayerManager
    {
        int Add(IPlayer player);
        bool Remove(IPlayer player);

        int MaxPlayers { get; }
        int PlayerCount { get; }

        IEnumerable<IPlayer> Players { get; }

        int GetId(IPlayer player);

        IPlayer this[string name] { get; }
        IPlayer this[int index] { get; }
        IPlayer this[ITetriNETCallback callback] { get; } // Callback property from IPlayer should only be used here
    }

    public delegate void RegisterPlayerHandler(IPlayer player, int id);
    public delegate void UnregisterPlayerHandler(IPlayer player);
    public delegate void PublishMessageHandler(IPlayer player, string msg);
    public delegate void PlaceTetriminoHandler(IPlayer player, Tetriminos tetrimino, Orientations orientation, Position position);
    public delegate void SendAttackHandler(IPlayer player, IPlayer target, Attacks attack);

    public interface IHost : IWCFTetriNET
    {
        event RegisterPlayerHandler OnPlayerRegistered;
        event UnregisterPlayerHandler OnPlayerUnregistered;
        event PublishMessageHandler OnMessagePublished;
        event PlaceTetriminoHandler OnTetriminoPlaced;
        event SendAttackHandler OnAttackSent;

        IPlayerManager PlayerManager { get; }

        void Start(string port);
        void Stop();
    }
}
