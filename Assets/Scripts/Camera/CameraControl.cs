using System;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    struct Displacement
    {
        public bool canMove;
        public Vector3 normal;

        public Displacement(Vector3 normal)
        {
            this.canMove = true;
            this.normal = normal;
        }
    }
    struct Transform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Transform(UnityEngine.Transform currentTrans, UnityEngine.Transform targetTrans)
        {
            this.position = currentTrans.position - targetTrans.position;
            this.rotation = currentTrans.rotation;
            this.scale = currentTrans.localScale;
        }
    }
    struct LerpDisplacement
    {
        public float x;
        public float y;

        public LerpDisplacement(float x = 0, float y = 0)
        {
            this.x = 0;
            this.y = 0;
        }
    }
    enum Side
    {
        TOP,
        RIGHT,
        DOWN,
        LEFT
    }
    struct PlayerData
    {
        private Rigidbody playerBody;
        public NinjaControlle controller;

        public Vector2 velocity;
        public Vector2 joystick;
        public int orientation;
        public int orientationTop;
        public float maxVelocityX;
        public float maxVelocityY;

        public PlayerData(GameObject player)
        {
            this.playerBody = player.GetComponent<Rigidbody>();
            this.controller = player.GetComponent<NinjaControlle>();
            this.velocity = Vector2.zero;
            this.joystick = Vector2.zero;
            this.orientation = 0;
            this.orientationTop = 0;
            this.maxVelocityX = this.controller.maxMovementSpeed;
            this.maxVelocityY = this.controller.maxJumpSpeed;
        }
        public void update()
        {
            this.velocity = this.playerBody.velocity;
            this.orientation = (int)this.controller.orientation;
            this.orientationTop = this.playerBody.velocity.y < 0 ? -1 : 1;
            this.joystick = this.controller.inputMove;
        }
    }
    struct LastHit
    {
        public Vector3 xNormal;
        public Vector3 yNormal;
    }
    public GameObject player;
    public float maxLerpX = 5;
    public float maxLerpY = 5;
    public float lerpSpeedX = 3;
    public float lerpSpeedy = 1;

    private Camera cam;
    private Transform startedTransf;
    private BoxCollider m_collider;
    private PlayerData playerData;
    private Displacement[] wallSide = new Displacement[4];
    private LerpDisplacement lerpDisplacement;
    private LastHit lastHit;
    private float respawnSpeed;
    private bool firstInit = true;
    private bool isRespawn;

    public void Start()
    {
        this.cam = this.GetComponent<Camera>();
        this.m_collider = this.gameObject.GetComponent<BoxCollider>();

        this.wallSide[(int)Side.TOP] = new Displacement(Vector3.up);
        this.wallSide[(int)Side.RIGHT] = new Displacement(Vector3.right);
        this.wallSide[(int)Side.DOWN] = new Displacement(Vector3.down);
        this.wallSide[(int)Side.LEFT] = new Displacement(Vector3.left);
        
        GameManager.Spawner.OnRespawn += new Spawner.SpawnEvent(delegate (GameObject player) {
            this.player = player;
            this.playerData = new PlayerData(player);
            this.lerpDisplacement = new LerpDisplacement();

            this.playerData.controller.OnDead += new NinjaControlle.DeathEvent(delegate () { this.player = null; });

            if (this.firstInit)
            {
                this.startedTransf = new Transform(this.gameObject.transform, this.player.transform);
                float y = 2.0f * this.startedTransf.position.z * Mathf.Tan(this.cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                float x = y * this.cam.aspect;
                this.m_collider.size = new Vector3(x, y, this.m_collider.size.z);

                this.firstInit = false;
                return;
            }

            this.isRespawn = true;

            this.respawnSpeed = Mathf.Abs(Vector3.Distance(player.transform.position, this.transform.position) * 2);
        });
    }
    public void Update()
    {
        if (player == null)
            return;

        this.playerData.update();

        if (!this.isRespawn)
            this.ChangeCameraPos();
        else
            this.RespawnLerp();
    }
    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    private void ChangeCameraPos()
    {
        Vector3 camPos = this.transform.position;
        this.m_collider.center = new Vector3(this.player.transform.position.x - this.transform.position.x, this.player.transform.position.y - this.transform.position.y, -this.startedTransf.position.z);
        if (this.wallSide[(int)Side.LEFT].canMove && this.wallSide[(int)Side.RIGHT].canMove)
        {
            float lerpVelocity = this.Remap(Mathf.Abs(this.playerData.velocity.x), 0, this.playerData.maxVelocityX, 0, 1);
            float destination = Mathf.Lerp(0, this.maxLerpX * this.playerData.orientation, lerpVelocity);
            this.lerpDisplacement.x = Mathf.Lerp(this.lerpDisplacement.x, destination, this.lerpSpeedX * Time.deltaTime);

            camPos.x = this.player.transform.position.x + this.startedTransf.position.x + this.lerpDisplacement.x;
        }

        if (this.wallSide[(int)Side.TOP].canMove && this.wallSide[(int)Side.DOWN].canMove)
        {
            float lerpVelocity = this.Remap(Mathf.Abs(this.playerData.velocity.y), 0, this.playerData.maxVelocityY, 0, 1);
            float destination = Mathf.Lerp(0, this.maxLerpY * this.playerData.orientationTop, lerpVelocity);
            if (destination > 0.05f || destination < -0.05f)
                this.lerpDisplacement.y = Mathf.Lerp(this.lerpDisplacement.y, destination, this.lerpSpeedy * Time.deltaTime);
            else
                this.lerpDisplacement.y = Mathf.Lerp(this.lerpDisplacement.y, 0, this.lerpSpeedy * Time.deltaTime);

            camPos.y = this.player.transform.position.y + this.startedTransf.position.y + this.lerpDisplacement.y;
        }

        this.transform.position = camPos;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Edge"))
        {
            Vector3 collisionPoint = other.ClosestPoint(this.transform.position);
            Vector3 normal = -(transform.position - collisionPoint);
            normal.x = Mathf.Clamp(normal.x, -1, 1);
            normal.y = Mathf.Clamp(normal.y, -1, 1);
            normal.z = 0;

            this.wallSide[(int)Side.RIGHT].canMove = normal != Vector3.right;
            this.wallSide[(int)Side.LEFT].canMove = normal != Vector3.left;

            this.lastHit.xNormal = normal;
        }
        else if (other.gameObject.CompareTag("Top"))
        {
            Vector3 collisionPoint = other.ClosestPoint(this.transform.position);
            Vector3 normal = -(transform.position - collisionPoint);
            normal.x = Mathf.Clamp(normal.x, -1, 1);
            normal.y = Mathf.Clamp(normal.y, -1, 1);
            normal.z = 0;

            this.wallSide[(int)Side.TOP].canMove = normal != Vector3.up;
            this.wallSide[(int)Side.DOWN].canMove = normal != Vector3.down;

            this.lastHit.yNormal = normal;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Edge"))
        {
            this.wallSide[(int)Side.RIGHT].canMove = true;
            this.wallSide[(int)Side.LEFT].canMove = true;

            this.lastHit.xNormal = Vector3.zero;
        }
        else if (other.gameObject.CompareTag("Top"))
        {
            this.wallSide[(int)Side.TOP].canMove = true;
            this.wallSide[(int)Side.DOWN].canMove = true;

            this.lastHit.yNormal = Vector3.zero;
        }
    }
    private void RespawnLerp()
    {
        Vector3 playerPos = this.player.transform.position;
        playerPos.y += this.startedTransf.position.y;
        playerPos.z = this.startedTransf.position.z;

        float dist = Vector2.Distance(this.transform.position, playerPos);
        if (dist < 0.001f)
        {
            this.isRespawn = false;
            return;
        }

        Vector3 newPos = Vector3.MoveTowards(this.transform.position, playerPos, this.respawnSpeed * Time.deltaTime);
        newPos.z = this.transform.position.z;
        this.transform.position = newPos;

        this.m_collider.center = new Vector3(this.player.transform.position.x - this.transform.position.x, this.player.transform.position.y - this.transform.position.y, -this.startedTransf.position.z);
    }
}