using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public InGameMenu menu;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            menu.OnWin();
    }
}
