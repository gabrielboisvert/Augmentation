using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] players;
    public GameObject player;
    private int currentPlayer;
    public Camera cam;

    public delegate void SpawnEvent(GameObject player);
    public event SpawnEvent OnRespawn;

    private List<GameObject> destroyedObjec = new List<GameObject>();
    void Awake()
    {
        GameManager.Spawner = this;
    }

    private void Start()
    {
        this.Respawn();
    }

    void Respawn()
    {
        int rand;
        do
        {
            rand = Random.Range(0, players.Length);
        } while (rand == this.currentPlayer);

        this.player = Instantiate(players[rand]);
        this.currentPlayer = rand;

        this.player.transform.position = this.transform.position;

        //cam.GetComponent<CameraControl>().player = this.player;

        this.OnRespawn.Invoke(this.player);

        this.ResetObj();
    }

    void ResetObj()
    {
        for (int i = 0; i < this.destroyedObjec.Count; i++)
        {
            this.destroyedObjec[i].SetActive(true);

            DoorSwitch ds = this.destroyedObjec[i].GetComponent<DoorSwitch>();
            if (ds != null)
                ds.isActivated = false;
        
        }

        this.destroyedObjec.Clear();
    }

    public void addObj(GameObject obj)
    {
        this.destroyedObjec.Add(obj);
    }

    private void Update()
    {
        if (this.player == null)
        {
            this.Respawn();
            //StartCoroutine(this.cam.GetComponent<CameraControl>().StarLerp());
        }
    }
}
