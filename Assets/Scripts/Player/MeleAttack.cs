using System.Collections;
using UnityEngine;

public class MeleAttack : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DestructibleBlock"))
            if (this.GetComponentInParent<KnightControlle>() != null)
                StartCoroutine(this.DestructBlock(other.gameObject));
    }

    IEnumerator DestructBlock(GameObject obj)
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(obj);
    }
}
