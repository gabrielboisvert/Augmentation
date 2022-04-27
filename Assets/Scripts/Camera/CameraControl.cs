using System;
using System.Collections;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject player;
    public Vector3 edgeDetectorSize;
    public float yAddition = 2;
    private float zAway;

    private BoxCollider collider;
    private bool canMoveX;
    private float oldX;

    private bool canMoveY;
    private float oldY;

    public float speed = 10;

    private bool isLerping = false;

    public Vector3 EdgeDetectorSize { get => edgeDetectorSize; set => edgeDetectorSize = value; }
    void Start()
    {
        this.zAway = this.transform.position.z;
        this.collider = this.GetComponent<BoxCollider>();

        this.canMoveX = true;
        this.canMoveY = true;

        this.transform.position = new Vector3(player.transform.position.x, transform.position.y, -this.zAway);
    }
    void Update()
    {
        if (!this.isLerping)
            this.ChangeCameraPos();
    }
    private void ChangeCameraPos()
    {
        // Camera position
        Vector3 newPos = new Vector3(this.oldX, this.oldY, this.zAway);
        if (this.canMoveY)
            newPos.y = this.oldY = player.transform.position.y + this.yAddition;
        if (this.canMoveX)
            newPos.x = this.oldX = player.transform.position.x;
        this.transform.position = newPos;

        // collider box
        float x = -(this.transform.position.x - player.transform.position.x);
        float y = -(this.transform.position.y - player.transform.position.y);
        this.collider.center = new Vector3(x, y, -this.zAway);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Top"))
            this.canMoveY = false;
        else if (other.gameObject.CompareTag("Edge"))
            this.canMoveX = false;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Top"))
            this.canMoveY = true;
        else if (other.gameObject.CompareTag("Edge"))
            this.canMoveX = true;
    }

    public IEnumerator StarLerp()
    {
        this.isLerping = true;

        float speed = Mathf.Abs(Vector3.Distance(player.transform.position, transform.position) * 2);

        while (true)
        {
            Vector3 playerPos = this.player.transform.position;
            playerPos.y += yAddition;
            playerPos.z = this.zAway;

            Vector3 newPos = Vector3.MoveTowards(this.transform.position, playerPos, speed * Time.deltaTime);
            this.transform.position = newPos;

            if (Vector3.Distance(transform.position, playerPos) < 0.001f)
                break;

            yield return null;
        }

        this.isLerping = false;
    }
}