using UnityEngine;

public class GroundController : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        float speed = GameManager.Instance.GetFinalGameSpeed() / transform.localScale.x;

        meshRenderer.material.mainTextureOffset += Vector2.right * speed * Time.deltaTime;
    }
}