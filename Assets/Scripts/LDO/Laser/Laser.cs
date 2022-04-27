using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float restDuration;
    private float restTimer;

    public float duration;
    private float durationTimer;

    bool on = false;
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
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            KnightControlle k = other.GetComponent<KnightControlle>();
            if (k != null)
            {
                StartCoroutine(k.dead());
                return;
            }

            NinjaControlle n = other.GetComponent<NinjaControlle>();
            if (n != null)
            {
                StartCoroutine(n.dead());
                return;
            }

            ArcherControlle a = other.GetComponent<ArcherControlle>();
            if (a != null)
            {
                //StartCoroutine(a.dead());
                Destroy(other.gameObject);
                return;
            }
        }
    }
}
