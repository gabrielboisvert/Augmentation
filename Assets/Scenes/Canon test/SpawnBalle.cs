using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBalle : MonoBehaviour
{

    float TimeBTWShoot;
    public float StartTimeBTWShoot;
    public Vector3 spawnPlace;
    public Vector3 direction;

    public GameObject balle;
    public GameObject canon;

    private void Start()
    {
        TimeBTWShoot = StartTimeBTWShoot;
        balle.GetComponent<BalleMouvement>().spawn = this;
    }

    private void Update()
    {
        direction = canon.transform.right;
        spawnPlace = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        if (TimeBTWShoot <= 0)
        {
            Instantiate(balle, spawnPlace, Quaternion.identity);
            TimeBTWShoot = StartTimeBTWShoot;
        }
        else
        {
            TimeBTWShoot -= Time.deltaTime;
        }
    }
}
