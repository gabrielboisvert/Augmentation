using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NinjaControlle : Player
{
    enum ANIMATION_STATE
    {
        IDLE,
        RUN,
        JUMP,
        ATTACK,
        SLIDE,
        WALL_JUMP,
        WALL_JUMP_SLIDE,
        DEATH,
    }

    public string[] animationStr;
    public CapsuleCollider[] fists;
    public float sideWallJumpForce = 10;
    public float dragSlide = 3.5f;
    public float dashSpeed = 400;
    public Vector3[] dashCollider;
    private Vector3[] originalCollider = new Vector3[2];

    private bool canDoubleJump = true;
    private bool hasDash = false;
    private bool onWall = false;
    private GameObject prevWall;
    //private BoxCollider originalCollider;

    public override void Start()
    {
        base.Start();
        this.transform.rotation = Quaternion.Euler(0, 90, 0);

        anim.clip = this.currentAnim = anim.GetClip(animationStr[(int)ANIMATION_STATE.IDLE]);
        anim.Play();

        this.originalCollider[0] = this.GetComponent<BoxCollider>().center;
        this.originalCollider[1] = this.GetComponent<BoxCollider>().size;
    }
    public override void Update()
    {
        if (this.isDead)
            return;

        this.m_body.drag = this.onWall ? this.dragSlide : 0;
        this.UpdateGravity();
        this.UpdateAnimationState();
    }
    protected override void UpdateAnimationState()
    {
        if (this.joystickSide != 0)
        {
            if (this.currentAnim == this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.RUN]) || this.isAttacking || !this.canJump || !this.canDoubleJump || this.hasDash)
                return;

            this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.RUN]);
            this.anim.Play();

            this.footstep.Play();
        }
        else if (Mathf.Abs(this.m_body.velocity.x) < 0.001f)
        {
            if (this.currentAnim == this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.IDLE]) || this.isAttacking || !this.canJump || !this.canDoubleJump || this.hasDash)
                return;

            this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.IDLE]);
            this.anim.Play();

            this.footstep.Stop();
        }
    }
    public override void FixedUpdate()
    {
        if (this.isDead || this.rotationAnimeCoro != null)
            return;

        this.m_body.velocity = new Vector3(Mathf.Clamp(this.m_body.velocity.x, -this.maxMovementSpeed, this.maxMovementSpeed), Mathf.Clamp(this.m_body.velocity.y, -this.maxJumpSpeed, this.maxJumpSpeed), 0);

        if (!this.hasDash)
            this.m_body.AddRelativeForce(-this.transform.right * (this.joystickSide * this.movementSpeed * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }
    protected override IEnumerator RotateAnimation()
    {
        while (true)
        {
            if (this.orientation == 1)
            {
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 90, 0), this.rotationSpeed * Time.deltaTime);
                this.rain.transform.localPosition = this.rainPosition;
            }
            else
            {
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 270, 0), this.rotationSpeed * Time.deltaTime);
                this.rain.transform.localPosition = new Vector3(-this.rainPosition.x, this.rainPosition.y, this.rainPosition.z);
            }

            if (this.transform.rotation.eulerAngles.y == 90 || this.transform.rotation.eulerAngles.y == 270)
                break;

            this.m_body.velocity = new Vector3(Mathf.Clamp(this.m_body.velocity.x, -this.maxMovementSpeed, this.maxMovementSpeed), Mathf.Clamp(this.m_body.velocity.y, -this.maxJumpSpeed, this.maxJumpSpeed), 0);
            yield return null;
        }

        this.rotationAnimeCoro = null;
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (this.isDead || !context.started || (!this.canJump && !this.canDoubleJump))
            return;


        this.m_body.velocity = new Vector3(this.m_body.velocity.x, 0, 0);

        if (this.onWall)
        {
            Vector3 jump = -this.transform.right * this.orientation * this.sideWallJumpForce;
            jump.y = this.jumpForce;

            this.anim.clip = this.currentAnim = anim.GetClip(animationStr[(int)ANIMATION_STATE.WALL_JUMP]);
            this.anim.Play();

            //this.src.PlayOneShot(this.clip[3]);
            //this.footstep.Stop();

            this.m_body.AddForce(jump, ForceMode.Impulse);
            //StartCoroutine(this.RestoreJoystick(0.5f));

            if (this.orientation == 1)
                this.transform.rotation = Quaternion.Euler(0, 90, 0);
            else
                this.transform.rotation = Quaternion.Euler(0, 270, 0);
        }
        else
        {
            this.m_body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);

            if (!this.inTheAir && this.attackCoro == null)
            {
                this.anim.clip = this.currentAnim = anim.GetClip(animationStr[(int)ANIMATION_STATE.JUMP]);
                this.anim.Play();
            }

            //this.src.PlayOneShot(this.clip[0]);
            //this.footstep.Stop();
        }

        if (this.canJump)
            this.canJump = false;
        else
            this.canDoubleJump = false;
        this.inTheAir = true;

        //if (this.hasDash)
        //    this.inTheAirDash = true;
    }
    public override void OnCollisionEnter(Collision collision)
    {
        if (this.isDead)
            return;

        if (collision.gameObject.CompareTag("AI") && collision.GetContact(0).normal == Vector3.up)
        {
            Vector3 jump = new Vector3(-this.transform.right.x * this.orientation * this.bunnyHopForce, this.jumpForce, 0);
            this.m_body.velocity = new Vector3(this.m_body.velocity.x, 0, 0);
            this.m_body.AddForce(jump, ForceMode.Impulse);
            this.canJump = true;

            if (!this.isAttacking)
            {
                this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.JUMP]);
                this.anim.Play();
            }
        }
        else if (collision.GetContact(0).normal == Vector3.up)
        {
            this.canJump = this.canDoubleJump = true;
            //this.inTheAirDash = false;
            this.inTheAir = false;

            //if (this.orientation == 1)
            //    this.transform.rotation = Quaternion.Euler(0, 90, 0);
            //else
            //    this.transform.rotation = Quaternion.Euler(0, 270, 0);

            //this.src.PlayOneShot(this.clip[5]);
            //this.footstep.Stop();
        }
        else if ((collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == Vector3.left) && !collision.gameObject.CompareTag("Platform"))
        {
            //this.footstep.Stop();

            if (rotationAnimeCoro != null)
                return;

            //if (this.orientation == 1)
            //{
            //    this.transform.rotation = Quaternion.Euler(0, 90, 0);
            //    this.orientation = -1;
            //}
            //else
            //{
            //    this.transform.rotation = Quaternion.Euler(0, 270, 0);
            //    this.orientation = 1;
            //}

            //if (this.current != this.anim.GetClip("ninja_walljump_slide") && !this.hasDash)
            //{
            //    this.anim.clip = this.current = this.anim.GetClip("ninja_walljump_slide");
            //    this.anim.Play();
            //}

            this.onWall = true;
            //this.inTheAirDash = false;
            this.inTheAir = false;
            if (this.prevWall != collision.gameObject)
                this.canJump = this.canDoubleJump = true;

            this.prevWall = collision.gameObject;
            //if (this.hasDash)
            //    this.body.velocity = new Vector3(0, this.body.velocity.y, 0);


        }
    }
    public void OnCollisionExit(Collision collision)
    {
        if (this.isDead)
            return;

        if (this.onWall && this.prevWall == collision.gameObject)
        {
            this.prevWall = null;
            this.onWall = false;
            this.inTheAir = true;
        }
    }
    public void Dash(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (!context.started || this.onWall || this.hasDash)
            return;

        //if (this.inTheAir)
        //{
        //    if (this.orientation == 1)
        //        this.transform.rotation = Quaternion.Euler(0, 90, 0);
        //    else
        //        this.transform.rotation = Quaternion.Euler(0, 270, 0);
        //}

        this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.SLIDE]);
        this.anim.Play();

        //this.src.PlayOneShot(this.clip[2]);
        //this.footstep.Stop();

        this.hasDash = true;
        this.GetComponent<BoxCollider>().center = this.dashCollider[0];
        this.GetComponent<BoxCollider>().size = this.dashCollider[1];
        StartCoroutine(this.StopSlide(this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.SLIDE]).length));
    }
    IEnumerator StopSlide(float duration)
    {
        float elapse = 0;
        while (elapse < duration)
        {
            elapse += Time.deltaTime;
            //if (!this.inTheAirDash)
            this.m_body.AddForce(new Vector3(this.orientation * this.dashSpeed * Time.deltaTime, 0, 0), ForceMode.Impulse);
            yield return null;
        }

        this.hasDash = false;
        this.GetComponent<BoxCollider>().center = this.originalCollider[0];
        this.GetComponent<BoxCollider>().size = this.originalCollider[1];
    }
    public override IEnumerator Dead()
    {
        this.isDead = true;
        this.m_body.velocity = Vector3.zero;

        this.anim.clip = this.currentAnim = this.anim.GetClip(animationStr[(int)ANIMATION_STATE.DEATH]);
        this.anim.Play();

        //this.m_audio.PlayOneShot(this.clip[5]);
        //this.footstep.Stop();

        yield return new WaitForSeconds(this.anim.GetClip(animationStr[(int)ANIMATION_STATE.DEATH]).length);
        Destroy(this.gameObject);
        this.FireDeath();
    }











    //
    //
    //private bool inTheAirDash = false;
    //private bool inTheAir = false;
    //private bool blockInput = false;
    //private float tmpInput = 0;
    //public float orientation = 1;
    //private GameObject prevWall;
    //private Vector3 prevWallNormal = Vector3.zero;
    //private float joystickSide = 0;
    //private Coroutine rotationAnimeCoro;
    //private Coroutine attackCoro;
    //private Animation anim;
    //private AnimationClip current;
    //private bool isDead = false;
    //private AudioSource src;


    //public float movementSpeed = 50;
    //public float jumpForce = 6.5f;
    //public float maxMovementSpeed = 5;
    //public float maxJumpSpeed = 10;
    //public float WallJumpForceX = 10;
    //public float userSlideDrag = 3.5f;

    //public float rotationSpeed = 700;
    //public GameObject attackRange;

    //public float slideHitBoxCenterOri = 0.5f;
    //public float slideHitBoxCenter = 0.25f;

    //public float slideHitBoxSizeOri = 1;
    //public float slideHitBoxSize = 0.5f;

    //public float gravityAdition = 15;

    //public AudioClip[] clip;
    //public AudioSource footstep;


    //public Vector2 inputMove;

    //public delegate void DeathEvent();
    //public event DeathEvent OnDead;

    //Start is called before the first frame update


    //public float Remap(float value, float from1, float to1, float from2, float to2)
    //{
    //    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    //}

    //Update is called once per frame
    //void Update()
    //{
    //    if (this.isDead)
    //        return;

    //    float friction = 20;

    //    float xVelocity = this.body.velocity.x;
    //    if (xVelocity != 0)
    //    {
    //        if (this.inputMove.x == 0 && !this.inTheAir)
    //            xVelocity = Mathf.MoveTowards(xVelocity, 0, friction * Time.deltaTime);
    //    }


    //    this.body.velocity = new Vector3(xVelocity, this.body.velocity.y - (this.gravityAdition * Time.deltaTime), this.body.velocity.z);

    //    this.body.drag = this.onWall ? this.userSlideDrag : 0;


    //    if (this.joystickSide != 0)
    //    {
    //        if (this.current == this.anim.GetClip("Ninja_run"))
    //            return;

    //        if (this.attackCoro != null)
    //            return;

    //        if (!this.canJump)
    //            return;

    //        if (this.hasDash)
    //            return;

    //        if (this.onWall)
    //            return;

    //        this.anim.clip = this.current = this.anim.GetClip("Ninja_run");
    //        this.anim.Play();

    //        this.footstep.Play();
    //    }
    //    else if (Mathf.Abs(this.body.velocity.x) < 0.001f)
    //    {
    //        if (this.attackCoro != null)
    //            return;

    //        if (!this.canJump)
    //            return;

    //        if (this.hasDash)
    //            return;

    //        if (this.onWall)
    //            return;

    //        if (this.current == this.anim.GetClip("ninja_idle"))
    //            return;

    //        this.anim.clip = this.current = this.anim.GetClip("ninja_idle");
    //        this.anim.Play();

    //        this.footstep.Stop();
    //    }
    //}




    //void FixedUpdate()
    //{
    //    if (this.isDead)
    //        return;

    //    this.body.velocity = new Vector3(Mathf.Clamp(this.body.velocity.x, -this.maxMovementSpeed, this.maxMovementSpeed), Mathf.Clamp(this.body.velocity.y, -this.maxJumpSpeed, this.maxJumpSpeed), 0);

    //    if (this.joystickSide == this.prevWallNormal.x)
    //        return;

    //    if (this.rotationAnimeCoro != null)
    //        return;

    //    Vector3 force = -this.transform.right * (this.joystickSide * this.movementSpeed * Time.fixedDeltaTime);
    //    this.body.AddRelativeForce(force, ForceMode.VelocityChange);
    //}




    //public void Move(InputAction.CallbackContext context)
    //{
    //    if (this.isDead)
    //        return;

    //    Vector2 direct = this.inputMove = context.ReadValue<Vector2>();

    //    if (!this.blockInput)
    //    {
    //        if (direct.x < 0)
    //        {
    //            if (this.onWall)
    //            {
    //                if (this.orientation == 1)
    //                {
    //                    this.footstep.Stop();
    //                    this.anim.Stop();
    //                }
    //                else
    //                {
    //                    this.joystickSide = this.tmpInput = this.orientation = -1;
    //                    transform.rotation = Quaternion.Euler(0, 270, 0);
    //                    return;
    //                }
    //            }

    //            this.joystickSide = this.tmpInput = this.orientation = -1;
    //            if (this.rotationAnimeCoro == null)
    //                this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
    //        }
    //        else if (direct.x > 0)
    //        {
    //            if (this.onWall)
    //            {
    //                if (this.orientation == -1)
    //                {
    //                    this.footstep.Stop();
    //                    this.anim.Stop();
    //                }
    //                else
    //                {
    //                    this.joystickSide = this.tmpInput = this.orientation = 1;
    //                    transform.rotation = Quaternion.Euler(0, 90, 0);
    //                    return;
    //                }
    //            }

    //            this.joystickSide = this.tmpInput = this.orientation = 1;
    //            if (this.rotationAnimeCoro == null)
    //                this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
    //        }
    //        else
    //            this.joystickSide = this.tmpInput = 0;
    //    }
    //    else
    //    {
    //        if (direct.x < 0)
    //            this.tmpInput = -1;
    //        else if (direct.x > 0)
    //            this.tmpInput = 1;
    //        else
    //            this.tmpInput = 0;
    //    }
    //}




    //IEnumerator RestoreJoystick(float duration)
    //{
    //    this.blockInput = true;
    //    this.joystickSide = this.orientation;
    //    yield return new WaitForSeconds(duration);
    //    this.joystickSide = this.tmpInput;
    //    this.blockInput = false;

    //    if (this.joystickSide != this.orientation && this.joystickSide != 0)
    //    {
    //        this.orientation = this.joystickSide;
    //        rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
    //    }
    //}









    //public void OnCollisionStay(Collision collision)
    //{
    //    if (this.isDead)
    //        return;

    //    if (collision.GetContact(0).normal == Vector3.up)
    //    {
    //        this.prevWall = null;
    //        this.prevWallNormal = Vector3.zero;
    //        this.onWall = false;
    //        this.inTheAirDash = false;
    //        this.inTheAir = false;
    //    }
    //    else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
    //    {
    //        this.prevWall = collision.gameObject;
    //        this.prevWallNormal = -collision.GetContact(0).normal;
    //        this.onWall = true;

    //        this.footstep.Stop();

    //        if (this.current != this.anim.GetClip("ninja_walljump_slide") && !this.hasDash)
    //        {
    //            this.anim.clip = this.current = this.anim.GetClip("ninja_idle");
    //            this.anim.Play();
    //        }
    //    }
    //}





    //public void MeleAttack(InputAction.CallbackContext context)
    //{
    //    if (this.isDead)
    //        return;

    //    if (this.attackCoro != null)
    //        return;

    //    if (context.started || context.performed)
    //        return;

    //    if (this.onWall)
    //        return;

    //    this.anim.clip = this.current = this.anim.GetClip("Ninja_Attack");
    //    this.anim.Play();


    //    this.attackCoro = StartCoroutine(this.DisableAttack(this.anim.GetClip("Ninja_Attack").length));

    //    this.src.PlayOneShot(this.clip[1]);
    //    this.footstep.Stop();
    //}

    //IEnumerator DisableAttack(float duration)
    //{
    //    this.attackRange.SetActive(true);
    //    yield return new WaitForSeconds(duration);
    //    this.attackRange.SetActive(false);
    //    this.attackCoro = null;
    //}

    //public void wasDead()
    //{
    //    StartCoroutine(this.dead());
    //}
}
