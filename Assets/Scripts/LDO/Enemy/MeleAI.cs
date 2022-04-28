using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleAI : MonoBehaviour
{
    private GameObject player;
    public float speed = 2;
    public float attackRange = 1;
    public float attackCoolDown = 1;
    public GameObject meleAttack;
    public float rotationSpeed = 700;
    public bool dead = false;

    private bool isAttacking = false;
    private float attackCoolDownTimer;
    private float orientation = 1;
    private Coroutine rotationAnimeCoro;

    // Update is called once per frame
    void Update()
    {
        if (this.dead)
            return;

        if (player != null)
        {
            Vector3 newPos = Vector3.MoveTowards(this.transform.position, player.transform.position, Time.deltaTime * this.speed);

            if (Mathf.Abs(this.transform.position.x - player.transform.position.x) > this.attackRange)
            {
                if (this.transform.position.x - player.transform.position.x < 0)
                {
                    if (this.orientation != 1)
                    {
                        this.orientation = 1;
                        this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
                    }
                }
                else
                {
                    if (this.orientation != -1)
                    {
                        this.orientation = -1;
                        this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
                    }
                }

                if (this.rotationAnimeCoro != null)
                    return;

                newPos.y = this.transform.position.y;
                this.transform.position = newPos;
            }

            if (Vector3.Distance(this.transform.position, player.transform.position) < this.attackRange)
                this.Attack();

            if (Time.time - this.attackCoolDownTimer > this.attackCoolDown)
                this.isAttacking = false;
        }
    }

    void Attack()
    {
        if (this.dead)
            return;

        if (this.isAttacking)
            return;

        this.isAttacking = true;
        this.attackCoolDownTimer = Time.time;

        this.meleAttack.SetActive(true);
        StartCoroutine(this.StopAttacking());
    }

    IEnumerator StopAttacking()
    {
        yield return new WaitForSeconds(0.2f);
        this.meleAttack.SetActive(false);
    }

    IEnumerator RotateAnimation()
    {
        while (true)
        {
            if (this.orientation == 1)
                transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 270, 0), this.rotationSpeed * Time.deltaTime);
            else
                transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 90, 0), this.rotationSpeed * Time.deltaTime);

            if (this.transform.rotation.eulerAngles.y == 90 || this.transform.rotation.eulerAngles.y == 270)
                break;

            yield return null;
        }

        this.rotationAnimeCoro = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.dead)
            return;

        if (other.CompareTag("Player"))
            player = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            player = null;
    }
}
