using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalInputManager : MonoBehaviour
{
    private GameObject Console;

    private void Start()
    {
        Console = GameObject.Find("Console");
        Console.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            Console.SetActive(!Console.activeSelf);
        }
    }
}
