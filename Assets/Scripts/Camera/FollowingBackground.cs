using UnityEngine;

public class FollowingBackground : MonoBehaviour
{
    [SerializeField] private float parralaxSpeed = 2f;
    [SerializeField] private GameObject player;
    private Material mat;

    // Start is called before the first frame update
    void Start()
    {
        this.mat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        mat.mainTextureOffset = new Vector2(this.player.transform.position.x / this.transform.localScale.x / this.parralaxSpeed, 0);
        this.transform.position = new Vector3(this.player.transform.position.x, this.transform.position.y, this.transform.position.z);
    }
}
