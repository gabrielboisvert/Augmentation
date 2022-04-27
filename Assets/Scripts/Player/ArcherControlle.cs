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
    private bool isGrappling = false;
    private float shootTimer;
    private GameObject grabbedBlock;

    public float movementSpeed = 50;
    public float jumpForce = 6.5f;
    public float maxMovementSpeed = 5;
    public float maxJumpSpeed = 10;
    public float rotationSpeed = 700;
    public GameObject leftFist;
    public GameObject rightFist;
    public GameObject arrow;
    public GameObject grappinVisual;
    public float shootCooldown = 0.5f;
    public LayerMask mask;
    public float grabSpeed = 10;

    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
        this.visibleFist = rightFist;
    }

    private void Update()
    {
        if (this.grabbedBlock != null)
        {
            //Interpolate
            Vector3 newPos = Vector3.MoveTowards(this.transform.position, this.grabbedBlock.transform.position, this.grabSpeed * Time.deltaTime);
            this.transform.position = newPos;

            this.GetComponent<Collider>().isTrigger = true;
            this.body.isKinematic = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.grabbedBlock != null)
            return;

        if (this.isShooting)
            this.Shoot();

        RotateArmsUpdate();

        this.body.velocity = new Vector3(Mathf.Clamp(this.body.velocity.x, -this.maxMovementSpeed, this.maxMovementSpeed), Mathf.Clamp(this.body.velocity.y, -this.maxJumpSpeed, this.maxJumpSpeed), 0);

        if (this.joystickSide == this.prevWallNormal.x)
            return;

        if (this.rotationAnimeCoro != null)
            return;

        Vector3 force = -this.transform.right * (this.joystickSide * this.movementSpeed * Time.fixedDeltaTime);
        this.body.AddRelativeForce(force, ForceMode.VelocityChange);
    }

    private void RotateArmsUpdate()
    {
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

        //Show Calculate grap
        if (this.isGrappling)
        {
            RaycastHit hit;
            Debug.DrawRay(this.visibleFist.transform.position, -this.visibleFist.transform.up * 2000, Color.yellow);
            if (Physics.Raycast(this.visibleFist.transform.position, -this.visibleFist.transform.up, out hit, Mathf.Infinity, this.mask))
            {
                //Show visual here
            }
        }
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
            {
                transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 270, 0), this.rotationSpeed * Time.deltaTime);
                this.visibleFist = this.rightFist;
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(0, 90, 0), this.rotationSpeed * Time.deltaTime);
                this.visibleFist = this.leftFist;
            }

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
        if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("DestructibleBlock"))
            if (collision.GetContact(0).normal == Vector3.up)
                this.canJump = true;
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("DestructibleBlock"))
        {
            if (collision.GetContact(0).normal == Vector3.up)
                this.prevWallNormal = Vector3.zero;
            else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
                this.prevWallNormal = -collision.GetContact(0).normal;
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("DestructibleBlock"))
            this.prevWallNormal = Vector3.zero;
    }
    public void Grappling(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            this.isGrappling = true;
        }
        else if (context.canceled)
        {
            this.isGrappling = false;

            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, -this.visibleFist.transform.up, out hit, Mathf.Infinity, this.mask))
                if (hit.collider.gameObject.CompareTag("GrabBlock"))
                    this.grabbedBlock = hit.collider.gameObject;
        }
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
        //bool isMouse = context.control.device is Mouse;
        //bool isGamepad = context.control.device is Gamepad;

        //if (isMouse)
        //{
        //    Vector2 pos = context.ReadValue<Vector2>();
        //    armRotation = new Vector2(Mathf.Clamp(pos.x, -1, 1), Mathf.Clamp(pos.y, -1, 1));
        //}
        //else
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

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GrabBlock"))
        {
            this.grabbedBlock = null;
            this.GetComponent<Collider>().isTrigger = false;
            this.body.isKinematic = false;
        }
    }
}
