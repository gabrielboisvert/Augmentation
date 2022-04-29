using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text timerUI;
    public float timer;

    private void Start()
    {
        timer = 0;


    }

    public void UpdateTimer()
    {
        timer += Time.deltaTime;
        this.timerUI.text = TimeSpan.FromMilliseconds((timer * 1000)).ToString(@"m\:ss\:ff");

    }
    // Update is called once per frame
    void Update()
    {
        UpdateTimer();


    }
}
