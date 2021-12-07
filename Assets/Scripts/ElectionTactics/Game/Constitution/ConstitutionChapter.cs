using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public abstract class ConstitutionChapter
    {
        protected ElectionTacticsGame Game;
        protected string ChapterTitle;

        public ConstitutionChapter(ElectionTacticsGame game)
        {
            Game = game;
            SetInitialRandomValues();
        }

        protected abstract void SetInitialRandomValues();
        public abstract string GetConstitutionText();

        protected int GetRandomConditionValue(int min, int max, int step)
        {
            int range = (max - min) / step;
            int val = Random.Range(0, range + 1);
            return min + val * step;
        }
        protected bool GetRandomBool()
        {
            return Random.value >= 0.5f;
        }
    }
}
