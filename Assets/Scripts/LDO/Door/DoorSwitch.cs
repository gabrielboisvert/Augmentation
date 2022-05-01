using UnityEngine;

public class DoorSwitch : MonoBehaviour, DestructibleObj
{
    public GameObject[] doors;
    private AudioSource m_audio;
    public AudioClip triggerHit;
    public AudioClip doorOpen;

    public bool isActivated = false;

    void Start()
    {
        this.m_audio = this.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            this.m_audio.PlayOneShot(this.triggerHit);

            if (this.isActivated)
                return;

            this.isActivated = true;
            this.m_audio.PlayOneShot(this.doorOpen);

            for (int i = 0; i < doors.Length; i++)
                doors[i].gameObject.SetActive(false);

            GameManager.Spawner.AddObj(this);
        }
    }

    public void Reset()
    {
        this.isActivated = false;
        for (int i = 0; i < doors.Length; i++)
            doors[i].gameObject.SetActive(true);
    }
}