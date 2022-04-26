using System;
using System.Collections;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 edgeDetectorSize;
    private float zAway;

    private BoxCollider collider;
    private bool canMoveX;
    private float oldX;

    private bool canMoveY;
    private float oldY;

    public float speed = 10;

    private Vector3 velocity = Vector3.zero;

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
        this.ChangeCameraPos();
    }
    private void ChangeCameraPos()
    {
        // Camera position
        Vector3 newPos = new Vector3(this.oldX, this.oldY, this.zAway);
        if (this.canMoveY)
            newPos.y = this.oldY = player.transform.position.y;
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
}