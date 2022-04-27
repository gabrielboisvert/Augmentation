using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleAttackAI : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            KnightControlle k = other.GetComponent<KnightControlle>();
            if (k != null)
            {
                StartCoroutine(k.dead());
                return;
            }

            NinjaControlle n = other.GetComponent<NinjaControlle>();
            if (n != null)
            {
                //StartCoroutine(n.dead());
                Destroy(other.gameObject);
                return;
            }

            ArcherControlle a = other.GetComponent<ArcherControlle>();
            if (a != null)
            {
                //StartCoroutine(a.dead());
                Destroy(other.gameObject);
                return;
            }
        }
    }
}
