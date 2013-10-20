﻿using System;
using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class DropItLikeItsHot : Achievement
    {
        public DropItLikeItsHot()
        {
            Id = 9;
            Points = 30;
            Title = "Drop it like it's hot";
            Description = "Drop a total of 10,000 Tetriminos";
            OnlyOnce = true;
        }

        public override string Progress
        {
            get { return String.Format("{0:#,0} / {1:#,0} ({2:0.0}%)", ExtraData, 10000, 100.0 * (ExtraData / 10000.0)); }
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            ExtraData++;
            if (ExtraData >= 10000)
                Achieve();
        }
    }
}