using UnityEngine;

public class InfiniteLayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;

    [Header("Loop")]
    public float recycleOffset = 10f;

    [Header("Spacing")]
    public float extraSpacing = 0f;

    [Header("Fix Small Gap")]
    public float overlapFix = 0.05f;

    [Header("Camera")]
    public float leftBoundOffset = 30f;

    [Header("Loop Type")]
    public bool useDynamicWidth = true;

    [Header("Ground Mode")]
    public bool matchGameSpeed = false;

    [Header("Parallax")]
    public float speedInfluence = 1f;

    private Transform[] segments;

    private Camera mainCamera;

    // =====================================================
    // UNITY
    // =====================================================

    private void Start()
    {
        mainCamera = Camera.main;

        // =========================================
        // AUTO GET ALL CHILDREN
        // =========================================

        segments = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            segments[i] = transform.GetChild(i);
        }
    }

    private void Update()
    {
        // =========================================
        // WAIT GAME MANAGER
        // =========================================

        if (GameManager.Instance == null)
        {
            return;
        }

        // =========================================
        // WAIT GAME START
        // =========================================

        if (!GameManager.Instance.isGameStarted)
        {
            return;
        }

        // =========================================
        // STOP WHEN GAME OVER
        // =========================================

        if (GameManager.Instance.isGameOver)
        {
            return;
        }

        MoveSegments();

        RecycleSegments();
    }

    // =====================================================
    // MOVE
    // =====================================================

    private void MoveSegments()
    {
        float finalSpeed;

        // =====================================
        // GROUND SPEED
        // =====================================

        if (matchGameSpeed)
        {
            finalSpeed =
                GameManager.Instance.GetFinalGameSpeed();
        }
        else
        {
            // =====================================
            // NORMAL PARALLAX
            // =====================================

            float speedIncrease =
                GameManager.Instance.gameSpeed -
                GameManager.Instance.initialGameSpeed;

            finalSpeed =
                moveSpeed +
                (speedIncrease * speedInfluence);
        }

        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].position +=
                Vector3.left *
                finalSpeed *
                Time.deltaTime;
        }
    }

    // =====================================================
    // LOOP
    // =====================================================

    private void RecycleSegments()
    {
        if (mainCamera == null)
        {
            return;
        }

        float leftBound =
            mainCamera.transform.position.x -
            leftBoundOffset;

        Transform rightMost = GetRightMostSegment();

        if (rightMost == null)
        {
            return;
        }

        for (int i = 0; i < segments.Length; i++)
        {
            Transform current = segments[i];

            // =========================================
            // RECYCLE WHEN TOO FAR LEFT
            // =========================================

            if (current.position.x < leftBound)
            {
                Vector3 newPos = current.position;

                // =====================================
                // DIFFERENT SIZE MODE
                // =====================================

                if (useDynamicWidth)
                {
                    SpriteRenderer rightRenderer =
                        rightMost.GetComponent<SpriteRenderer>();

                    SpriteRenderer currentRenderer =
                        current.GetComponent<SpriteRenderer>();

                    if (rightRenderer == null ||
                        currentRenderer == null)
                    {
                        return;
                    }

                    float rightMostEdge =
                        rightRenderer.bounds.max.x;

                    float currentHalfWidth =
                        currentRenderer.bounds.size.x * 0.5f;

                    newPos.x =
                        rightMostEdge +
                        currentHalfWidth -
                        overlapFix +
                        extraSpacing;
                }
                else
                {
                    // =====================================
                    // SAME SIZE MODE
                    // =====================================

                    newPos.x =
                        rightMost.position.x +
                        recycleOffset;
                }

                current.position = newPos;

                rightMost = current;
            }
        }
    }

    // =====================================================
    // GET RIGHT MOST
    // =====================================================

    private Transform GetRightMostSegment()
    {
        if (segments == null || segments.Length == 0)
        {
            return null;
        }

        Transform rightMost = segments[0];

        for (int i = 1; i < segments.Length; i++)
        {
            if (segments[i].position.x >
                rightMost.position.x)
            {
                rightMost = segments[i];
            }
        }

        return rightMost;
    }
}