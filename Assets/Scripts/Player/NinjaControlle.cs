using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NinjaControlle : MonoBehaviour
{
    private Rigidbody body;
    private bool canJump = true;
    private bool canDoubleJump = true;
    private bool hasDash = false;
    private bool onWall = false;
    private bool inTheAirDash = false;
    private bool inTheAir = false;
    private bool blockInput = false;
    private float tmpInput = 0;
    public float orientation = 1;
    private GameObject prevWall;
    private Vector3 prevWallNormal = Vector3.zero;
    private float joystickSide = 0;
    private Coroutine rotationAnimeCoro;
    private Coroutine attackCoro;
    private Animation anim;
    private AnimationClip current;
    private bool isDead = false;
    private AudioSource src;


    public float movementSpeed = 50;
    public float jumpForce = 6.5f;
    public float maxMovementSpeed = 5;
    public float maxJumpSpeed = 10;
    public float WallJumpForceX = 10;
    public float userSlideDrag = 3.5f;
    public float dashSpeed = 400;
    public float rotationSpeed = 700;
    public GameObject attackRange;

    public float slideHitBoxCenterOri = 0.5f;
    public float slideHitBoxCenter = 0.25f;

    public float slideHitBoxSizeOri = 1;
    public float slideHitBoxSize = 0.5f;

    public float gravityAdition = 15;

    public AudioClip[] clip;
    public AudioSource footstep;


    public Vector2 inputMove;

    public delegate void DeathEvent();
    public event DeathEvent OnDead;

    // Start is called before the first frame update
    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
        this.attackRange.SetActive(false);

        this.anim = this.GetComponent<Animation>();

        anim.clip = this.current = anim.GetClip("Idle");
        anim.Play();

        this.src = this.GetComponent<AudioSource>();

        GameManager.Player = this.gameObject;
    }

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isDead)
            return;

        float friction = 20;

        float xVelocity = this.body.velocity.x;
        if (xVelocity != 0)
        {
            if (this.inputMove.x == 0 && !this.inTheAir)
                xVelocity = Mathf.MoveTowards(xVelocity, 0, friction * Time.deltaTime);
        }
       
        
        this.body.velocity = new Vector3(xVelocity, this.body.velocity.y - (this.gravityAdition * Time.deltaTime), this.body.velocity.z);

        this.body.drag = this.onWall ? this.userSlideDrag : 0;


        if (this.joystickSide != 0)
        {
            if (this.current == this.anim.GetClip("Ninja_run"))
                return;

            if (this.attackCoro != null)
                return;

            if (!this.canJump)
                return;

            if (this.hasDash)
                return;

            if (this.onWall)
                return;

            this.anim.clip = this.current = this.anim.GetClip("Ninja_run");
            this.anim.Play();

            this.footstep.Play();
        }
        else if (Mathf.Abs(this.body.velocity.x) < 0.001f)
        {
            if (this.attackCoro != null)
                return;

            if (!this.canJump)
                return;

            if (this.hasDash)
                return;

            if (this.onWall)
                return;

            if (this.current == this.anim.GetClip("ninja_idle"))
                return;

            this.anim.clip = this.current = this.anim.GetClip("ninja_idle");
            this.anim.Play();

            this.footstep.Stop();
        }
    }

    void FixedUpdate()
    {
        if (this.isDead)
            return;

        this.body.velocity = new Vector3(Mathf.Clamp(this.body.velocity.x, -this.maxMovementSpeed, this.maxMovementSpeed), Mathf.Clamp(this.body.velocity.y, -this.maxJumpSpeed, this.maxJumpSpeed), 0);

        if (this.joystickSide == this.prevWallNormal.x)
            return;

        if (this.rotationAnimeCoro != null)
            return;

        Vector3 force = -this.transform.right * (this.joystickSide * this.movementSpeed * Time.fixedDeltaTime);
        this.body.AddRelativeForce(force, ForceMode.VelocityChange);
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        Vector2 direct = this.inputMove = context.ReadValue<Vector2>();

        if (!this.blockInput)
        {
            if (direct.x < 0)
            {
                if (this.onWall)
                {
                    if (this.orientation == 1)
                    {
                        this.footstep.Stop();
                        //this.anim.Stop();
                    }
                    else
                    {
                        this.joystickSide = this.tmpInput = this.orientation = -1;
                        transform.rotation = Quaternion.Euler(0, 270, 0);
                        return;
                    }
                }

                this.joystickSide = this.tmpInput = this.orientation = -1;
                if (this.rotationAnimeCoro == null)
                    this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
            }
            else if (direct.x > 0)
            {
                if (this.onWall)
                {
                    if (this.orientation == -1)
                    {
                        this.footstep.Stop();
                        //this.anim.Stop();
                    }
                    else
                    {
                        this.joystickSide = this.tmpInput = this.orientation = 1;
                        transform.rotation = Quaternion.Euler(0, 90, 0);
                        return;
                    }
                }

                this.joystickSide = this.tmpInput = this.orientation = 1;
                if (this.rotationAnimeCoro == null)
                    this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
            }
            else
                this.joystickSide = this.tmpInput = 0;
        }
        else
        {
            if (direct.x < 0)
                this.tmpInput = -1;
            else if (direct.x > 0)
                this.tmpInput = 1;
            else
                this.tmpInput = 0;
        }
    }

    IEnumerator RotateAnimation()
    {
        while (true)
        {
            if (this.orientation == 1)
                transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 90, 0), this.rotationSpeed * Time.deltaTime);
            else
                transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 270, 0), this.rotationSpeed * Time.deltaTime);

            if (this.transform.rotation.eulerAngles.y == 90 || this.transform.rotation.eulerAngles.y == 270)
                break;

            yield return null;
        }

        this.rotationAnimeCoro = null;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (!context.started || (!this.canJump && !this.canDoubleJump))
            return;

        this.body.velocity = new Vector3(this.body.velocity.x, 0, 0);

        if (this.onWall)
        {
            Vector3 jump = -this.transform.right * this.orientation * this.WallJumpForceX;
            jump.y = this.jumpForce;

            this.anim.clip = this.current = this.anim.GetClip("ninja_wallJump");
            this.anim.Play();

            this.src.PlayOneShot(this.clip[3]);
            this.footstep.Stop();

            this.body.AddForce(jump, ForceMode.Impulse);
            StartCoroutine(this.RestoreJoystick(0.5f));

            if (this.orientation == 1)
                this.transform.rotation = Quaternion.Euler(0, 90, 0);
            else
                this.transform.rotation = Quaternion.Euler(0, 270, 0);
        }
        else
        {
            this.body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);

            if (!this.inTheAir && this.attackCoro == null)
            {
                this.anim.clip = this.current = this.anim.GetClip("ninja_jump");
                this.anim.Play();
            }

            this.src.PlayOneShot(this.clip[0]);
            this.footstep.Stop();
        }

        if (this.canJump)
            this.canJump = false;
        else
            this.canDoubleJump = false;

        if (this.hasDash)
            this.inTheAirDash = true;

        this.inTheAir = true;
    }

    IEnumerator RestoreJoystick(float duration)
    {
        this.blockInput = true;
        this.joystickSide = this.orientation;
        yield return new WaitForSeconds(duration);
        this.joystickSide = this.tmpInput;
        this.blockInput = false;

        if (this.joystickSide != this.orientation && this.joystickSide != 0)
        {
            this.orientation = this.joystickSide;
            rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (this.isDead)
            return;

        if (collision.gameObject.CompareTag("AI"))
        {
            if (collision.GetContact(0).normal == Vector3.up)
            {
                this.body.velocity = new Vector3(this.body.velocity.x, 0, 0);

                Vector3 jump = -this.transform.right * this.orientation * 5;
                jump.y = jumpForce;

                this.body.AddForce(jump, ForceMode.Impulse);


                this.anim.clip = this.current = this.anim.GetClip("ninja_jump");
                this.anim.Play();

                this.src.PlayOneShot(this.clip[0]);
                this.footstep.Stop();
                return;
            }
        }

        if (collision.GetContact(0).normal == Vector3.up)
        {
            this.canJump = this.canDoubleJump = true;
            this.inTheAirDash = false;
            this.inTheAir = false;

            if (this.orientation == 1)
                this.transform.rotation = Quaternion.Euler(0, 90, 0);
            else
                this.transform.rotation = Quaternion.Euler(0, 270, 0);

            this.src.PlayOneShot(this.clip[5]);
            this.footstep.Stop();
        }
        else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
        {
            this.footstep.Stop();

            if (rotationAnimeCoro != null)
                return;

            if (this.orientation == 1)
            {
                this.transform.rotation = Quaternion.Euler(0, 90, 0);
                this.orientation = -1;
            }
            else
            {
                this.transform.rotation = Quaternion.Euler(0, 270, 0);
                this.orientation = 1;
            }

            if (this.current != this.anim.GetClip("ninja_walljump_slide") && !this.hasDash)
            {
                this.anim.clip = this.current = this.anim.GetClip("ninja_walljump_slide");
                this.anim.Play();
            }

            this.onWall = true;
            this.inTheAirDash = false;
            this.inTheAir = false;

            if (this.hasDash)
                this.body.velocity = new Vector3(0, this.body.velocity.y, 0);

            if (this.prevWall != collision.gameObject)
                this.canJump = this.canDoubleJump = true;
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (this.isDead)
            return;

        if (collision.GetContact(0).normal == Vector3.up)
        {
            this.prevWall = null;
            this.prevWallNormal = Vector3.zero;
            this.onWall = false;
            this.inTheAirDash = false;
            this.inTheAir = false;
        }
        else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
        {
            this.prevWall = collision.gameObject;
            this.prevWallNormal = -collision.GetContact(0).normal;
            this.onWall = true;

            this.footstep.Stop();

            if (this.current != this.anim.GetClip("ninja_walljump_slide") && !this.hasDash)
            {
                this.anim.clip = this.current = this.anim.GetClip("ninja_idle");
                this.anim.Play();
            }
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (this.isDead)
            return;

        this.prevWallNormal = Vector3.zero;
        this.onWall = false;
        this.inTheAir = true;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (!context.started || this.onWall || this.hasDash)
            return;

        if (this.inTheAir)
        {
            if (this.orientation == 1)
                this.transform.rotation = Quaternion.Euler(0, 90, 0);
            else
                this.transform.rotation = Quaternion.Euler(0, 270, 0);
        }

        this.anim.clip = this.current = this.anim.GetClip("Ninja_Slide");
        this.anim.Play();

        this.src.PlayOneShot(this.clip[2]);
        this.footstep.Stop();

        this.hasDash = true;
        this.GetComponent<BoxCollider>().center = new Vector3(0, this.slideHitBoxCenter, 0);
        this.GetComponent<BoxCollider>().size = new Vector3(1, this.slideHitBoxSize, 0.5f);
        StartCoroutine(this.StopSlide(0.5f));
    }

    IEnumerator StopSlide(float duration)
    {
        float elapse = 0;
        while (elapse < duration)
        {
            elapse += Time.deltaTime;
            if (!this.inTheAirDash)
                this.body.AddForce(new Vector3(this.orientation * this.dashSpeed * Time.deltaTime, 0, 0), ForceMode.Impulse);

            yield return null;
        }

        this.hasDash = false;
        this.GetComponent<BoxCollider>().center = new Vector3(0, this.slideHitBoxCenterOri, 0);
        this.GetComponent<BoxCollider>().size = new Vector3(1, this.slideHitBoxSizeOri, 0.5f);
    }

    public void MeleAttack(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (this.attackCoro != null)
            return;

        if (context.started || context.performed)
            return;

        if (this.onWall)
            return;

        this.anim.clip = this.current = this.anim.GetClip("Ninja_Attack");
        this.anim.Play();

        
        this.attackCoro = StartCoroutine(this.DisableAttack(this.anim.GetClip("Ninja_Attack").length));

        this.src.PlayOneShot(this.clip[1]);
        this.footstep.Stop();
    }

    IEnumerator DisableAttack(float duration)
    {
        this.attackRange.SetActive(true);
        yield return new WaitForSeconds(duration);
        this.attackRange.SetActive(false);
        this.attackCoro = null;
    }

    public void wasDead()
    {
        StartCoroutine(this.dead());
    }

    public IEnumerator dead()
    {
        this.anim.clip = this.current = this.anim.GetClip("ninja_dead");
        this.anim.Play();
        this.GetComponent<Collider>().enabled = false;
        this.body.isKinematic = true;
        this.isDead = true;
        this.src.PlayOneShot(this.clip[6]);
        this.footstep.Stop();
        yield return new WaitForSeconds(this.anim.GetClip("ninja_dead").length);
        Destroy(this.gameObject);

        this.OnDead.Invoke();
    }

    public void Pause(InputAction.CallbackContext context)
    {
        GameManager.Spawner.GetComponent<InGameMenu>().Pause(context);
    }
}
