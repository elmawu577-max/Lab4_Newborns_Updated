using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Transform endLimit;

    private float offset;
    private float startX;
    private float endX;
    private float viewportHalfWidth;

    void Start()
    {
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));

        viewportHalfWidth = Mathf.Abs(bottomLeft.x - transform.position.x);
        offset = transform.position.x - player.position.x;
        startX = transform.position.x;
        endX = endLimit.position.x - viewportHalfWidth;
    }

    void Update()
    {
        float desiredX = player.position.x + offset;

        if (desiredX > startX && desiredX < endX)
        {
            transform.position = new Vector3(
                desiredX,
                transform.position.y,
                transform.position.z
            );
        }
    }
}