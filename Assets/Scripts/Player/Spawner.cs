using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public delegate void SpawnEvent(GameObject player);
    public event SpawnEvent OnRespawn;

    public GameObject[] players;
    private GameObject player;
    private int currentPlayer;
    private List<DestructibleObj> destroyedObject = new List<DestructibleObj>();

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
        this.ResetObj();

        int rand;
        do
        {
            rand = Random.Range(0, players.Length);
        } while (rand == this.currentPlayer);

        this.player = Instantiate(players[rand], this.transform.position, Quaternion.identity);
        this.player.GetComponent<Player>().OnDead += new Player.DeathEvent(delegate () { this.Respawn(); });
        this.currentPlayer = rand;

        this.OnRespawn.Invoke(this.player);
    }
    void ResetObj()
    {
        for (int i = 0; i < this.destroyedObject.Count; i++)
            this.destroyedObject[i].Reset();
        this.destroyedObject.Clear();
    }
    public void AddObj(DestructibleObj obj)
    {
        this.destroyedObject.Add(obj);
    }
}