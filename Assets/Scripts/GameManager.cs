using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Animator")]
    public Animator animator;

    [Header("UI")]
    public TextMeshProUGUI gameStartText;
    public TextMeshProUGUI gameOverText;
    public Button retryButton;

    [Header("Tutorial")]
    public GameObject tutorialOverlay;

    [Header("Dark Background")]
    public GameObject darkBackground;

    [Header("Audio")]
    public AudioClip gameOverSound;

    [Range(0f, 1f)]
    public float gameOverVolume = 0.7f;

    private AudioSource audioSource;

    [Header("Score")]
    public float score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hiscoreText;

    // References
    private PlayerController player;
    private ObstacleGenerator spawner;

    [Header("Game Speed")]
    public float initialGameSpeed = 5f;
    public float gameSpeedIncrease = 0.1f;

    // Current speed
    public float gameSpeed { get; private set; }

    // ===== ENERGY SYSTEM =====
    private float currentMultiplier = 1f;
    private float targetMultiplier = 1f;

    [HideInInspector]
    public float spawnRateMultiplier = 1f;

    // Game state
    [Header("Game State")]
    public bool isGameStarted = false;
    public bool isGameOver = false;

    // ===== TUTORIAL =====
    private bool tutorialClosed = false;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        player = FindObjectOfType<PlayerController>();
        spawner = FindObjectOfType<ObstacleGenerator>();
        audioSource = GetComponent<AudioSource>();

        animator.SetBool("isGameStarted", false);

        gameSpeedIncrease = 0;

        spawner.gameObject.SetActive(false);

        isGameStarted = false;
        isGameOver = false;

        // ===== SHOW TUTORIAL =====

        tutorialClosed = false;

        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(true);
        }

        if (darkBackground != null)
        {
            darkBackground.SetActive(true);
        }

        if (gameStartText != null)
        {
            gameStartText.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // NEW GAME
    // =====================================================

    public void NewGame()
    {
        // Reset speed system
        gameSpeedIncrease = 0.1f;

        currentMultiplier = 1f;
        targetMultiplier = 1f;

        // Reset player
        player.gameObject.SetActive(false);

        // Destroy all obstacles
        ObstacleController[] obstacles =
            FindObjectsOfType<ObstacleController>();

        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        // Reset score
        score = 0f;

        // Reset speed
        gameSpeed = initialGameSpeed;

        // Enable manager
        enabled = true;

        // Enable gameplay
        player.gameObject.SetActive(true);
        spawner.gameObject.SetActive(true);

        // Hide UI
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        // ===== HIDE DARK BG =====

        if (darkBackground != null)
        {
            darkBackground.SetActive(false);
        }

        // Reset states
        isGameOver = false;
        isGameStarted = true;

        animator.SetBool("isGameStarted", true);
        animator.SetBool("isGameOver", false);
    }

    // =====================================================
    // RETURN TO TUTORIAL
    // =====================================================

    private void ReturnToTutorial()
    {
        isGameStarted = false;
        isGameOver = false;

        tutorialClosed = false;

        gameSpeed = 0f;

        // =====================================
        // DESTROY ALL OBSTACLES
        // =====================================

        ObstacleController[] obstacles =
            FindObjectsOfType<ObstacleController>();

        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        // =====================================
        // UI
        // =====================================

        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(true);
        }

        if (darkBackground != null)
        {
            darkBackground.SetActive(true);
        }

        if (gameStartText != null)
        {
            gameStartText.gameObject.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
        }

        // =====================================
        // STOP GAMEPLAY
        // =====================================

        if (spawner != null)
        {
            spawner.gameObject.SetActive(false);
        }

        animator.SetBool("isGameStarted", false);
        animator.SetBool("isGameOver", false);
    }

    // =====================================================
    // START GAME
    // =====================================================

    public void GameStart()
    {
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        UpdateHiscore();

        // =========================================
        // CLOSE TUTORIAL FIRST
        // =========================================

        if (!tutorialClosed)
        {
            if (Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space))
            {
                tutorialClosed = true;

                if (tutorialOverlay != null)
                {
                    tutorialOverlay.SetActive(false);
                }

                // ===== HIDE DARKGROUND =====

                if (darkBackground != null)
                {
                    darkBackground.SetActive(false);
                }

                if (gameStartText != null)
                {
                    gameStartText.gameObject.SetActive(true);
                }
            }

            return;
        }

        // =========================================
        // START GAME
        // =========================================

        if ((Input.GetKeyDown(KeyCode.Space) ||
             Input.GetMouseButtonDown(0))
            && !isGameStarted)
        {
            isGameStarted = true;

            NewGame();

            if (gameStartText != null)
            {
                gameStartText.gameObject.SetActive(false);
            }

            animator.SetBool("isGameStarted", true);
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        // =====================================
        // GAME OVER INPUT
        // =====================================

        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetMouseButtonDown(0))
            {
                ReturnToTutorial();
            }

            return;
        }

        // =====================================
        // NORMAL GAME LOOP
        // =====================================

        if (!isGameOver)
        {
            GameStart();

            // ===== ONLY RUN AFTER START =====

            if (!isGameStarted)
            {
                return;
            }

            // ===== SMOOTH ENERGY SPEED =====

            currentMultiplier = Mathf.Lerp(
                currentMultiplier,
                targetMultiplier,
                2f * Time.deltaTime
            );

            // ===== GAME SPEED =====

            gameSpeed +=
                gameSpeedIncrease *
                currentMultiplier *
                Time.deltaTime;

            // ===== SCORE =====

            score +=
                gameSpeed *
                currentMultiplier *
                Time.deltaTime;

            scoreText.text =
                Mathf.FloorToInt(score).ToString("D5");
        }
    }

    // =====================================================
    // ENERGY MODE
    // =====================================================

    public void SetEnergyMode(bool active)
    {
        if (active)
        {
            currentMultiplier = 2f;
            targetMultiplier = 2f;
        }
        else
        {
            currentMultiplier = 1f;
            targetMultiplier = 1f;
        }
    }

    public void StartEnergyEnding()
    {
        targetMultiplier = 1f;
    }

    // =====================================================
    // STRESS MODE
    // =====================================================

    public void SetStressMode(bool active)
    {
        if (active)
        {
            spawnRateMultiplier = 2f;
        }
        else
        {
            spawnRateMultiplier = 1f;
        }
    }

    // =====================================================
    // HISCORE
    // =====================================================

    private void UpdateHiscore()
    {
        float hiscore =
            PlayerPrefs.GetFloat("hiscore", 0);

        hiscoreText.text =
            Mathf.FloorToInt(hiscore).ToString("D5");

        if (isGameOver)
        {
            if (score > hiscore)
            {
                hiscore = score;

                PlayerPrefs.SetFloat(
                    "hiscore",
                    hiscore
                );
            }

            hiscoreText.text =
                Mathf.FloorToInt(hiscore).ToString("D5");
        }
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    public void GameOver()
    {
        gameSpeed = 0f;

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(
                0.35f,
                0.18f
            );
        }

        if (gameOverSound != null &&
            audioSource != null)
        {
            audioSource.PlayOneShot(
                gameOverSound,
                gameOverVolume
            );
        }

        animator.SetBool("isGameOver", true);

        spawner.gameObject.SetActive(false);

        gameOverText.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);

        isGameOver = true;

        UpdateHiscore();
    }

    // =====================================================
    // FINAL SPEED
    // =====================================================

    public float GetFinalGameSpeed()
    {
        return gameSpeed * currentMultiplier;
    }
}