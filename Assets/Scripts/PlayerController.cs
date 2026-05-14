using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private CharacterController character;
    private AudioSource audioSource;

    // =====================================================
    // HIT EFFECT
    // =====================================================

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Hit Effect")]
    public float hitFlashDuration = 0.8f;
    public Color hitFlashColor = Color.red;

    [Header("Heal Effect")]
    public float healFlashDuration = 0.25f;
    public Color healFlashColor = Color.green;

    // =====================================================
    // MOVEMENT
    // =====================================================

    private Vector3 direction;

    [Header("Movement")]
    public float gravity = 9.81f * 2f;
    public float jumpForce = 8f;

    // =====================================================
    // JUMP ASSIST
    // =====================================================

    [Header("Jump Assist")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    private float coyoteCounter;
    private float jumpBufferCounter;

    // =====================================================
    // ANIMATION FIX
    // =====================================================

    private bool stableGrounded;

    // =====================================================
    // HEALTH
    // =====================================================

    [Header("Health")]
    public int maxHealth = 3;
    public int currentHealth = 1;
    public HeartUI heartUI;

    // =====================================================
    // SHIELD
    // =====================================================

    [Header("Shield")]
    public bool hasShield = false;
    public float shieldDuration = 10f;
    public GameObject shieldIcon;

    // =====================================================
    // ENERGY
    // =====================================================

    [Header("Energy")]
    public bool hasEnergy = false;
    public float energyDuration = 10f;
    public GameObject energyIcon;

    [Header("Energy Warning")]
    public float warningTime = 3f;

    private bool isEnergyEnding = false;

    // =====================================================
    // STRESS
    // =====================================================

    [Header("Stress")]
    public bool hasStress = false;
    public float stressDuration = 10f;
    public GameObject stressIcon;

    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip hitSound;
    public AudioClip pickupSound;
    public AudioClip dieSound;

    [Range(0f, 1f)] public float jumpVolume = 0.3f;
    [Range(0f, 1f)] public float hitVolume = 0.6f;
    [Range(0f, 1f)] public float pickupVolume = 0.5f;
    [Range(0f, 1f)] public float dieVolume = 1f;

    private Coroutine shieldCoroutine;
    private Coroutine energyCoroutine;
    private Coroutine stressCoroutine;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void OnEnable()
    {
        direction = Vector3.zero;

        currentHealth = 1;

        hasShield = false;
        hasEnergy = false;
        hasStress = false;

        isEnergyEnding = false;

        coyoteCounter = 0f;
        jumpBufferCounter = 0f;

        stableGrounded = true;

        if (shieldIcon != null)
        {
            shieldIcon.SetActive(false);
        }

        if (energyIcon != null)
        {
            energyIcon.SetActive(false);
        }

        if (stressIcon != null)
        {
            stressIcon.SetActive(false);
        }

        if (animator != null)
        {
            animator.SetBool("isGrounded", true);
            animator.SetBool("hasEnergy", false);
        }

        UpdateHeartUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
            return;
        }

        if (GameManager.Instance.isGameOver)
        {
            return;
        }

        UpdateJumpTimers();

        HandleJump();

        direction.y -= gravity * Time.deltaTime;

        character.Move(direction * Time.deltaTime);

        UpdateAnimator();
    }

    // =====================================================
    // ANIMATOR
    // =====================================================

    private void UpdateAnimator()
    {
        if (animator == null) return;

        bool grounded =
            character.isGrounded &&
            direction.y <= 0.1f;

        animator.SetBool("isGrounded", grounded);
    }

    // =====================================================
    // JUMP SYSTEM
    // =====================================================

    private void UpdateJumpTimers()
    {
        // ===== COYOTE TIME =====
        if (character.isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // ===== JUMP BUFFER =====
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        // ===== LANDING =====
        if (character.isGrounded && direction.y < 0f)
        {
            direction.y = -2f;
        }

        // ===== BUFFERED JUMP =====
        if (jumpBufferCounter > 0f &&
            coyoteCounter > 0f)
        {
            PlaySound(jumpSound, jumpVolume);

            direction.y = jumpForce;

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    // =====================================================
    // ITEM PICKUP
    // =====================================================

    public void CollectItem(ItemType itemType)
    {
        PlaySound(pickupSound, pickupVolume);

        switch (itemType)
        {
            case ItemType.Heart:
                AddHealth();
                break;

            case ItemType.Shield:
                ActivateShield();
                break;

            case ItemType.Energy:
                ActivateEnergy();
                break;

            case ItemType.Stress:
                ActivateStress();
                break;
        }
    }

    private void AddHealth()
    {
        currentHealth++;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHeartUI();

        StartCoroutine(HealFlashCoroutine());

        Debug.Log("Health: " + currentHealth);
    }

    // =====================================================
    // COLLISION
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;

        HandleObstacleCollision(other.gameObject);
    }

    private void HandleObstacleCollision(GameObject obstacle)
    {
        // =========================================
        // ENERGY MODE
        // =========================================

        if (hasEnergy)
        {
            Destroy(obstacle);
            return;
        }

        // =========================================
        // SHIELD
        // =========================================

        if (hasShield)
        {
            Destroy(obstacle);
            BreakShield();
            return;
        }

        // =========================================
        // DAMAGE
        // =========================================

        TakeDamage();

        if (currentHealth > 0)
        {
            Destroy(obstacle);

            StartCoroutine(HitFlashCoroutine());
        }
    }

    private void TakeDamage()
    {
        currentHealth--;

        PlaySound(hitSound, hitVolume);

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.12f, 0.08f);
        }

        UpdateHeartUI();

        Debug.Log("Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            PlaySound(dieSound, dieVolume);

            GameManager.Instance.GameOver();
        }
    }

    // =====================================================
    // HIT FLASH
    // =====================================================

    private System.Collections.IEnumerator HitFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = hitFlashColor;

        yield return new WaitForSeconds(hitFlashDuration);

        spriteRenderer.color = originalColor;
    }

    private System.Collections.IEnumerator HealFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = healFlashColor;

        yield return new WaitForSeconds(healFlashDuration);

        spriteRenderer.color = originalColor;
    }

    // =====================================================
    // PLAYER BLINK
    // =====================================================

    private System.Collections.IEnumerator EnergyBlinkCoroutine()
    {
        while (hasEnergy && isEnergyEnding)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }

            yield return new WaitForSeconds(0.1f);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // =====================================================
    // ICON BLINK
    // =====================================================

    private System.Collections.IEnumerator BlinkIcon(
        GameObject icon,
        System.Func<bool> isActive
    )
    {
        if (icon == null)
        {
            yield break;
        }

        float blinkSpeed = 0.3f;

        while (isActive())
        {
            icon.SetActive(false);

            yield return new WaitForSeconds(blinkSpeed);

            if (!isActive())
            {
                break;
            }

            icon.SetActive(true);

            yield return new WaitForSeconds(blinkSpeed);

            blinkSpeed -= 0.03f;

            if (blinkSpeed < 0.05f)
            {
                blinkSpeed = 0.05f;
            }
        }

        icon.SetActive(false);
    }

    // =====================================================
    // SHIELD
    // =====================================================

    private void ActivateShield()
    {
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        shieldCoroutine = StartCoroutine(ShieldCoroutine());
    }

    private void BreakShield()
    {
        hasShield = false;

        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }

        if (shieldIcon != null)
        {
            shieldIcon.SetActive(false);
        }

        Debug.Log("Shield Broken");
    }

    private System.Collections.IEnumerator ShieldCoroutine()
    {
        hasShield = true;

        if (shieldIcon != null)
        {
            shieldIcon.SetActive(true);
        }

        Debug.Log("Shield Activated");

        float timer = shieldDuration;

        bool warningStarted = false;

        while (timer > 0f && hasShield)
        {
            timer -= Time.deltaTime;

            if (timer <= warningTime && !warningStarted)
            {
                warningStarted = true;

                StartCoroutine(BlinkIcon(
                    shieldIcon,
                    () => hasShield
                ));
            }

            yield return null;
        }

        hasShield = false;

        if (shieldIcon != null)
        {
            shieldIcon.SetActive(false);
        }

        shieldCoroutine = null;

        Debug.Log("Shield Ended");
    }

    // =====================================================
    // ENERGY
    // =====================================================

    private void ActivateEnergy()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.04f);
        }

        if (energyCoroutine != null)
        {
            StopCoroutine(energyCoroutine);
        }

        energyCoroutine = StartCoroutine(EnergyCoroutine());
    }

    private System.Collections.IEnumerator EnergyCoroutine()
    {
        hasEnergy = true;
        isEnergyEnding = false;

        if (energyIcon != null)
        {
            energyIcon.SetActive(true);
        }

        if (animator != null)
        {
            animator.SetBool("hasEnergy", true);
        }

        GameManager.Instance.SetEnergyMode(true);

        float timer = energyDuration;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timer <= warningTime && !isEnergyEnding)
            {
                isEnergyEnding = true;

                StartCoroutine(EnergyBlinkCoroutine());

                StartCoroutine(BlinkIcon(
                    energyIcon,
                    () => hasEnergy && isEnergyEnding
                ));

                GameManager.Instance.StartEnergyEnding();
            }

            yield return null;
        }

        hasEnergy = false;
        isEnergyEnding = false;

        if (energyIcon != null)
        {
            energyIcon.SetActive(false);
        }

        if (animator != null)
        {
            animator.SetBool("hasEnergy", false);
        }

        GameManager.Instance.SetEnergyMode(false);

        energyCoroutine = null;
    }

    // =====================================================
    // STRESS
    // =====================================================

    private void ActivateStress()
    {
        if (stressCoroutine != null)
        {
            StopCoroutine(stressCoroutine);
        }

        stressCoroutine = StartCoroutine(StressCoroutine());
    }

    private System.Collections.IEnumerator StressCoroutine()
    {
        hasStress = true;

        if (stressIcon != null)
        {
            stressIcon.SetActive(true);
        }

        GameManager.Instance.SetStressMode(true);

        float timer = stressDuration;

        bool warningStarted = false;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timer <= warningTime && !warningStarted)
            {
                warningStarted = true;

                StartCoroutine(BlinkIcon(
                    stressIcon,
                    () => hasStress
                ));
            }

            yield return null;
        }

        hasStress = false;

        if (stressIcon != null)
        {
            stressIcon.SetActive(false);
        }

        GameManager.Instance.SetStressMode(false);

        stressCoroutine = null;
    }

    // =====================================================
    // UI
    // =====================================================

    private void UpdateHeartUI()
    {
        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHealth);
        }
    }

    // =====================================================
    // AUDIO
    // =====================================================

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    // =====================================================
    // QUIT
    // =====================================================

    private void QuitGame()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}