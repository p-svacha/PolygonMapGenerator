using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonDeselect : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(null));
    }
}