using System.Collections;
using UnityEngine;

public class MeleAttack : MonoBehaviour
{
    public AudioClip clip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DestructibleBlock"))
            if (this.GetComponentInParent<KnightControlle>() != null)
                if (this.GetComponentInParent<KnightControlle>().isChargedAttack())
                    StartCoroutine(this.DestructBlock(other.gameObject));

        if (other.gameObject.CompareTag("AI"))
        {
            if (other.GetType() == typeof(BoxCollider))
            {
                RangedAI Ra = other.GetComponent<RangedAI>();
                if (Ra != null)
                {
                    Ra.wasDead();
                    return;
                }

                MeleAI Ma = other.GetComponent<MeleAI>();
                if (Ma != null)
                {
                    Ma.wasDead();
                    return;
                }
            }
        }
    }

    IEnumerator DestructBlock(GameObject obj)
    {
        GameManager.Spawner.addObj(obj);
        GameManager.PlaySound(this.clip);
        obj.GetComponent<Collider>().isTrigger = true;
        yield return new WaitForSeconds(0.1f);
        obj.GetComponent<Collider>().isTrigger = false;
        obj.SetActive(false);
    }
}
