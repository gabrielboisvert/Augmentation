using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    //private PersistentMusic mainMenuMusic = null;
    private FadeSceneTransition fade = null;
    private Spawner spawner;

    public void Awake()
    {
        if (instance != null)
            Destroy(this.gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
    public static GameManager Instance { get => instance; set => instance = value; }
    public static FadeSceneTransition Fade { get => Instance.fade; set => Instance.fade = value; }
    public static Spawner Spawner { get => Instance.spawner; set => Instance.spawner = value; }
    public static void PlaySound(AudioClip clip)
    {
        GameObject sound = new GameObject("sound", typeof(AudioSource));
        sound.GetComponent<AudioSource>().PlayOneShot(clip);
    }
}