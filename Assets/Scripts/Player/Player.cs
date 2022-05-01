using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public delegate void DeathEvent();
    public event DeathEvent OnDead;

    public float movementSpeed = 50;
    public float maxMovementSpeed = 5;
    public float friction = 20;

    public float jumpForce = 15;
    public float bunnyHopForce = 5;
    public float maxJumpSpeed = 10;
    public float mass = 15;

    public float rotationSpeed = 1400;

    protected Rigidbody m_body;
    protected Animation anim;
    protected AudioSource m_audio;
    protected AnimationClip currentAnim;
    protected bool isDead = false;
    protected int joystickSide;
    protected int orientation = -1;
    protected bool canJump = true;
    protected bool inTheAir = false;
    protected Coroutine rotationAnimeCoro = null;

    public void Start()
    {
        this.m_body = this.GetComponent<Rigidbody>();
        this.anim = this.GetComponent<Animation>();
        this.m_audio = this.GetComponent<AudioSource>();
    }
    public virtual void FixedUpdate()
    {
        if (this.isDead)
            return;

        this.m_body.velocity = new Vector3(Mathf.Clamp(this.m_body.velocity.x, -this.maxMovementSpeed, this.maxMovementSpeed), Mathf.Clamp(this.m_body.velocity.y, -this.maxJumpSpeed, this.maxJumpSpeed), 0);
        this.m_body.AddRelativeForce(-this.transform.right * (this.joystickSide * this.movementSpeed * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }
    protected void updateGravity()
    {
        if (this.isDead)
            return;

        float xVelocity = this.m_body.velocity.x;
        if (xVelocity != 0)
        {
            if (this.joystickSide == 0 && !this.inTheAir)
                xVelocity = Mathf.MoveTowards(xVelocity, 0, this.friction * Time.deltaTime);
        }
        this.m_body.velocity = new Vector3(xVelocity, this.m_body.velocity.y - (this.mass * Time.deltaTime), this.m_body.velocity.z);
    }
    protected virtual void updateAnimationState() { throw new NotImplementedException(""); }
    protected virtual void FireDeath()
    {
        this.OnDead.Invoke();
    }
    public void Move(InputAction.CallbackContext context)
    {
        if (this.isDead)
            return;

        Vector2 direct = context.ReadValue<Vector2>();
        direct.x = Mathf.RoundToInt(direct.x);

        this.joystickSide = (int)direct.x;

        if (this.orientation != direct.x && direct.x != 0)
        {
            this.orientation = (int)direct.x;
            if (this.rotationAnimeCoro == null)
                this.rotationAnimeCoro = StartCoroutine(this.RotateAnimation());
        }
    }
    protected IEnumerator RotateAnimation()
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
    public void Pause(InputAction.CallbackContext context)
    {
        GameManager.Spawner.GetComponent<InGameMenu>().Pause(context);
    }
    public int Orientation { get => orientation; set => orientation = value; }
    public virtual void OnCollisionEnter(Collision collision) { throw new NotImplementedException(""); }
    public virtual void Kill()
    {
        StartCoroutine(this.Dead());
    }
    public virtual IEnumerator Dead() { throw new NotImplementedException(""); }
}