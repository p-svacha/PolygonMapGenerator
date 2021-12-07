using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_Constitution : MonoBehaviour
    {
        public Text ConstitutionText;

        public void Init(Constitution c)
        {
            ConstitutionText.text = c.GetConstitutionText();
        }
    }
}
