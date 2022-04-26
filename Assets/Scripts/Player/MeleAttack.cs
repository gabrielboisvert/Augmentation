using UnityEngine;

public class MeleAttack : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (this.GetComponentInParent<KnightControlle>() != null)
            if (!other.CompareTag("ground"))
                Destroy(other.gameObject);
    }
}
