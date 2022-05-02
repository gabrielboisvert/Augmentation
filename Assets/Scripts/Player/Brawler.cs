using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Brawler : Player
{
    enum ANIMATION_STATE
    {
        IDLE,
        RUN,
        JUMP,
        ATTACK,
        BLOCK_ACTIF,
        BLOCK_INACTIF,
        CHARGE,
        PUNCH,
        DEATH,
    }

    public string[] animationStr;
    public GameObject attackRange;
    public float dashSpeed = 400;
    public float shieldDuration = 3;

    private bool isAttacking = false;
    private bool isChargeAttack = false;
    private float chargeTimer;
    private float chargeMinTime;
    private Coroutine attackCoro;
    private Coroutine shieldCoro;
    private bool hasShield = false;
    private float shieldTimer;



    public AudioClip[] clip;
    public AudioSource footstep;


    public new void Start()
    {
        base.Start();
        this.transform.rotation = Quaternion.Euler(0, -90, 0);

        anim.clip = this.currentAnim = anim.GetClip(animationStr[(int)ANIMATION_STATE.IDLE]);
        anim.Play();

        this.chargeMinTime = (anim.GetClip(animationStr[(int)ANIMATION_STATE.CHARGE]).length / 3) * 2;
    }
    public void Update()
    {
        if (this.isDead)
            return;

        this.UpdateGravity();
        this.UpdateAnimationState();

        if (this.hasShield && Time.time - this.shieldTimer > this.shieldDuration && this.shieldCoro == null)
            this.shieldCoro = StartCoroutine(this.StopShield());
    }
    protected override void UpdateAnimationState() 
    {
        if (this.joystickSide != 0)
        {
            if (this.currentAnim == this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.RUN]) || this.isAttacking || !this.canJump || this.hasShield || this.shieldCoro != null)
                return;

            this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.RUN]);
            this.anim.Play();

            //this.footstep.Play();
        }
        else if (Mathf.Abs(this.m_body.velocity.x) < 0.001f)
        {
            if (this.currentAnim == this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.IDLE]) || this.isAttacking || !this.canJump || this.hasShield || this.shieldCoro != null)
                return;

            this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.IDLE]);
            this.anim.Play();

            //this.footstep.Stop();
        }
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (this.isDead || !context.started || !this.canJump)
            return;

        this.canJump = false;
        this.inTheAir = true;

        this.m_body.velocity = new Vector3(this.m_body.velocity.x, 0, 0);
        this.m_body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);


        if (!this.isAttacking && !this.hasShield)
        {
            this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.JUMP]);
            this.anim.Play();
        }

        //this.m_audio.PlayOneShot(this.clip[3]);
        //this.footstep.Stop();
    }
    public override void OnCollisionEnter(Collision collision)
    {
        if (this.isDead)
            return;

        if (collision.gameObject.CompareTag("AI"))
        {
            if (collision.GetContact(0).normal == Vector3.up)
            {
                Vector3 jump = new Vector3(-this.transform.right.x * this.orientation * this.bunnyHopForce, this.jumpForce, 0);
                this.m_body.velocity = new Vector3(this.m_body.velocity.x, 0, 0);
                this.m_body.AddForce(jump, ForceMode.Impulse);
                //this.inTheAir = false;

                //this.m_audio.PlayOneShot(this.clip[3]);
                //this.footstep.Stop();

                if (!this.hasShield && !this.isAttacking)
                {
                    this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.JUMP]);
                    this.anim.Play();
                }
            }
        }
        else if (collision.GetContact(0).normal == Vector3.up)
        {
            this.canJump = true;
            this.inTheAir = false;
            //this.m_audio.PlayOneShot(this.clip[2]);
            //this.footstep.Stop();
        }
    }
    public override void Kill()
    {
        if (this.hasShield)
            return;

        StartCoroutine(this.Dead());
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
    public void Shield(InputAction.CallbackContext context)
    {
        if (this.isDead || this.isAttacking)
            return;

        if (context.started && this.shieldCoro == null)
        {
            this.shieldTimer = Time.time;
            this.hasShield = true;

            this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.BLOCK_ACTIF]);
            this.anim.Play();

            //this.m_audio.PlayOneShot(this.clip[6]);
            //this.footstep.Stop();
        }
        else if (context.canceled && this.shieldCoro == null && this.hasShield)
            this.shieldCoro = StartCoroutine(this.StopShield());
    }
    public IEnumerator StopShield()
    {
        this.hasShield = false;
        this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.BLOCK_INACTIF]);
        this.anim.Play();
        yield return new WaitForSeconds(this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.BLOCK_INACTIF]).length);
        this.shieldCoro = null;
    }
    public void MeleAttack(InputAction.CallbackContext context)
    {
        if (this.isDead || this.attackCoro != null || context.started || context.performed || this.isChargeAttack || this.hasShield)
            return;

        this.attackCoro = StartCoroutine(this.DisableAttack());
    }
    public IEnumerator DisableAttack()
    {
        this.isAttacking = true;
        this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.ATTACK]);
        this.anim.Play();

        //this.m_audio.PlayOneShot(this.clip[1]);
        //this.footstep.Stop();
        yield return new WaitForSeconds(this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.ATTACK]).length / 4);
        this.attackRange.SetActive(true);
        yield return new WaitForSeconds((this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.ATTACK]).length / 4) * 2);
        this.isAttacking = false;
        this.attackRange.SetActive(false);
        this.attackCoro = null;
    }
    public void ChargeAttack(InputAction.CallbackContext context)
    {
        if (this.isDead || this.attackCoro != null || this.hasShield)
            return;

        if (context.performed)
        {
            this.isAttacking = true;
            this.isChargeAttack = true;
            this.chargeTimer = Time.time;

            this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.CHARGE]);
            this.anim.Play();
            //this.m_audio.PlayOneShot(this.clip[4]);
            //this.footstep.Stop();
        }
        else if (context.canceled && this.isChargeAttack)
        {
            if (Time.time - this.chargeTimer < this.chargeMinTime)
            {
                this.isAttacking = false;
                this.isChargeAttack = false;
                //this.m_audio.Stop();

                this.attackCoro = StartCoroutine(this.DisableAttack());
                return;
            }

            StartCoroutine(this.StopSlide((this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.PUNCH]).length / 4) * 3));
            this.attackCoro = StartCoroutine(this.DisablePunch());
        }
    }
    public IEnumerator DisablePunch()
    {
        //this.m_audio.PlayOneShot(this.clip[0]);
        //this.footstep.Stop();

        this.anim.clip = this.currentAnim = this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.PUNCH]);
        this.anim.Play();
        this.attackRange.SetActive(true);
        yield return new WaitForSeconds((this.anim.GetClip(this.animationStr[(int)ANIMATION_STATE.PUNCH]).length));

        this.isAttacking = false;
        this.isChargeAttack = false;
        this.attackRange.SetActive(false);
        this.attackCoro = null;
    }
    IEnumerator StopSlide(float duration)
    {
        float elapse = 0;
        while (elapse < duration)
        {
            elapse += Time.deltaTime;
            this.m_body.AddForce(new Vector3(this.orientation * this.dashSpeed * Time.deltaTime, 0, 0), ForceMode.Impulse);

            yield return null;
        }
    }
    public bool IsChargedAttack()
    {
        return this.isChargeAttack;
    }
}