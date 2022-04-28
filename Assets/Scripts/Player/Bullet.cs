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

        if (other.gameObject.CompareTag("AI"))
            StartCoroutine(this.DestructAI(other.gameObject));

        Destroy(this.gameObject);
    }

    IEnumerator DestructAI(GameObject obj)
    {
        GameManager.Spawner.addObj(obj);
        this.GetComponent<Collider>().isTrigger = true;
        yield return new WaitForSeconds(0.1f);
        obj.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            return;

        if (other.gameObject.CompareTag("AI"))
            return;

        Destroy(this.gameObject);
    }
}
