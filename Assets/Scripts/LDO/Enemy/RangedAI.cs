using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAI : MonoBehaviour
{
    private GameObject player;
    public float speed = 2;
    public float attackRange = 3;
    public float attackCoolDown = 1;
    public GameObject bullet;
    public float rotationSpeed = 700;
    public bool dead = false;
    public AnimationClip[] clips;
    public GameObject aim;


    private Coroutine rotationAnimeCoro;
    private bool isAttacking = false;
    private float orientation = 1;
    private Animation anim;
    private AnimationClip current;
    private AudioSource src;
    public AudioClip[] clip;

    void Start()
    {
        this.anim = this.GetComponent<Animation>();

        this.anim.clip = this.current = this.anim.GetClip("Ranged_Idle");
        this.anim.Play();

        this.src = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.dead)
            return;

        if (player != null)
        {
            Vector3 newPos = Vector3.MoveTowards(this.transform.position, player.transform.position, Time.deltaTime * this.speed);

            if (this.transform.position.x - player.transform.position.x > 0)
            {
                if (this.orientation != -1)
                {
                    this.orientation = -1;
                    this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
                }
            }
            else
            {
                if (this.orientation != 1)
                {
                    this.orientation = 1;
                    this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
                }
            }

            if (Mathf.Abs(this.transform.position.x - player.transform.position.x) > this.attackRange)
            {
                newPos.y = this.transform.position.y;
                this.transform.position = newPos;
            }

            if (Vector3.Distance(this.transform.position, player.transform.position) < this.attackRange)
                this.Attack();

            if (!this.isAttacking)
            {
                if (this.current == this.anim.GetClip("Ranged_Idle"))
                    return;

                this.anim.clip = this.current = this.anim.GetClip("Ranged_Idle");
                this.anim.Play();
            }
        }
        else
        {
            if (this.current != this.anim.GetClip("Ranged_Idle"))
            {
                this.anim.clip = this.current = this.anim.GetClip("Ranged_Idle");
                this.anim.Play();
            }
            else if (!this.anim.isPlaying)
                this.anim.Play();
        }
    }

    void Attack()
    {
        if (this.dead)
            return;

        if (this.isAttacking)
            return;

        if (this.rotationAnimeCoro != null)
            return;

        this.isAttacking = true;
        this.anim.clip = this.current = this.anim.GetClip("Ranged_attack_Charge");
        this.anim.Play();

        StartCoroutine(this.StopAttacking());
    }

    IEnumerator StopAttacking()
    {
        yield return new WaitForSeconds(this.anim.GetClip("Ranged_attack_Charge").length);
        if (this.orientation == 1)
            Instantiate(this.bullet, this.aim.transform.position, Quaternion.Euler(0, 0, 270));
        else
            Instantiate(this.bullet, this.aim.transform.position, Quaternion.Euler(0, 0, 90));
        this.anim.clip = this.current = this.anim.GetClip("Ranged_Attack");
        this.anim.Play();

        this.src.clip = this.clip[0];

        yield return new WaitForSeconds(this.anim.GetClip("Ranged_Attack").length);
        this.isAttacking = false;
        this.anim.Stop();
    }

    IEnumerator RotateAnimation()
    {
        while (true)
        {
            if (this.orientation == 1)
            {
                transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 0, 0), this.rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 180, 0), this.rotationSpeed * Time.deltaTime);
            }

            if (this.transform.rotation.eulerAngles.y == 0 || this.transform.rotation.eulerAngles.y == 180)
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

    public void wasDead()
    {
        StartCoroutine(DestructAI());
    }

    IEnumerator DestructAI()
    {
        GameManager.Spawner.addObj(this.gameObject);
        this.dead = true;
        yield return new WaitForSeconds(0.3f);
        this.dead = false;
        this.isAttacking = false;
        this.anim.clip = this.current = this.anim.GetClip("Ranged_Idle");
        this.anim.Play();
        this.gameObject.SetActive(false);
    }
}
