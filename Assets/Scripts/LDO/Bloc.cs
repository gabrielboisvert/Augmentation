using System.Collections;
using UnityEngine;

public class Bloc : MonoBehaviour, DestructibleObj
{
    public AudioClip destructibleBloc;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Brawler br = other.GetComponentInParent<Brawler>();
            if (br != null && br.IsChargedAttack())
            {
                this.gameObject.SetActive(false);
                GameManager.Spawner.AddObj(this);
                GameManager.PlaySound(this.destructibleBloc);
            }
        }
    }
    public void Reset()
    {
        this.gameObject.SetActive(true);
    }
}
