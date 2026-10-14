using UnityEngine;
using System;

public class GoombaMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float leftLimit = -3f;
    public float rightLimit = 3f;

    public event Action Stomped;

    private Rigidbody2D rb;
    private int direction = -1;
    private bool isStomped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isStomped)
            return;

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );

        if (transform.position.x <= leftLimit)
        {
            direction = 1;
        }
        else if (transform.position.x >= rightLimit)
        {
            direction = -1;
        }
    }

    public void Stomp()
    {
        if (isStomped)
            return;

        isStomped = true;

        rb.linearVelocity = Vector2.zero;

        Stomped?.Invoke();

        if (LabGameManager.Instance != null)
            LabGameManager.Instance.PlayGoombaStomp();

        transform.localScale = new Vector3(
            transform.localScale.x,
            transform.localScale.y * 0.5f,
            transform.localScale.z
        );

        Destroy(gameObject, 0.3f);
    }
}
