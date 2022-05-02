using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBullet : MonoBehaviour
{
    public float speed = 20;
    void Update()
    {
        this.transform.position += (this.transform.up * this.speed * Time.deltaTime);
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
            other.GetComponent<Player>().Kill(this.tag);
            
            //NinjaControlle n = other.GetComponent<NinjaControlle>();
            //if (n != null)
            //{
            //    n.wasDead();
            //    Destroy(this.gameObject);
            //    return;
            //}

            //ArcherControlle a = other.GetComponent<ArcherControlle>();
            //if (a != null)
            //{
            //    a.wasDead();
            //    Destroy(this.gameObject);
            //    return;
            //}
        }

        Destroy(this.gameObject);
    }
}