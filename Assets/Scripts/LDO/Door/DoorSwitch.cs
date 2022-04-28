using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitch : MonoBehaviour
{
    public GameObject[] door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            for (int i = 0; i < door.Length; i++)
            {

                GameManager.Spawner.addObj(door[i]);
                door[i].gameObject.SetActive(false);
            }
        }
    }
}
