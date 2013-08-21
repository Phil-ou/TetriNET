﻿using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TetriNET.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            //string baseAddress = "net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            //SimpleTetriNETProxyManager proxyManager = new SimpleTetriNETProxyManager(baseAddress);
            ExceptionFreeProxyManager proxyManager = new ExceptionFreeProxyManager(baseAddress);

            GameClient client = new GameClient(proxyManager);
            client.PlayerName = "Joel_" + Guid.NewGuid().ToString().Substring(0, 6);

            System.Console.WriteLine("Press any key to stop client");

            while (true)
            {
                client.Test();
                if (System.Console.KeyAvailable)
                    break;
                System.Threading.Thread.Sleep(250);
            }

            System.Console.ReadLine();
        }
    }
}
