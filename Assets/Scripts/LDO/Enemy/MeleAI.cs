using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleAI : MonoBehaviour
{
    private GameObject player;
    public float speed = 2;
    public float attackRange = 1;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            Vector3 newPos = Vector3.MoveTowards(this.transform.position, player.transform.position, Time.deltaTime * this.speed);

            if (Mathf.Abs(this.transform.position.x - player.transform.position.x) > this.attackRange)
            {
                newPos.y = this.transform.position.y;
                this.transform.position = newPos;
            }

            if (Vector3.Distance(this.transform.position, player.transform.position) < 0.001f)
            {
                this.Attack();
            }
        }
    }

    void Attack()
    { 
          
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            player = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            player = null;
    }
}
