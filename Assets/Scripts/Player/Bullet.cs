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
                StartCoroutine(this.DestructAI(other.gameObject));
            return;
        }

        Destroy(this.gameObject);
    }

    IEnumerator DestructAI(GameObject obj)
    {
        GameManager.Spawner.addObj(obj);
        obj.GetComponent<MeleAI>().dead = true;
        yield return new WaitForSeconds(0.3f);
        obj.GetComponent<MeleAI>().dead = false;
        obj.SetActive(false);
    }
}
