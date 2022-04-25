using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller: MonoBehaviour
{
    private float sideDirection;
    private Rigidbody body;
    public float speed = 10;
    public float jumpForce = 10;

    // Start is called before the first frame update
    void Start()
    {
        this.body = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        this.body.AddForce(new Vector3(this.sideDirection * Time.deltaTime * speed, 0, 0), ForceMode.VelocityChange);
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 direct = context.ReadValue<Vector2>();

        if (direct.x < 0)
            this.sideDirection = -1;
        else if (direct.x > 0)
            this.sideDirection = 1;
        else
            this.sideDirection = 0;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        this.body.AddForce(new Vector3(0, this.jumpForce, 0), ForceMode.Impulse);
    }
}
