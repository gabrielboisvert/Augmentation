using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBullet : MonoBehaviour
{
    public float speed = 20;
    void Update()
    {
        this.transform.position += (this.transform.right * this.speed * Time.deltaTime);
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Tower"))
            return;

        if (other.gameObject.CompareTag("AI"))
            return;
            
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Tower"))
            return;

        if (other.gameObject.CompareTag("AI"))
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            KnightControlle k = other.gameObject.GetComponent<KnightControlle>();
            if (k != null)
            {
                StartCoroutine(k.dead());
                return;
            }

            NinjaControlle n = other.gameObject.GetComponent<NinjaControlle>();
            if (n != null)
            {
                StartCoroutine(n.dead());
                return;
            }

            ArcherControlle a = other.gameObject.GetComponent<ArcherControlle>();
            if (a != null)
            {
                StartCoroutine(a.dead());
                return;
            }
        }

        Destroy(this.gameObject);
    }
}