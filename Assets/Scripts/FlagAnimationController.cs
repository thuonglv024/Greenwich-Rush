using UnityEngine;

public class FlagAnimationController : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        animator.SetBool(
            "isGameStarted",
            GameManager.Instance.isGameStarted
        );
    }
}