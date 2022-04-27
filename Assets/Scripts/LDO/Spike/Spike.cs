using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KnightControlle k = collision.gameObject.GetComponent<KnightControlle>();
            if (k != null)
            {
                StartCoroutine(k.dead());
                return;
            }

            NinjaControlle n = collision.gameObject.GetComponent<NinjaControlle>();
            if (n != null)
            {
                //StartCoroutine(n.dead());
                Destroy(collision.gameObject);
                return;
            }

            ArcherControlle a = collision.gameObject.GetComponent<ArcherControlle>();
            if (a != null)
            {
                //StartCoroutine(a.dead());
                Destroy(collision.gameObject);
                return;
            }
        }
    }
}
