using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float floatHeight = 0.2f;

    private float leftEdge;
    private Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.position;
        leftEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).x - 2f;
    }

    private void Update()
    {
        float speed = GameManager.Instance.GetFinalGameSpeed();

        // Di chuyển cùng tốc độ game
        basePosition += Vector3.left * speed * Time.deltaTime;

        // Chỉ nhấp lên xuống
        transform.position =
            basePosition +
            Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // Xoá khi ra khỏi màn hình
        if (transform.position.x < leftEdge)
        {
            Destroy(gameObject);
        }
    }
}