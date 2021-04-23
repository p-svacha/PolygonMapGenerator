using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParriskGame
{
    public class Player
    {
        public string Name;
        public Color Color;

        public List<Territory> Territories = new List<Territory>();

        public Player(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }
}
