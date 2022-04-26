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
    private float orientation = 1;
    private GameObject prevWall;
    private Vector3 prevWallNormal = Vector3.zero;
    private float joystickSide = 0;



    public float movementSpeed = 50;
    public float jumpForce = 6.5f;
    public float maxMovementSpeed = 5;
    public float maxJumpSpeed = 10;
    public float WallJumpForceX = 10;
    public float userSlideDrag = 3.5f;
    public float dashSpeed = 10;


    // Start is called before the first frame update
    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        this.body.drag = this.onWall ? this.userSlideDrag : 0;
    }

    void FixedUpdate()
    {
        this.body.velocity = new Vector3(Mathf.Clamp(this.body.velocity.x, -this.maxMovementSpeed, this.maxMovementSpeed), Mathf.Clamp(this.body.velocity.y, -this.maxJumpSpeed, this.maxJumpSpeed), 0);

        if (this.joystickSide == this.prevWallNormal.x)
            return;

        this.body.AddRelativeForce(new Vector3(this.joystickSide * this.movementSpeed * Time.fixedDeltaTime, 0, 0), ForceMode.VelocityChange);
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 direct = context.ReadValue<Vector2>();

        if (!this.blockInput)
        {
            if (direct.x < 0)
                this.joystickSide = this.tmpInput = this.orientation = -1;
            else if (direct.x > 0)
                this.joystickSide = this.tmpInput = this.orientation = 1;
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

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started || (!this.canJump && !this.canDoubleJump))
            return;

        if (this.onWall)
        {
            this.orientation *= -1;
            this.body.AddForce(new Vector3(this.orientation * this.WallJumpForceX, this.jumpForce, 0), ForceMode.Impulse);
            StartCoroutine(this.RestoreJoystick(0.5f));
        }
        else
            this.body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);

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
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            if (collision.GetContact(0).normal == Vector3.up)
            {
                this.canJump = this.canDoubleJump = true;
                this.inTheAirDash = false;
                this.inTheAir = false;
            }
            else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
                if (this.prevWall != collision.gameObject)
                {
                    this.canJump = this.canDoubleJump = true;
                    this.inTheAirDash = false;
                    this.inTheAir = false;
                }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
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
                this.inTheAirDash = false;
                this.inTheAir = false;
            }
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            this.prevWallNormal = Vector3.zero;
            this.onWall = false;
            this.inTheAir = true;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!context.started || this.onWall || this.hasDash || this.inTheAir)
            return;

        this.hasDash = true;
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
    }
}
