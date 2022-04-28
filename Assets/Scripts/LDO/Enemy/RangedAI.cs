using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAI : MonoBehaviour
{
    private GameObject player;
    public float speed = 2;
    public float attackCoolDown = 1;
    public float rotationSpeed = 1400;
    public bool dead = false;
    public GameObject bullet;
    public LayerMask mask;
    public float range = 5;
    public GameObject pivot;
    public float angleRangeNeedToShoot = 10;

    private bool isAttacking = false;
    private float attackCoolDownTimer;
    private float orientation = 1;
    private Coroutine rotationAnimeCoro;
    private float detectionFov = 360;


    // Update is called once per frame
    void Update()
    {
        if (this.dead)
            return;

        if (player != null)
        {
            Vector3 newPos = Vector3.MoveTowards(this.transform.position, player.transform.position, Time.deltaTime * this.speed);

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

            if (this.player != null)
                this.CheckConeRange();
        }
    }

    private void CheckConeRange()
    {
        Collider[] rangeCheck = Physics.OverlapSphere(this.transform.position, this.range * this.transform.localScale.x, this.mask);
        if (rangeCheck.Length != 0)
        {
            Transform target = rangeCheck[0].transform;
            Vector3 dir = (target.position - this.transform.position);

            float angle = Vector3.Angle(this.transform.up, dir);
            if (angle < detectionFov / 2)
            {
                float dist = Vector3.Distance(this.transform.position, target.position);

                if (Physics.Raycast(this.transform.position, dir, dist, this.mask))
                {
                    Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, this.player.transform.position - this.transform.position);
                    float oldRot = targetRotation.eulerAngles.z;
                    //this.pivot.transform.rotation = Quaternion.RotateTowards(this.pivot.transform.rotation, targetRotation, Time.deltaTime * this.rotationSpeed);

                    if (Time.time - this.attackCoolDownTimer > this.attackCoolDown)
                    {
                        float zDiff = oldRot - this.pivot.transform.rotation.eulerAngles.z;
                        if (-this.angleRangeNeedToShoot <= zDiff && zDiff <= this.angleRangeNeedToShoot)
                        {
                            this.attackCoolDownTimer = Time.time;

                            //this.src.PlayOneShot(this.src.clip);
                            Instantiate(this.bullet, this.pivot.transform.position, this.pivot.transform.rotation);
                        }
                    }
                }
            }
        }
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
