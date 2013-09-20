﻿using TetriNET.Common.DataContracts;

namespace TetriNET.Common.Contracts
{
    public interface ITetriNET
    {
        // Player connexion/deconnexion management
        void RegisterPlayer(ITetriNETCallback callback, string playerName);
        void UnregisterPlayer(ITetriNETCallback callback);
        void Heartbeat(ITetriNETCallback callback);

        // Chat
        void PublishMessage(ITetriNETCallback callback, string msg); // Partyline Chat Message

        // In-game
        void PlacePiece(ITetriNETCallback callback, int index, Pieces piece, int orientation, int posX, int posY, byte[] grid);
        void ModifyGrid(ITetriNETCallback callback, byte[] grid);
        void UseSpecial(ITetriNETCallback callback, int targetId, Specials special);
        void SendLines(ITetriNETCallback callback, int count);
        void GameLost(ITetriNETCallback callback);

        // Server management
        void StartGame(ITetriNETCallback callback);
        void StopGame(ITetriNETCallback callback);
        void PauseGame(ITetriNETCallback callback);
        void ResumeGame(ITetriNETCallback callback);
        void ChangeOptions(ITetriNETCallback callback, GameOptions options);
        void KickPlayer(ITetriNETCallback callback, int playerId);
        void BanPlayer(ITetriNETCallback callback, int playerId);
        void ResetWinList(ITetriNETCallback callback);
    }
}
