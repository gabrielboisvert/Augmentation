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
    private Animation anim;
    private AnimationClip current;
    private bool isDead;
    private Coroutine shootingCoro;
    private LineRenderer line;
    private AudioSource src;

    public float movementSpeed = 50;
    public float jumpForce = 6.5f;
    public float maxMovementSpeed = 5;
    public float maxJumpSpeed = 10;
    public float rotationSpeed = 700;
    public GameObject leftFist;
    public GameObject rightFist;
    public GameObject arrow;
    public float shootCooldown = 0.5f;
    public LayerMask mask;
    public float grabSpeed = 10;

    public float gravityAdition = 15;

    public AudioClip[] clip;
    public AudioSource footstep;

    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
        this.visibleFist = rightFist;

        this.line = this.GetComponent<LineRenderer>();

        this.anim = this.GetComponent<Animation>();
        anim.clip = this.current = anim.GetClip("Shooter_idle");
        anim.Play();

        this.src = this.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (this.isDead)
            return;

        this.body.velocity = new Vector3(this.body.velocity.x, this.body.velocity.y - (this.gravityAdition * Time.deltaTime), this.body.velocity.z);

        if (this.grabbedBlock != null)
        {
            //Interpolate
            Vector3 newPos = Vector3.MoveTowards(this.transform.position, this.grabbedBlock.transform.position, this.grabSpeed * Time.deltaTime);
            this.transform.position = newPos;

            this.GetComponent<Collider>().isTrigger = true;
            this.body.isKinematic = true;
        }

        if (this.joystickSide != 0)
        {
            if (this.current == this.anim.GetClip("Shooter_run"))
                return;

            if (!this.canJump)
                return;

            if (this.shootingCoro != null || this.isShooting)
                return;

            if ((this.armRotation != Vector2.zero))
                return;

            this.anim.clip = this.current = this.anim.GetClip("Shooter_run");
            this.anim.Play();

            this.footstep.Play();
        }
        else if (Mathf.Abs(this.body.velocity.x) < 0.001f)
        {
            if (this.current == this.anim.GetClip("Shooter_idle"))
                return;

            if (!this.canJump)
                return;

            if (this.shootingCoro != null || this.isShooting)
                return;

            if ((this.armRotation != Vector2.zero))
                return;

            this.anim.clip = this.current = this.anim.GetClip("Shooter_idle");
            this.anim.Play();

            this.footstep.Stop();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.isDead)
            return;

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
        if (this.isDead)
            return;

        if (this.armRotation != Vector2.zero)
            this.anim.Stop();
        else
            this.anim.Play();

        if (this.rotationAnimeCoro != null)
            return;

        float angle = 180 - (Mathf.Atan2(this.armRotation.x, this.armRotation.y) * Mathf.Rad2Deg);
        this.visibleFist.transform.rotation = Quaternion.Euler(angle, -90, 90);

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
            this.line.SetPosition(0, this.transform.position);
            this.line.SetPosition(1, this.transform.position);

            RaycastHit hit;
            if (Physics.Raycast(this.visibleFist.transform.position, new Vector3(this.armRotation.x, this.armRotation.y, 0), out hit, Mathf.Infinity, this.mask))
            {
                this.line.SetPosition(0, this.visibleFist.transform.position);
                this.line.SetPosition(1, hit.point);
            }
            else
            {
                this.line.SetPosition(0, this.visibleFist.transform.position);
                this.line.SetPosition(1, this.visibleFist.transform.position + new Vector3(this.armRotation.x, this.armRotation.y, 0) * 10);
            }
        }
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
        if (this.isDead)
            return;

        if (!context.started || !this.canJump)
            return;

        this.body.velocity = new Vector3(this.body.velocity.x, 0, 0);

        this.body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);
        this.canJump = false;

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

        if (collision.GetContact(0).normal == Vector3.up)
        {
            this.canJump = true;

            this.src.PlayOneShot(this.clip[2]);
            this.footstep.Stop();
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (this.isDead)
            return;

        if (collision.GetContact(0).normal == Vector3.up)
            this.prevWallNormal = Vector3.zero;
        else if (collision.GetContact(0).normal == Vector3.right || collision.GetContact(0).normal == -Vector3.right)
            this.prevWallNormal = -collision.GetContact(0).normal;
    }

    public void OnCollisionExit(Collision collision)
    {
        if (this.isDead)
            return;

        this.prevWallNormal = Vector3.zero;
    }
    public void Grappling(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (context.started)
        {
            this.isGrappling = true;
            this.line.enabled = true;

            this.line.SetPosition(0, this.transform.position);
            this.line.SetPosition(1, this.transform.position);
        }
        else if (context.canceled)
        {
            this.isGrappling = false;
            this.line.enabled = false;

            this.line.SetPosition(0, this.transform.position);
            this.line.SetPosition(1, this.transform.position);

            RaycastHit hit;
            if (Physics.Raycast(this.visibleFist.transform.position, new Vector3(this.armRotation.x, this.armRotation.y, 0), out hit, Mathf.Infinity, this.mask))
                if (hit.collider.gameObject.CompareTag("GrabBlock"))
                {
                    this.grabbedBlock = hit.collider.gameObject;

                    this.src.PlayOneShot(this.clip[0]);
                    this.footstep.Stop();
                }
        }
    }
    public void RangeAttack(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        if (context.started)
        {
            this.isShooting = true;
        }
        else if (context.canceled)
        {
            this.isShooting = false;
            if (this.shootingCoro != null)
            {
                StopCoroutine(this.shootingCoro);
                this.shootingCoro = null;
            }
        }
    }

    public void RotateArm(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        armRotation = context.ReadValue<Vector2>();
    }

    public void Shoot()
    {
        if (this.isDead)
            return;

        if (this.shootingCoro == null)
            shootingCoro = StartCoroutine(this.shootBullet());
    }

    IEnumerator shootBullet()
    {
        this.anim.clip = this.current = this.anim.GetClip("Shooter_shoot");
        this.anim.Play();

        yield return new WaitForSeconds(this.anim.GetClip("Shooter_shoot").length);

        this.src.PlayOneShot(this.clip[1]);
        this.footstep.Stop();

        if (this.orientation == -1)
        {
            float angle = Vector3.Angle(new Vector3(this.armRotation.x, this.armRotation.y, 0), Vector3.up);
            Instantiate(this.arrow, this.visibleFist.transform.position, Quaternion.Euler(0, 0, angle + 180));
        }
        else
        {
            float angle = Vector3.Angle(new Vector3(this.armRotation.x, this.armRotation.y, 0), -Vector3.up);
            Instantiate(this.arrow, this.visibleFist.transform.position, Quaternion.Euler(0, 0, angle));
        }

        this.shootingCoro = null;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (this.isDead)
            return;

        if (other.gameObject.CompareTag("GrabBlock"))
        {
            this.grabbedBlock = null;
            this.GetComponent<Collider>().isTrigger = false;
            this.body.isKinematic = false;

            this.canJump = true;
        }
    }

    public void wasDead()
    {
        StartCoroutine(this.dead());
    }

    public IEnumerator dead()
    {
        this.anim.clip = this.current = this.anim.GetClip("Shooter_dead");
        this.anim.Play();
        this.GetComponent<Collider>().enabled = false;
        this.body.isKinematic = true;
        this.isDead = true;
        this.src.PlayOneShot(this.clip[4]);
        this.footstep.Stop();
        float time = this.anim.GetClip("Shooter_dead").length;
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }

    public void Pause(InputAction.CallbackContext context)
    {
        GameManager.Spawner.GetComponent<InGameMenu>().Pause(context);
    }
}
