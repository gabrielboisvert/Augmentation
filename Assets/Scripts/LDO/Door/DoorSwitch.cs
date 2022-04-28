using UnityEngine;

public class DoorSwitch : MonoBehaviour
{
    public GameObject[] door;
    public AudioSource src;
    public AudioClip triggerHit;
    public AudioClip doorOpen;

    private bool isActivated = false;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            this.src.PlayOneShot(this.triggerHit);

            if (this.isActivated)
                return;

            this.isActivated = true;
            this.src.PlayOneShot(this.doorOpen);

            for (int i = 0; i < door.Length; i++)
            {
                GameManager.Spawner.addObj(door[i]);
                door[i].gameObject.SetActive(false);
            }
        }
    }
}
