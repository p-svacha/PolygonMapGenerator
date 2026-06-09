using UnityEngine;

namespace ElectionTactics
{
    /// <summary>
    /// An event represents everything notable that happens in the game and that changes the gamestate in a significant way without (or with indirect) player input.
    /// <br/>Every event has a corresponding news article, both for flavour purposes and with additional information.
    /// </summary>
    public class Event
    {
        public int Year { get; private set; }
        public int Cycle { get; private set; }

        public Party AssociatedParty { get; private set; }
        public District AssociatedDistrict { get; private set; }
    }
}
