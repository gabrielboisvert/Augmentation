using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleAttack : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<KnightControlle>() != null)
            if (!other.CompareTag("ground"))
                Destroy(other.gameObject);
    }
}
