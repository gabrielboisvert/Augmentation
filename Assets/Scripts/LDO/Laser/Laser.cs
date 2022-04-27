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
                this.duration = Time.time;
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
}
