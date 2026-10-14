using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 50f;
    public float stompBounceForce = 25f;

    public GameObject gameOverPanel;
    public TMP_Text scoreText;
    public AudioSource marioAudio;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool isGameOver = false;

    private float previousMove = 0f;
    private float moveInput = 0f;
    private float baseMoveSpeed;
    private Coroutine mushroomRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseMoveSpeed = moveSpeed;

        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

        UpdateScore();

        if (LabGameManager.Instance != null)
            LabGameManager.Instance.ScoreChanged += OnScoreChanged;

        GoombaMovement[] goombas =
            FindObjectsByType<GoombaMovement>(FindObjectsSortMode.None);

        foreach (GoombaMovement goomba in goombas)
        {
            goomba.Stomped += AddScore;
        }
    }

    void OnDestroy()
    {
        if (LabGameManager.Instance != null)
            LabGameManager.Instance.ScoreChanged -= OnScoreChanged;

        GoombaMovement[] goombas =
            FindObjectsByType<GoombaMovement>(FindObjectsSortMode.None);

        foreach (GoombaMovement goomba in goombas)
        {
            goomba.Stomped -= AddScore;
        }
    }

    void Update()
    {
        if (isGameOver)
            return;

        float move = moveInput;

        bool isSkidding =
            isGrounded &&
            move != 0 &&
            previousMove != 0 &&
            Mathf.Sign(move) != Mathf.Sign(previousMove);

        animator.SetBool("IsSkidding", isSkidding);
        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetBool("IsJumping", !isGrounded);

        rb.linearVelocity = new Vector2(
            move * moveSpeed,
            rb.linearVelocity.y
        );

        if (move < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (move > 0)
        {
            spriteRenderer.flipX = false;
        }

        if (move != 0)
        {
            previousMove = move;
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = input.x;
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (isGameOver || !isGrounded)
            return;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        isGrounded = false;
        animator.SetBool("IsJumping", true);

        if (marioAudio != null)
        {
            marioAudio.PlayOneShot(marioAudio.clip);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlatform(collision.gameObject))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    animator.SetBool("IsJumping", false);
                    break;
                }
            }
        }

        if (collision.gameObject.TryGetComponent<GoombaMovement>(
            out GoombaMovement goomba))
        {
            bool stomped = false;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    stomped = true;
                    break;
                }
            }

            if (stomped)
            {
                goomba.Stomp();

                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    stompBounceForce
                );

                isGrounded = false;
                animator.SetBool("IsJumping", true);
            }
            else
            {
                GameOver();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsPlatform(collision.gameObject))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    return;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsPlatform(collision.gameObject))
        {
            isGrounded = false;
            animator.SetBool("IsJumping", true);
        }
    }

    void GameOver()
    {
        isGameOver = true;

        rb.linearVelocity = Vector2.zero;

        gameOverPanel.SetActive(true);

        if (LabGameManager.Instance != null)
            LabGameManager.Instance.PlayGameOver();

        Time.timeScale = 0f;
    }

    bool IsPlatform(GameObject obj)
    {
        return obj.name == "Ground" ||
               obj.name.StartsWith("Brick") ||
               obj.name.StartsWith("QuestionBox");
    }

    public void AddScore()
    {
        if (LabGameManager.Instance != null)
            LabGameManager.Instance.AddScore(1);
    }

    void UpdateScore()
    {
        int currentScore = LabGameManager.Instance == null
            ? 0
            : LabGameManager.Instance.State.currentScore;

        int highScore = LabGameManager.Instance == null
            ? 0
            : LabGameManager.Instance.State.highScore;

        scoreText.text = "SCORE: " + currentScore + "  TOP: " + highScore;
    }

    private void OnScoreChanged(int currentScore, int highScore)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + currentScore + "  TOP: " + highScore;
    }

    public void ApplyMushroom()
    {
        if (mushroomRoutine != null)
            StopCoroutine(mushroomRoutine);

        mushroomRoutine = StartCoroutine(MushroomBoost());
    }

    private IEnumerator MushroomBoost()
    {
        float multiplier = LabGameManager.Instance == null
            ? 1.5f
            : LabGameManager.Instance.State.mushroomSpeedMultiplier;
        float duration = LabGameManager.Instance == null
            ? 6f
            : LabGameManager.Instance.State.mushroomDuration;

        moveSpeed = baseMoveSpeed * multiplier;

        if (LabGameManager.Instance != null)
            LabGameManager.Instance.PlayPowerupCollect();

        RuntimeSceneUI.ShowMessage("MUSHROOM COLLECTED - SPEED BOOST!");
        yield return new WaitForSeconds(duration);
        moveSpeed = baseMoveSpeed;
        RuntimeSceneUI.ShowMessage("MUSHROOM BOOST ENDED");
    }

    public void RestartGame()
    {
        if (LabGameManager.Instance != null)
            LabGameManager.Instance.RestartCurrentScene();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
