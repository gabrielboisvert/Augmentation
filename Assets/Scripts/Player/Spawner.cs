using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] players;
    public GameObject player;
    public Camera cam;
    void Awake()
    {
        this.Respawn();
    }

    void Respawn()
    {
        int rand = Random.Range(0, players.Length);

        this.player = Instantiate(players[rand]);

        this.player.transform.position = this.transform.position;

        cam.GetComponent<CameraControl>().player = this.player;
    }

    private void Update()
    {
        if (this.player == null)
        {
            this.Respawn();
            StartCoroutine(this.cam.GetComponent<CameraControl>().StarLerp());
        }
    }
}
