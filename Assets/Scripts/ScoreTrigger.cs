using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    private bool canScore = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "mario_0" && canScore)
        {
            canScore = false;

            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player != null)
            {
                player.AddScore();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "mario_0")
        {
            CancelInvoke(nameof(ResetScore));
            Invoke(nameof(ResetScore), 0.2f);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.name == "mario_0")
        {
            CancelInvoke(nameof(ResetScore));
        }
    }

    private void ResetScore()
    {
        canScore = true;
    }
}