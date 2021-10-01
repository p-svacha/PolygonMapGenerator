using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public Text Title;
    public Text Text;

    public float Width;
    public float Height;
    private const int MouseOffset = 5;
    private const int ScreenEdgeOffset = 20;

    public int InitializeOffset; // We need to wait 2 frames before width and height can be read correctly

    public void Initialize(string title, string text)
    {
        Title.text = title;
        Text.text = text;
        InitializeOffset = 0;
    }

    private void Update()
    {
        if (InitializeOffset < 2) InitializeOffset++;
        else if (InitializeOffset == 2)
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            Vector3 position = Input.mousePosition + new Vector3(MouseOffset, -MouseOffset, 0);
            Width = GetComponent<RectTransform>().rect.width;
            Height = GetComponent<RectTransform>().rect.height;
            if (position.x + Width > Screen.width) position.x = Screen.width - Width - ScreenEdgeOffset;
            if (position.y - Height < 0) position.y = Height + ScreenEdgeOffset;
            transform.position = position;
            InitializeOffset++;
        }
    }
}
