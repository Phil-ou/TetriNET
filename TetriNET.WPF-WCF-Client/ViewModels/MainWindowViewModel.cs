﻿using System;
using System.Configuration;
using System.Linq;
using TetriNET.Client.DefaultBoardAndTetriminos;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.Logger;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Properties;
using TetriNET.WPF_WCF_Client.ViewModels.Connection;
using TetriNET.WPF_WCF_Client.ViewModels.Options;
using TetriNET.WPF_WCF_Client.ViewModels.PartyLine;
using TetriNET.WPF_WCF_Client.ViewModels.PlayField;
using TetriNET.WPF_WCF_Client.ViewModels.Statistics;
using TetriNET.WPF_WCF_Client.ViewModels.WinList;

namespace TetriNET.WPF_WCF_Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public WinListViewModel WinListViewModel { get; set; }
        public ClientStatisticsViewModel ClientStatisticsViewModel { get; set; }
        public OptionsViewModel OptionsViewModel { get; set; }
        public PartyLineViewModel PartyLineViewModel { get; set; }
        public ConnectionViewModel ConnectionViewModel { get; set; }
        public PlayFieldViewModel PlayFieldViewModel { get; set; }

        private int _activeTabItemIndex;
        public int ActiveTabItemIndex {
            get
            {
                return _activeTabItemIndex;
            }
            set
            {
                if (_activeTabItemIndex != value)
                {
                    _activeTabItemIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            //
            ExecuteOnUIThread.Initialize();

            // Create sub view models
            WinListViewModel = new WinListViewModel();
            ClientStatisticsViewModel = new ClientStatisticsViewModel();
            OptionsViewModel = new OptionsViewModel();
            PartyLineViewModel = new PartyLineViewModel();
            ConnectionViewModel = new ConnectionViewModel();
            PlayFieldViewModel = new PlayFieldViewModel();

            // Initialize Log
            string logFilename = "WPF_" + Guid.NewGuid().ToString().Substring(0, 5) + ".log";
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], logFilename);

            // Log user settings path
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            Log.WriteLine(Log.LogLevels.Info, "Local user config path: {0}", config.FilePath);

            // Create client
            Client = new Client.Client(Tetrimino.CreateTetrimino, () => new Board(12, 22));

            // Get saved or default options
            Models.Options.OptionsSingleton.Instance.ServerOptions = Settings.Default.GameOptions ?? Client.Options;
            // TODO: fix this bug  ---- Workaround: remove duplicate key
            Models.Options.OptionsSingleton.Instance.ServerOptions.SpecialOccurancies = Models.Options.OptionsSingleton.Instance.ServerOptions.SpecialOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();
            Models.Options.OptionsSingleton.Instance.ServerOptions.TetriminoOccurancies = Models.Options.OptionsSingleton.Instance.ServerOptions.TetriminoOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();
            Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPartyLineOnRegistered = Settings.Default.AutomaticallySwitchToPartyLineOnRegistered;
            Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted = Settings.Default.AutomaticallySwitchToPlayFieldOnGameStarted;

            // Get textures
            Textures.Textures.TexturesSingleton.Instance.ReadFromFile(ConfigurationManager.AppSettings["texture"]);
        }

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregisted;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameOver -= OnGameOver;
            oldClient.OnConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerUnregistered += OnPlayerUnregisted;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameOver += OnGameOver;
            newClient.OnConnectionLost += OnConnectionLost;
        }

        public override void OnClientAssigned(IClient newClient)
        {
            // TODO: use reflection to set Client on each ViewModel
            WinListViewModel.Client = newClient;
            ClientStatisticsViewModel.Client = newClient;
            OptionsViewModel.Client = newClient;
            PartyLineViewModel.Client = newClient;
            ConnectionViewModel.Client = newClient;
            PlayFieldViewModel.Client = newClient;
        }
        #endregion

        #region IClient events handler
        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded && Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPartyLineOnRegistered)
                if (ActiveTabItemIndex == 0) // Connect
                    ActiveTabItemIndex = 3; // Party line
            if (succeeded && Client.IsServerMaster)
                Client.ChangeOptions(Models.Options.OptionsSingleton.Instance.ServerOptions);
        }

        private void OnPlayerUnregisted()
        {
            ActiveTabItemIndex = 0; // Connect
        }

        private void OnGameStarted()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
                ActiveTabItemIndex = 4; // Play fields
        }

        private void OnGameFinished()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                if (ActiveTabItemIndex == 4) // Play fields
                    ActiveTabItemIndex = 3; // Party line
            }
        }

        private void OnGameOver()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                if (ActiveTabItemIndex == 4) // Play fields
                    ActiveTabItemIndex = 3; // Party line
            }
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            ActiveTabItemIndex = 0; // Connect
        }
        #endregion
    }
}
