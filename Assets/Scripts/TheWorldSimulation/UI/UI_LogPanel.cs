using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LogPanel : MonoBehaviour
{
    public Text MainText;

    public void SetText(string text)
    {
        MainText.text = text;
    }
}
