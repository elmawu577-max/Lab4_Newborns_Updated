using System.Collections;
using UnityEngine;

public class BrickHit : MonoBehaviour
{
    private bool isBouncing = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "mario_0" && !isBouncing)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    StartCoroutine(Bounce());
                    break;
                }
            }
        }
    }

    IEnumerator Bounce()
    {
        isBouncing = true;

        Vector3 startPosition = transform.localPosition;

        transform.localPosition = startPosition + Vector3.up * 0.15f;
        yield return new WaitForSeconds(0.08f);

        transform.localPosition = startPosition;

        isBouncing = false;
    }
}
