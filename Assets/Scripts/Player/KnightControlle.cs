using System.Collections;
using System.Collections.Generic;
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

    public float movementSpeed = 50;
    public float jumpForce = 6.5f;
    public float maxMovementSpeed = 5;
    public float maxJumpSpeed = 10;
    public float dashSpeed = 400;
    public float rotationSpeed = 700;
    public GameObject Shield;
    public GameObject attackRange;

    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
        this.attackRange.SetActive(false);
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

    public void MeleAttack(InputAction.CallbackContext context)
    {
        if (this.attackCoro != null)
            return;

        if (context.started || context.performed)
            return;

        if (this.isChargeAttack)
            return;

        this.attackRange.SetActive(true);
        this.attackCoro = StartCoroutine(this.DisableAttack(0.2f));
    }

    public void ChargeAttack(InputAction.CallbackContext context)
    {
        if (this.attackCoro != null)
            return;

        if (context.performed)
        {
            this.isChargeAttack = true;
        }
        else if (context.canceled)
        {
            if (this.isChargeAttack)
            {
                StartCoroutine(this.DisableCharge());
                StartCoroutine(this.StopSlide(0.2f));
                this.attackRange.SetActive(true);
                this.attackCoro = StartCoroutine(this.DisableAttack(0.5f));
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

    IEnumerator DisableCharge()
    {
        yield return new WaitForSeconds(0.5f);
        this.isChargeAttack = false;
    }

    IEnumerator DisableAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
        this.attackRange.SetActive(false);

        this.attackCoro = null;
    }
}
