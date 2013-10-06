﻿using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Strategy
{
    public class SpecialAdvices
    {
        public enum SpecialAdviceActions
        {
            Discard,
            UseSelf,
            UseOpponent,
            Wait
        }

        public SpecialAdviceActions SpecialAdviceAction { get; set; }
        public int OpponentId { get; set; }
    }

    public interface ISpecialStrategy
    {
        bool GetSpecialAdvice(IBoard board, IPiece current, IPiece next, List<Specials> inventory, int inventoryMaxSize, List<IOpponent> opponents, out List<SpecialAdvices> advices);
    }
}