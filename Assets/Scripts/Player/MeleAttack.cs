using System.Collections;
using UnityEngine;

public class MeleAttack : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DestructibleBlock"))
            if (this.GetComponentInParent<KnightControlle>() != null)
                if (this.GetComponentInParent<KnightControlle>().isChargedAttack())
                    StartCoroutine(this.DestructBlock(other.gameObject));

        if (other.gameObject.CompareTag("AI"))
            if (other.GetType() == typeof(BoxCollider))
                StartCoroutine(this.DestructAI(other.gameObject));
    }

    IEnumerator DestructBlock(GameObject obj)
    {
        GameManager.Spawner.addObj(obj);
        obj.GetComponent<Collider>().isTrigger = true;
        yield return new WaitForSeconds(0.1f);
        obj.GetComponent<Collider>().isTrigger = true;
        obj.SetActive(false);
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
