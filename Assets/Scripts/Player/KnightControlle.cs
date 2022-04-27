using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KnightControlle : MonoBehaviour
{
    private Rigidbody body;
    private bool canJump = true;
    private float joystickSide = 0;
    private float orientation = 1;
    private Vector3 prevWallNormal = Vector3.zero;
    private Coroutine rotationAnimeCoro;
    private Coroutine attackCoro;
    private bool hasShield = false;
    private bool isChargeAttack = false;
    private float shieldTimer;
    private float shieldCooldownTimer;
    private bool isShieldCooldown = false;
    private Animation anim;
    private AnimationClip current;
    private float chargeTime;
    private AudioSource src;
    private bool isDead = false;

    public float movementSpeed = 50;
    public float jumpForce = 6.5f;
    public float maxMovementSpeed = 5;
    public float maxJumpSpeed = 10;
    public float dashSpeed = 400;
    public float rotationSpeed = 700;
    public GameObject attackRange;
    public float shieldDuration = 1;
    public float shieldCooldown = 2;
    public AudioClip[] clip;
    public AudioSource footstep;

    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
        this.attackRange.SetActive(false);

        this.anim = this.GetComponent<Animation>();

        anim.clip = this.current = anim.GetClip("Idle");
        anim.Play();

        this.src = this.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (this.isDead)
            return;

        if (this.hasShield)
        {
            if (Time.time - this.shieldTimer > this.shieldDuration)
            {
                this.hasShield = false;
                this.shieldCooldownTimer = Time.time;
                isShieldCooldown = true;

                this.anim.clip = this.current = this.anim.GetClip("block_stopped");
                this.anim.Play();
            }
        }

        if (this.isShieldCooldown)
            if (Time.time - this.shieldCooldownTimer > this.shieldCooldown)
                this.isShieldCooldown = false;

        if (this.joystickSide != 0)
        {
            if (this.current == this.anim.GetClip("Walk"))
                return;

            if (this.attackCoro != null || this.isChargeAttack)
                return;

            if (!this.canJump)
                return;

            if (this.hasShield)
                return;

            this.anim.clip = this.current = this.anim.GetClip("Walk");
            this.anim.Play();

            this.footstep.Play();
        }
        else if (Mathf.Abs(this.body.velocity.x) < 0.001f)
        {
            if (this.current == this.anim.GetClip("Idle"))
                return;

            if (this.attackCoro != null || this.isChargeAttack)
                return;

            if (!this.canJump)
                return;

            if (this.hasShield)
                return;

            this.anim.clip = this.current = this.anim.GetClip("Idle");
            this.anim.Play();
        }
    }

    // Update is called once per frame
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

        Vector2 direct = context.ReadValue<Vector2>();

        if (direct.x < 0)
        {
            this.joystickSide = this.orientation = -1;
            if (this.rotationAnimeCoro == null)
                this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
        }
        else if (direct.x > 0)
        {
            this.joystickSide = this.orientation = 1;
            if (this.rotationAnimeCoro == null)
                this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
        }
        else
            this.joystickSide = 0;
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

    public void Jump(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (!context.started || !this.canJump)
            return;

        this.body.velocity = new Vector3(this.body.velocity.x, 0, 0);

        this.body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);
        this.canJump = false;

        this.anim.clip = this.current = this.anim.GetClip("Jump");
        this.anim.Play();

        this.src.PlayOneShot(this.clip[3]);
        this.footstep.Stop();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (this.isDead)
            return;

        if (collision.gameObject.CompareTag("AI"))
            if (collision.GetContact(0).normal == Vector3.up)
            {
                this.body.velocity = new Vector3(this.body.velocity.x, 0, 0);

                Vector3 jump = -this.transform.right * this.orientation * 5;
                jump.y = 5;

                this.body.AddForce(jump, ForceMode.Impulse);
                return;
            }

        //if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("DestructibleBlock"))
        if (collision.GetContact(0).normal == Vector3.up)
        {
            this.canJump = true;
            this.src.PlayOneShot(this.clip[2]);
            this.footstep.Stop();
        }
        
        if (this.hasShield)
            return;
    }

    public void OnCollisionStay(Collision collision)
    {
        if (this.isDead)
            return;

        //if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("DestructibleBlock"))
        if (collision.GetContact(0).normal == Vector3.up)
                this.prevWallNormal = Vector3.zero;
            else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
                this.prevWallNormal = -collision.GetContact(0).normal;
    }

    public void OnCollisionExit(Collision collision)
    {
        if (this.isDead)
            return;

        //if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("DestructibleBlock"))
        this.prevWallNormal = Vector3.zero;
    }

    public void MeleAttack(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (this.attackCoro != null)
            return;

        if (context.started || context.performed)
            return;

        if (this.isChargeAttack)
            return;

        this.attackRange.SetActive(true);
        this.attackCoro = StartCoroutine(this.DisableAttack(0.7f));
        
        this.anim.clip = this.current = this.anim.GetClip("Punch");
        this.anim.Play();

        this.src.PlayOneShot(this.clip[1]);
        this.footstep.Stop();
    }

    public void ChargeAttack(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (this.attackCoro != null)
            return;

        if (context.performed)
        {
            this.isChargeAttack = true;
            this.chargeTime = Time.time;

            this.anim.clip = this.current = this.anim.GetClip("Charge");
            this.anim.Play();

            this.src.PlayOneShot(this.clip[4]);
            this.footstep.Stop();
        }
        else if (context.canceled)
        {
            if (this.isChargeAttack)
            {
                if (Time.time - this.chargeTime < 0.65f)
                {
                    this.isChargeAttack = false;
                    this.src.Stop();
                    return;
                }

                StartCoroutine(this.StopSlide(0.2f));
                this.attackRange.SetActive(true);
                this.attackCoro = StartCoroutine(this.DisableAttack(0.7f));

                this.anim.clip = this.current = this.anim.GetClip("DashPunch");
                this.anim.Play();

                this.src.PlayOneShot(this.clip[0]);
                this.footstep.Stop();
            }
        }
    }

    IEnumerator StopSlide(float duration)
    {
        float elapse = 0;
        while (elapse < duration)
        {
            elapse += Time.deltaTime;
            this.body.AddForce(new Vector3(this.orientation * this.dashSpeed * Time.deltaTime, 0, 0), ForceMode.Impulse);

            yield return null;
        }
    }

    IEnumerator DisableAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
        this.attackRange.SetActive(false);

        this.attackCoro = null;
        this.isChargeAttack = false;
    }

    public void Shield(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (this.isShieldCooldown)
            return;

        if (context.started)
        {
            this.shieldTimer = Time.time;
            this.hasShield = true;

            this.anim.clip = this.current = this.anim.GetClip("block_actif");
            this.anim.Play();
        }
        else if (context.canceled)
        {
            this.hasShield = false;
            this.shieldCooldownTimer = Time.time;
            this.isShieldCooldown = true;

            this.anim.clip = this.current = this.anim.GetClip("block_stopped");
            this.anim.Play();
        }
    }

    public IEnumerator dead()
    {
        this.anim.clip = this.current = this.anim.GetClip("DeadAnim");
        this.anim.Play();
        this.GetComponent<Collider>().enabled = false;
        this.body.isKinematic = true;
        this.isDead = true;
        this.src.PlayOneShot(this.clip[5]);
        yield return new WaitForSeconds(this.anim.GetClip("DashPunch").length + 0.5f);
        Destroy(this.gameObject);
    }

    public bool isChargedAttack()
    {
        return this.isChargeAttack;
    }
}
