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
        public string Name;

        public NetworkConnectionData(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return "Name: " + Name;
        }
    }
}
