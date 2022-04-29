using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20;

    void Update()
    {
        this.transform.position += (-this.transform.up * this.speed * Time.deltaTime);
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
            return;

        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            return;

        if (other.gameObject.CompareTag("AI"))
        {
            if (other.GetType() == typeof(BoxCollider))
            {
                RangedAI Ra = other.GetComponent<RangedAI>();
                if (Ra != null)
                {
                    Ra.wasDead();
                    Destroy(this.gameObject);
                    return;
                }

                MeleAI Ma = other.GetComponent<MeleAI>();
                if (Ma != null)
                {
                    Ma.wasDead();
                    Destroy(this.gameObject);
                    return;
                }
            }
            return;
        }

        Destroy(this.gameObject);
    }
}
