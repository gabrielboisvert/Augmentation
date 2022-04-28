using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleAI : MonoBehaviour
{
    private GameObject player;
    public float speed = 2;
    public float attackRange = 3;
    public float attackCoolDown = 1;
    public GameObject meleAttack;
    public float rotationSpeed = 700;
    public bool dead = false;
    public AnimationClip[] clips;

    private bool isAttacking = false;
    private float orientation = 1;
    private Animation anim;
    private AnimationClip current;

    void Start()
    {
        this.anim = this.GetComponent<Animation>();

        this.anim.clip = this.current = this.anim.GetClip("Mele_idle");
        this.anim.Play();
    }

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
                    this.orientation = -1;
                else
                    this.orientation = 1;

                newPos.y = this.transform.position.y;
                this.transform.position = newPos;
            }

            if (Vector3.Distance(this.transform.position, player.transform.position) < this.attackRange)
                this.Attack();

            if (!this.isAttacking)
            {
                if (this.current == this.anim.GetClip("Mele_idle"))
                    return;

                this.anim.clip = this.current = this.anim.GetClip("Mele_idle");
                this.anim.Play();
            }
        }
    }

    void Attack()
    {
        if (this.dead)
            return;

        if (this.isAttacking)
            return;

        if (this.orientation == 1)
            this.transform.rotation = Quaternion.Euler(0, 180, 0);
        else
            this.transform.rotation = Quaternion.Euler(0, 360, 0);

        this.isAttacking = true;
        this.anim.clip = this.current = this.anim.GetClip("Mele_attack");
        this.anim.Play();

        StartCoroutine(this.StopAttacking());
    }

    IEnumerator StopAttacking()
    {
        yield return new WaitForSeconds(0.3f);
        this.meleAttack.SetActive(true);
        yield return new WaitForSeconds(this.anim.GetClip("Mele_attack").length - 0.6f);
        this.meleAttack.SetActive(false);
        this.isAttacking = false;
        this.anim.Stop();
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
