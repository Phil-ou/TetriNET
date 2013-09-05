﻿using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.Controls
{
    /// <summary>
    /// Interaction logic for OpponentGrid.xaml
    /// </summary>
    public partial class OpponentGrid : UserControl
    {
        // TODO: dynamically get width/height
        private const int ColumnsCount = 12;
        private const int RowsCount = 22;

        private readonly object _lock = new object();

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);
        private static readonly SolidColorBrush SpecialColor = new SolidColorBrush(Colors.LightGray);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("OpponentClientProperty", typeof(IClient), typeof(OpponentGrid), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private int _playerId;
        public int PlayerId {
            get { return _playerId; }
            set
            {
                _playerId = value;
                TxtOpponentId.Text = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        private string _playerName;
        public string PlayerName {
            get { return _playerName; }
            set
            {
                _playerName = value;
                TxtOpponentName.Text = value;
            }
        }

        public OpponentGrid()
        {
            InitializeComponent();

            PlayerId = -1;
            PlayerName = "Not playing"; // default value

            // Add Grid row definitions
            for (int i = 0; i < RowsCount; i++)
                Grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(12)
                });
            // Add Grid column definitions
            for (int i = 0; i < ColumnsCount; i++)
                Grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(12)
                });

            // Populate the grid with cells
            for (int i = 0; i < Grid.RowDefinitions.Count(); i++)
                for (int j = 0; j < Grid.ColumnDefinitions.Count(); j++)
                {
                    //  Create a new textblock as "cell" and add it to the grid
                    TextBlock txt = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Colors.Black),
                        Background = new SolidColorBrush(Colors.Transparent),
                        TextAlignment = TextAlignment.Center,
                        FontSize = 9,
                        Margin = new Thickness(1, 1, 1, 1)
                    };
                    Grid.Children.Add(txt);
                    Grid.SetRow(txt, i);
                    Grid.SetColumn(txt, j);
                }
        }

        private void DrawGrid(IBoard board)
        {
            if (board == null)
                return;
            lock (_lock)
            {
                for (int y = 1; y <= board.Height; y++)
                    for (int x = 1; x <= board.Width; x++)
                    {
                        int cellY = board.Height - y;
                        int cellX = x - 1;
                        byte cellValue = board[x, y];

                        TextBlock uiPart = GetControl<TextBlock>(cellX, cellY);
                        if (cellValue == CellHelper.EmptyCell)
                        {
                            uiPart.Text = "";
                            uiPart.Background = TransparentColor;
                        }
                        else
                        {
                            Specials special = CellHelper.GetSpecial(cellValue);
                            Tetriminos color = CellHelper.GetColor(cellValue);

                            if (special == Specials.Invalid)
                            {
                                uiPart.Text = "";
                                uiPart.Background = Mapper.MapTetriminoToColor(color);
                            }
                            else
                            {
                                uiPart.Text = Mapper.MapSpecialToChar(special).ToString(CultureInfo.InvariantCulture);
                                uiPart.Background = SpecialColor;
                            }
                        }
                    }
            }
        }

        private void ClearGrid()
        {
            foreach (TextBlock uiPart in Grid.Children.Cast<TextBlock>())
            {
                uiPart.Background = TransparentColor;
                uiPart.Text = "";
            }
        }

        private T GetControl<T>(int cellX, int cellY) where T : FrameworkElement
        {
            return Grid.Children.Cast<T>().Single(e => Grid.GetRow(e) == cellY && Grid.GetColumn(e) == cellX);
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            OpponentGrid _this = sender as OpponentGrid;

            if (_this != null)
            {
                IClient client = args.NewValue as IClient;
                if (client != null)
                {
                    _this.Client = client;
                    // Register the Client UI events
                    _this.Client.OnGameStarted += _this.OnGameStarted;
                    _this.Client.OnRedrawBoard += _this.OnRedrawBoard;
                }
            }
        }

        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                Visibility = Visibility.Visible;
                ClearGrid();
            });
        }

        private void OnRedrawBoard(int playerId, IBoard board)
        {
            if (playerId == PlayerId)
                ExecuteOnUIThread.Invoke(() => DrawGrid(board));
        }
    }
}