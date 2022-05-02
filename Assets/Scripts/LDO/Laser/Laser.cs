using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float restDuration;
    private float restTimer;

    public float duration;
    private float durationTimer;

    public AudioSource src;

    bool on = false;

    public ParticleSystem hit;
    
    void Start()
    {
        restTimer = Time.time;
        this.gameObject.GetComponent<Renderer>().enabled = false;
        this.gameObject.GetComponent<Collider>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.on)
        {
            if (Time.time - this.restTimer > this.restDuration)
            {
                this.on = true;
                this.durationTimer = Time.time;
                this.gameObject.GetComponent<Renderer>().enabled = false;
                this.gameObject.GetComponent<Collider>().enabled = false;

                hit.Stop();
                src.Stop();
            }
        }
        else
        {
            if (Time.time - this.durationTimer > this.duration)
            {
                this.on = false;
                this.restTimer = Time.time;
                this.gameObject.GetComponent<Renderer>().enabled = true;
                this.gameObject.GetComponent<Collider>().enabled = true;

                hit.Play();
                src.Play();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Player>().Kill();
            
            //NinjaControlle n = other.GetComponent<NinjaControlle>();
            //if (n != null)
            //{
            //    StartCoroutine(n.dead());
            //    return;
            //}

            //ArcherControlle a = other.GetComponent<ArcherControlle>();
            //if (a != null)
            //{
            //    StartCoroutine(a.dead());
            //    return;
            //}
        }
    }
}
