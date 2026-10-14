using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    public enum PickupKind
    {
        Mushroom
    }

    public PickupKind kind = PickupKind.Mushroom;
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null)
            return;

        collected = true;
        player.ApplyMushroom();
        gameObject.SetActive(false);
    }
}
