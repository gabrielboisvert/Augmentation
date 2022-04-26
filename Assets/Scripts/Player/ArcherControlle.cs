using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArcherControlle : MonoBehaviour
{
    private Rigidbody body;
    private bool canJump = true;
    private float joystickSide = 0;
    private Vector2 armRotation;
    private float orientation = 1;
    private Vector3 prevWallNormal = Vector3.zero;
    private Coroutine rotationAnimeCoro;
    private GameObject visibleFist;
    private bool isShooting = false;
    private float shootTimer;

    public float movementSpeed = 50;
    public float jumpForce = 6.5f;
    public float maxMovementSpeed = 5;
    public float maxJumpSpeed = 10;
    public float rotationSpeed = 700;
    public GameObject leftFist;
    public GameObject rightFist;
    public GameObject arrow;
    public float shootCooldown = 0.5f;

    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
        this.visibleFist = rightFist;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.isShooting)
            this.Shoot();

        float angle = 180 - (Mathf.Atan2(this.armRotation.x, this.armRotation.y) * Mathf.Rad2Deg);
        this.visibleFist.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (this.orientation == 1 && angle > 180)
        {
            this.orientation = -1;
            this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
            this.visibleFist = this.leftFist;
        }
        else if (this.orientation == -1 && angle < 180)
        {
            this.orientation = 1;
            this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
            this.visibleFist = this.rightFist;
        }


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
        if (!context.started || !this.canJump)
            return;

        this.body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);
        this.canJump = false;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            if (collision.GetContact(0).normal == Vector3.up)
                this.canJump = true;
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            if (collision.GetContact(0).normal == Vector3.up)
                this.prevWallNormal = Vector3.zero;
            else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
                this.prevWallNormal = -collision.GetContact(0).normal;
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
            this.prevWallNormal = Vector3.zero;
    }

    public void RangeAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            this.isShooting = true;
            this.shootTimer = Time.time;
        }
        else if (context.canceled)
            this.isShooting = false;
    }

    public void RotateArm(InputAction.CallbackContext context)
    {
        armRotation = context.ReadValue<Vector2>();
    }

    public void Shoot()
    {
        if (Time.time - this.shootTimer > this.shootCooldown)
        {
            this.shootTimer = Time.time;
            Instantiate(this.arrow, this.visibleFist.transform.position, this.visibleFist.transform.rotation);
        }
    }
}
