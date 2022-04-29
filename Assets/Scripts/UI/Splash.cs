using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splash : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(switchScreen());
    }

    IEnumerator switchScreen()
    {
        yield return new WaitForSeconds(1.5f);
        GameManager.Fade.FadeStart("Menu", 1f);
        
    }
}
