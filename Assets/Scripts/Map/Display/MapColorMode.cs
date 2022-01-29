using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapColorMode
{
    Basic = 0,  // basic differentiation between water and land
    White = 1,  // everything full white, useful when working with textures
    Biomes = 2, // shows biomes in different colors
    Continents = 3, // shows each continent in a different color
    ParriskBoard = 4, // used in parrisk game
}
