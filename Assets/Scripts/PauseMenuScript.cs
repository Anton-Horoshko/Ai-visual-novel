using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScript : MonoBehaviour
{
    public GameObject Object;

    public void ActivateObject()
    {
        Object.SetActive(!Object.activeSelf);
    }
}
