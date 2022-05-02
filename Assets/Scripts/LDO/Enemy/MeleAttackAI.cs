using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleAttackAI : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Player>().Kill(this.tag);

            //NinjaControlle n = other.GetComponent<NinjaControlle>();
            //if (n != null)
            //{
            //    this.gameObject.SetActive(false);
            //    n.wasDead();
            //    return;
            //}

            //ArcherControlle a = other.GetComponent<ArcherControlle>();
            //if (a != null)
            //{
            //    this.gameObject.SetActive(false);
            //    a.wasDead();
            //    return;
            //}
        }
    }
}
