using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ElectionTactics
{
    [Serializable]
    public class NetworkConnectionData
    {
        public ulong ClientId; 
        public string Name;

        public NetworkConnectionData(string name, ulong clientId = 0)
        {
            Name = name;
            ClientId = clientId;
        }

        public override string ToString()
        {
            return "Name: " + Name + ", ClientId: " + ClientId;
        }
    }
}
