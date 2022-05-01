using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player k = collision.gameObject.GetComponent<Player>();
            if (k != null)
            {
                k.Kill();
                return;
            }

            NinjaControlle n = collision.gameObject.GetComponent<NinjaControlle>();
            if (n != null)
            {
                StartCoroutine(n.dead());
                return;
            }

            ArcherControlle a = collision.gameObject.GetComponent<ArcherControlle>();
            if (a != null)
            {
                StartCoroutine(a.dead());
                return;
            }
        }
    }
}
