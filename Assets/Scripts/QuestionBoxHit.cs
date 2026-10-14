using System.Collections;
using UnityEngine;

public class QuestionBoxHit : MonoBehaviour
{
    public enum RewardType
    {
        Auto,
        Coin,
        Mushroom
    }

    public GameObject coin;
    public Sprite disabledSprite;
    public RewardType rewardType = RewardType.Auto;

    private static int automaticBoxNumber;
    private bool used = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    void Awake()
    {
        if (rewardType == RewardType.Auto)
        {
            rewardType = automaticBoxNumber % 2 == 0
                ? RewardType.Coin
                : RewardType.Mushroom;
            automaticBoxNumber++;
        }
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        coin.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (used)
            return;

        if (collision.gameObject.TryGetComponent<PlayerMovement>(out PlayerMovement player))
        {
            bool playerIsBelow = player.transform.position.y < transform.position.y;
            bool hitBottomEdge = false;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                // In the box's collision callback, an upward normal means
                // Mario touched the bottom face of the box.
                if (contact.normal.y > 0.5f)
                {
                    hitBottomEdge = true;
                    break;
                }
            }

            if (playerIsBelow && hitBottomEdge)
            {
                used = true;
                StartCoroutine(HitBox());
            }
        }
    }

    IEnumerator HitBox()
    {
        Vector3 boxStart = transform.localPosition;

        transform.localPosition = boxStart + Vector3.up * 0.15f;
        yield return new WaitForSeconds(0.08f);

        transform.localPosition = boxStart;

        coin.SetActive(true);

        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();
        if (LabGameManager.Instance != null)
            LabGameManager.Instance.PlayPowerupSpawn();

        Vector3 coinStart = coin.transform.localPosition;

        coin.transform.localPosition = coinStart + Vector3.up * 0.8f;

        if (rewardType == RewardType.Coin)
        {
            if (LabGameManager.Instance != null)
            {
                LabGameManager.Instance.AddScore(1);
                LabGameManager.Instance.PlayPowerupCollect();
            }

            RuntimeSceneUI.ShowMessage("COIN POWER-UP +1 SCORE");
            yield return new WaitForSeconds(0.35f);
            coin.transform.localPosition = coinStart;
            coin.SetActive(false);
        }
        else
        {
            coin.name = "Mushroom Power-up";
            SpriteRenderer mushroomSprite = coin.GetComponentInChildren<SpriteRenderer>();
            if (mushroomSprite != null)
                mushroomSprite.color = new Color(1f, 0.2f, 0.2f, 1f);

            CircleCollider2D pickupCollider = coin.GetComponent<CircleCollider2D>();
            if (pickupCollider == null)
                pickupCollider = coin.AddComponent<CircleCollider2D>();
            pickupCollider.isTrigger = true;
            pickupCollider.radius = 0.12f;

            if (coin.GetComponent<PowerupPickup>() == null)
                coin.AddComponent<PowerupPickup>();

            RuntimeSceneUI.ShowMessage("MUSHROOM SPAWNED - COLLECT IT!");
        }

        spriteRenderer.sprite = disabledSprite;
    }
}
