using UnityEngine;
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] protected float duration = 2;
    [SerializeField] protected Vector3 maxPos;
    protected Vector3 minimum;
    protected float startTime;
    protected bool starting = true;
    private GameObject player;
    private Vector3 offset;

    public Vector3 MaxPos { get => maxPos; set => maxPos = value; }
    public void Start()
    {
        this.minimum = this.transform.position;
        this.startTime = Time.time;
    }
    public void Update()
    {
        float t = (Time.time - this.startTime) / this.duration;

        if (this.starting)
            this.transform.position = Vector3.Lerp(this.minimum, this.minimum + this.MaxPos, t);
        else
            this.transform.position = Vector3.Lerp(this.minimum + this.MaxPos, this.minimum, t);

        if (this.player != null)
        {
            this.offset = this.transform.position - this.player.transform.position;
            this.player.transform.position = this.transform.position + this.offset;
        }

        if (Time.time - this.startTime > this.duration)
        {
            this.starting = !this.starting;
            this.startTime = Time.time;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            if (collision.GetContact(0).normal == Vector3.down)
            {
                this.player = collision.gameObject;
                this.offset = this.transform.position - collision.gameObject.transform.position;
            }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            this.player = null;
    }
}