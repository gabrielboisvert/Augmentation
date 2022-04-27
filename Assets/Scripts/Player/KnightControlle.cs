using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor.Animations;

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

    public float movementSpeed = 50;
    public float jumpForce = 6.5f;
    public float maxMovementSpeed = 5;
    public float maxJumpSpeed = 10;
    public float dashSpeed = 400;
    public float rotationSpeed = 700;
    public GameObject attackRange;
    public float shieldDuration = 1;
    public float shieldCooldown = 2;

    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
        this.attackRange.SetActive(false);

        this.anim = this.GetComponent<Animation>();

        anim.clip = this.current = anim.GetClip("Idle");
        anim.Play();
    }

    private void Update()
    {
        if (this.hasShield)
        {
            if (Time.time - this.shieldTimer > this.shieldDuration)
            {
                this.hasShield = false;
                this.shieldCooldownTimer = Time.time;
                isShieldCooldown = true;
            }
        }

        if (this.isShieldCooldown)
        {
            if (Time.time - this.shieldCooldownTimer > this.shieldCooldown)
            {
                this.isShieldCooldown = false;
                Debug.Log("shield ready");
            }
        }

        if (this.joystickSide != 0)
        {
            if (this.current == this.anim.GetClip("Walk"))
                return;

            if (this.attackCoro != null || this.isChargeAttack)
                return;

            this.anim.clip = this.current = this.anim.GetClip("Walk");
            this.anim.Play();
        }
        else if (Mathf.Abs(this.body.velocity.x) < 0.001f)
        {
            if (this.current == this.anim.GetClip("Idle"))
                return;

            if (this.attackCoro != null || this.isChargeAttack)
                return;

            this.anim.clip = this.current = this.anim.GetClip("Idle");
            this.anim.Play();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
        if (!context.started || !this.canJump)
            return;

        this.body.velocity = new Vector3(this.body.velocity.x, 0, 0);

        this.body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);
        this.canJump = false;
    }

    public void OnCollisionEnter(Collision collision)
    {
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
                this.canJump = true;
        
            if (this.hasShield)
                return;
    }

    public void OnCollisionStay(Collision collision)
    {
        //if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("DestructibleBlock"))
            if (collision.GetContact(0).normal == Vector3.up)
                this.prevWallNormal = Vector3.zero;
            else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
                this.prevWallNormal = -collision.GetContact(0).normal;
    }

    public void OnCollisionExit(Collision collision)
    {
        //if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("DestructibleBlock"))
            this.prevWallNormal = Vector3.zero;
    }

    public void MeleAttack(InputAction.CallbackContext context)
    {
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
    }

    public void ChargeAttack(InputAction.CallbackContext context)
    {
        if (this.attackCoro != null)
            return;

        if (context.performed)
        {
            this.isChargeAttack = true;
            this.chargeTime = Time.time;

            this.anim.clip = this.current = this.anim.GetClip("Charge");
            this.anim.Play();
        }
        else if (context.canceled)
        {
            if (this.isChargeAttack)
            {
                if (Time.time - this.chargeTime < 0.65f)
                {
                    this.isChargeAttack = false;
                    return;
                }

                StartCoroutine(this.StopSlide(0.2f));
                this.attackRange.SetActive(true);
                this.attackCoro = StartCoroutine(this.DisableAttack(0.7f));

                this.anim.clip = this.current = this.anim.GetClip("DashPunch");
                this.anim.Play();
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
        if (this.isShieldCooldown)
            return;

        if (context.started)
        {
            this.shieldTimer = Time.time;
            this.hasShield = true;
        }
        else if (context.canceled)
        {
            this.hasShield = false;
            this.shieldCooldownTimer = Time.time;
            this.isShieldCooldown = true;
        }
    }
}
