using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalleMouvement : MonoBehaviour
{
    public float speed;
    public GameObject balle;
    public SpawnBalle spawn;
    Vector3 balledirection;
    // Start is called before the first frame update
    void Start()
    {
        balledirection = spawn.direction;
    }

    // Update is called once per frame
    void Update()
    {

        transform.position += balledirection * speed * Time.deltaTime;
        Destroy(gameObject, 3);
    }
}