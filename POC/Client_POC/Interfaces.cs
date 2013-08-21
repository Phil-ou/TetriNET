﻿using TetriNET.Common.Contracts;

namespace POC.Client_POC
{
    public delegate void ConnectionLostHandler();

    public interface IProxy : ITetriNET
    {
        event ConnectionLostHandler OnConnectionLost;
    }
}
