using UnityEngine;

public class DoorSwitch : MonoBehaviour
{
    public GameObject[] door;
    private AudioSource src;
    public AudioClip triggerHit;
    public AudioClip doorOpen;

    public bool isActivated = false;

    void Start()
    {
        this.src = this.GetComponent<AudioSource>();
    }

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
