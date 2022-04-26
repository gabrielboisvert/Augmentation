using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    private float sideDirection;
    private Rigidbody body;
    public float speed = 50;
    public float jumpForce = 10;
    public float maxSpeed = 3;
    private bool hasJump = false;
    private bool hasDoubleJump = false;
    private GameObject previousWallJump;
    private GameObject prevPreviousWallJump;
    private float xSideJump;
    public float WallJumpForceX = 10;
    public float userSlideDrag = 3.5f;
    private bool hasSlide = false;
    private float orientation = 1;
    public float slideSpeed = 50;
    private bool slideJump = false;

    // Start is called before the first frame update
    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.previousWallJump != null && !this.hasJump)
            this.body.drag = this.userSlideDrag;
        else
            this.body.drag = 0;
    }

    void FixedUpdate()
    {
        if (this.body.velocity.x > this.maxSpeed)
            this.body.velocity = new Vector3(this.maxSpeed, this.body.velocity.y, 0);
        else if (this.body.velocity.x < -this.maxSpeed)
            this.body.velocity = new Vector3(-this.maxSpeed, this.body.velocity.y, 0);

        if (this.previousWallJump == null)
            this.body.AddForce(new Vector3(this.sideDirection * Time.deltaTime * speed, 0, 0), ForceMode.VelocityChange);
        else if (this.sideDirection == this.xSideJump)
        {
            this.body.AddForce(new Vector3(this.sideDirection * Time.deltaTime * speed, 0, 0), ForceMode.VelocityChange);
            this.prevPreviousWallJump = this.previousWallJump;
            this.previousWallJump = null;
        }
        
        if (this.slideJump)
            this.body.AddForce(new Vector3(this.orientation * Time.deltaTime * this.slideSpeed, 0, 0), ForceMode.Impulse);
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 direct = context.ReadValue<Vector2>();

        if (direct.x < 0)
        {
            this.sideDirection = -1;
            this.orientation = -1;
        }
        else if (direct.x > 0)
        {
            this.sideDirection = 1;
            this.orientation = 1;
        }
        else
            this.sideDirection = 0;
    }

    public void Slide(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        if (this.hasJump)
            return;

        if (this.hasSlide)
            return;

        if (this.previousWallJump != null)
            return;

        this.hasSlide = true;

        StartCoroutine(this.StopSlide(0.5f));
    }

    IEnumerator StopSlide(float duration)
    {
        float elapse = 0;
        while (elapse < duration)
        {
            this.body.AddForce(new Vector3(this.orientation * Time.deltaTime * this.slideSpeed, 0, 0), ForceMode.Impulse);
            elapse += Time.deltaTime;
            yield return null;
        }

        this.hasSlide = false;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        if (this.hasDoubleJump)
            return;

        if (this.previousWallJump == this.prevPreviousWallJump && this.previousWallJump != null)
            return;

        if (this.previousWallJump != null || this.prevPreviousWallJump != null)
        {
            this.body.AddForce(new Vector3(this.xSideJump * this.WallJumpForceX, this.jumpForce, 0), ForceMode.Impulse);
            this.orientation *= -1;
        }
        else
            this.body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);

        this.prevPreviousWallJump = this.previousWallJump;
        this.previousWallJump = null;
        
        if (this.hasJump)
            this.hasDoubleJump = true;
        else
            this.hasJump = true;

        if (this.hasSlide)
            this.slideJump = true;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            if (collision.GetContact(0).normal == Vector3.up)
            {
                this.hasJump = false;
                this.previousWallJump = null;
                this.prevPreviousWallJump = null;
            }
            else if ((collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right) && this.previousWallJump != collision.gameObject)
            {
                this.hasJump = false;
                this.previousWallJump = collision.gameObject;

                this.xSideJump = collision.GetContact(0).normal.x;
            }

            this.slideJump = false;
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            if (collision.GetContact(0).normal == Vector3.up)
            {
                this.hasJump = false;
                this.hasDoubleJump = false;
            }

    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            this.hasJump = true;
        }
    }
}
