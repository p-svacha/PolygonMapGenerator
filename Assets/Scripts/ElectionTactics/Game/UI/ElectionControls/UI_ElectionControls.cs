using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    /// <summary>
    /// ElectionControls are used to manipulate the animation that plays during an election.
    /// </summary>
    public class UI_ElectionControls : UIElement
    {
        private ElectionTacticsGame Game;

        [Header("Elements")]
        public TabButton Speed1Button;
        public TabButton Speed2Button;
        public TabButton Speed3Button;

        public Button ConcludeDistrictButton;
        public Button ConcludeElectionButton;

        private TabButton ActiveTabButton;

        void Start()
        {
            Speed1Button.Button.onClick.AddListener(() => DoSetSpeed(ElectionAnimationSpeed.Normal));
            Speed2Button.Button.onClick.AddListener(() => DoSetSpeed(ElectionAnimationSpeed.Fast));
            Speed3Button.Button.onClick.AddListener(() => DoSetSpeed(ElectionAnimationSpeed.VeryFast));
            ConcludeDistrictButton.onClick.AddListener(DoConcludeDistrict); 
            ConcludeElectionButton.onClick.AddListener(DoConcludeElection);
        }

        public void Init(ElectionTacticsGame game)
        {
            Game = game;
        }

        /// <summary>
        /// Sets the speed of the election animation.
        /// </summary>
        public void DoSetSpeed(ElectionAnimationSpeed speed)
        {
            if (ActiveTabButton != null) ActiveTabButton.SetSelected(false);
            if (speed == ElectionAnimationSpeed.Normal)
            {
                Speed1Button.SetSelected(true);
                ActiveTabButton = Speed1Button;
            }
            if (speed == ElectionAnimationSpeed.Fast)
            {
                Speed2Button.SetSelected(true);
                ActiveTabButton = Speed2Button;
            }
            if (speed == ElectionAnimationSpeed.VeryFast)
            {
                Speed3Button.SetSelected(true);
                ActiveTabButton = Speed3Button;
            }
            Game.ElectionAnimationHandler.SetAnimationSpeed(speed);
        }

        /// <summary>
        /// Jumps the election animation to the next end of graph animation state.
        /// </summary>
        public void DoConcludeDistrict()
        {
            Game.ElectionAnimationHandler.ConcludeDistrict();
        }

        /// <summary>
        /// Jumps the election animation to the zoomed out map at the end of the election.
        /// </summary>
        public void DoConcludeElection()
        {
            Game.ElectionAnimationHandler.ConcludeElection();
        }
    }
}
