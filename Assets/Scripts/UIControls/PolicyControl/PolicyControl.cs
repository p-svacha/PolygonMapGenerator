using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PolicyControl : MonoBehaviour
{
    private int MaxValue = 8;

    public Text Label;
    public GameObject ValueContainer;
    public Button MinusButton;
    public Button PlusButton;

    public int Value;

    // Start is called before the first frame update
    void Start()
    {
        PlusButton.onClick.AddListener(IncreaseValue);
        MinusButton.onClick.AddListener(DecreaseValue);
        SetValue(0);
    }

    public void Init(string label, int value)
    {
        Label.text = label;
        SetValue(value);
    }

    private void SetValue(int value)
    {
        Mathf.Clamp(value, 0, MaxValue);
        Value = value;
        for(int i = 0; i < MaxValue; i++)
        {
            ValueContainer.transform.GetChild(i).gameObject.SetActive(i < value);
        }
    }

    private void IncreaseValue()
    {
        SetValue(Value + 1);
    }
    private void DecreaseValue()
    {
        SetValue(Value - 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
