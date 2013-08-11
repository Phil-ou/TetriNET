﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using TetriNET.Common;
using TetriNET.Common.WCF;

namespace TetriNET.Server
{
    public class ExceptionFreeCallbackManager : ITetriNETCallbackManager
    {
        // TODO: weak reference
        private Dictionary<ITetriNETCallback, ExceptionFreeTetriNETCallback>  _callbacks = new Dictionary<ITetriNETCallback, ExceptionFreeTetriNETCallback>();
        private readonly IPlayerManager _playerManager;

        public ExceptionFreeCallbackManager(IPlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public ITetriNETCallback Callback
        {
            get
            {
                ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
                ExceptionFreeTetriNETCallback exceptionFreeCallback;
                bool found = _callbacks.TryGetValue(callback, out exceptionFreeCallback);
                if (!found)
                {
                    exceptionFreeCallback = new ExceptionFreeTetriNETCallback(callback, _playerManager);
                    _callbacks.Add(callback, exceptionFreeCallback);
                }
                return exceptionFreeCallback;
            }
        }
    }

    public class BasicCallbackManager : ITetriNETCallbackManager
    {
        public ITetriNETCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
            }
        }
    }

    //[ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Single)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    internal class GameServer : ITetriNET
    {
        public enum States
        {
            WaitingStartServer, // -> StartingServer
            StartingServer, // -> WaitingStartGame
            WaitingStartGame, // -> StartingGame
            StartingGame, // -> GameStarted
            GameStarted, // -> GameFinished
            GameFinished, // -> WaitingStartGame
            StoppingServer, // -> WaitingStartServer
        }

        private readonly Common.ThreadSafeSingleton<TetriminoQueue> _tetriminoQueue = new Common.ThreadSafeSingleton<TetriminoQueue>(() => new TetriminoQueue());
        private ServiceHost Host { get; set; }
        private ITetriNETCallbackManager CallbackManager { get; set; }
        private IPlayerManager PlayerManager { get; set; }
        public States State { get; private set; }

        public GameServer(ITetriNETCallbackManager callbackManager, IPlayerManager playerManager)
        {
            Log.WriteLine("***GameServer:ctor***");

            State = States.WaitingStartServer;

            CallbackManager = callbackManager;
            PlayerManager = playerManager;
            AttackId = 0;

            Task.Factory.StartNew(TaskResolveActions);
        }

        public void StartService()
        {
            Log.WriteLine("StartService");
            if (State != States.WaitingStartServer)
                return; // TODO: error
            
            State = States.StartingServer;

            string port = ConfigurationManager.AppSettings["port"];
            Uri baseAddress;
            if (String.IsNullOrEmpty(port) || port.ToLower() == "auto")
                baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;
            else
                baseAddress =  new Uri("net.tcp://localhost:"+port+"/TetriNET");

            //Host = new ServiceHost(typeof(GameServer), baseAddress);
            Host = new ServiceHost(this, baseAddress);
            Host.AddServiceEndpoint(typeof(ITetriNET), new NetTcpBinding(SecurityMode.None), "");
            //Host.Description.Behaviors.Add(new IPFilterServiceBehavior("DenyLocal"));
            Host.Open();

            foreach (var endpt in Host.Description.Endpoints)
            {
                Log.WriteLine("Enpoint address:\t{0}", endpt.Address);
                Log.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                Log.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
            }
            
            State = States.WaitingStartGame;
        }

        public void StopService()
        {
            Log.WriteLine("StopService");

            State = States.StoppingServer;

            // Inform players
            foreach (IPlayer p in PlayerManager.Players)
                p.Callback.OnServerStopped();

            // Close service host
            Host.Close();

            State = States.WaitingStartServer;
        }

        public void StartGame()
        {
            Log.WriteLine("Start game");

            State = States.GameStarted;

            // Reset Tetrimino Queue
            _tetriminoQueue.Instance.Reset(); // TODO: random seed
            Tetriminos firstTetrimino = _tetriminoQueue.Instance[0];
            Tetriminos secondTetrimino = _tetriminoQueue.Instance[1];
            // Send start game to every connected player
            foreach (IPlayer p in PlayerManager.Players)
            {
                p.TetriminoIndex = 0;
                p.Callback.OnGameStarted(firstTetrimino, secondTetrimino);
            }
        }

        public void StopGame()
        {
            Log.WriteLine("Stop game");

            State = States.GameFinished;

            // Send start game to every connected player
            foreach (IPlayer p in PlayerManager.Players)
                p.Callback.OnGameFinished();

            State = States.WaitingStartGame;
        }

        #region ITetriNET

        public void RegisterPlayer(string playerName)
        {
            Log.WriteLine("RegisterPlayer:" + playerName);

            ITetriNETCallback callback = CallbackManager.Callback;

            // Try to add new player
            IPlayer player = PlayerManager.Add(playerName, callback);
            if (player != null)
            {
                player.Callback.OnPlayerRegistered(true, player.Id);
                // Check if OnPlayerRegistered has not failed
                if (PlayerManager[callback] != null)
                {
                    // Inform players
                    foreach (IPlayer p in PlayerManager.Players)
                        p.Callback.OnPublishServerMessage(playerName + " is now connected");
                    //
                    Log.WriteLine("New player:[" + player.Id + "] " + playerName);
                }
            }
            else
            {
                Log.WriteLine("Register failed for player " + playerName);
                callback.OnPlayerRegistered(false, -1);
            }
        }

        public void PublishMessage(string msg)
        {
            ITetriNETCallback callback = CallbackManager.Callback;
            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                Log.WriteLine("PublishMessage:[" + player.Name + "]:" + msg);

                foreach (IPlayer p in PlayerManager.Players)
                    p.Callback.OnPublishPlayerMessage(player.Name, msg);
            }
            else
            {
                RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                Log.WriteLine("PublishMessage from unknown player[" + (clientEndpoint == null ? callback.ToString() : clientEndpoint.Address) + "]");
            }
        }

        public void PlaceTetrimino(Tetriminos tetrimino, Orientations orientation, Position position)
        {
            ITetriNETCallback callback = CallbackManager.Callback;
            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                Log.WriteLine("PlaceTetrimino:[" + player.Name + "]" + tetrimino + " " + orientation + " at " + position.X + "," + position.Y);

                _actionQueue.Enqueue(() => PlaceTetrimino(player, tetrimino, orientation, position));
            }
            else
            {
                RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                Log.WriteLine("PlaceTetrimino from unknown player[" + (clientEndpoint == null ? callback.ToString() : clientEndpoint.Address) + "]");
            }
        }

        public void SendAttack(int targetId, Attacks attack)
        {
            ITetriNETCallback callback = CallbackManager.Callback;
            IPlayer player = PlayerManager[callback];
            if (player != null)
            {
                IPlayer target = PlayerManager[targetId];
                if (target != null)
                {
                    Log.WriteLine("SendAttack:[" + player.Name + "] -> [" + targetId + "]:" + attack);

                    _actionQueue.Enqueue(() => Attack(player, target, attack));
                }
                else
                    Log.WriteLine("SendAttack to unknown player[" + targetId + "] from [" + player.Name + "]");
            }
            else
            {
                RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                Log.WriteLine("SendAttack from unknown player[" + (clientEndpoint == null ? callback.ToString() : clientEndpoint.Address) + "]");
            }
        }

        #endregion

        #region Attack Id

        public int AttackId
        {
            get;
            private set;
        }

        #endregion

        #region Tetrimino queue
        private class TetriminoQueue
        {
            private int _tetriminosCount;
            private readonly object _lock = new object();
            private int _size;
            private int[] _array;
            private Random _random;

            public void Reset(int seed = 0)
            {
                _tetriminosCount = Enum.GetValues(typeof (Tetriminos)).Length;
                _random = new Random(seed);
                Grow(1);
            }

            public Tetriminos this[int index]
            {
                get
                {
                    Tetriminos tetrimino;
                    lock (_lock)
                    {
                        if (index >= _size)
                            Grow(128);
                        tetrimino = (Tetriminos)_array[index];
                    }
                    return tetrimino;
                }
            }

            private void Grow(int increment)
            {
                int newSize = _size + increment;
                int[] newArray = new int[newSize];
                if (_size > 0)
                    Array.Copy(_array, newArray, _size);
                for (int i = _size; i < newSize; i++)
                    newArray[i] = _random.Next(_tetriminosCount);
                _array = newArray;
                _size = newSize;
            }
        }
        #endregion

        #region Action queue

        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        private void TaskResolveActions()
        {
            while (true)
            {
                if (!_actionQueue.IsEmpty)
                {
                    Action action;
                    bool dequeue = _actionQueue.TryDequeue(out action);
                    if (dequeue)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine("Exception raised in TaskResolveActions. Exception:"+ex.ToString());
                        }
                    }
                }
                Thread.Sleep(0);
                // TODO: break
            }
        }

        #endregion

        #region Actions

        private void Attack(IPlayer player, IPlayer target, Attacks attack)
        {
            Log.WriteLine("SendAttack[" + player.Name + "][" + target.Name + "]" + attack);

            // Store attack id locally
            int attackId = AttackId;
            // Increment attack
            AttackId++;
            // Send attack to target
            target.Callback.OnAttackReceived(attack);
            // Send attack message to players
            string attackString = attackId + ": " + attack + " from " + player.Name + " to " + target.Name;
            foreach (Player p in PlayerManager.Players)
                p.Callback.OnAttackMessageReceived(attackString);
        }

        private void PlaceTetrimino(IPlayer player, Tetriminos tetrimino, Orientations orientation, Position position)
        {
            Log.WriteLine("PlaceTetrimino[" + player.Name + "]" + tetrimino + " " + orientation + " at " + position.X + "," + position.Y);
            // Get next piece
            player.TetriminoIndex++;
            Tetriminos nextTetrimino = _tetriminoQueue.Instance[player.TetriminoIndex];
            // Send next piece
            player.Callback.OnNextTetrimino(player.TetriminoIndex, nextTetrimino);
        }

        #endregion
    }
}