using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuScript : MonoBehaviour
{
    public GameObject Object;
    public void SetObject()
    {
        if (Object == false)
        {
            Object.SetActive(true);
        }
        else
        {
            Object.SetActive(false);
        }
    }
    //не включается при нажатии, когда выключенно

}
