using UnityEngine;
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] protected float duration = 2;
    [SerializeField] protected Vector3 maxPos;
    protected Vector3 minimum;
    protected float startTime;
    protected bool starting = true;
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
            transform.position = Vector3.Lerp(this.minimum, this.minimum + this.MaxPos, t);
        else
            transform.position = Vector3.Lerp(this.minimum + this.MaxPos, this.minimum, t);

        if (Time.time - this.startTime > this.duration)
        {
            this.starting = !this.starting;
            this.startTime = Time.time;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.SetParent(this.transform);
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.SetParent(null);
    }
}