using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private float leftEdge;

    private void Start()
    {
        Debug.Log(gameObject.name + " spawned at " + transform.position);
        leftEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).x - 2f;
    }

    private void Update()
    {
        if (GameManager.Instance.isGameOver)
        {
            Destroy(gameObject);
            return;
        }

        if (GameManager.Instance == null) return;

        float speed = GameManager.Instance.GetFinalGameSpeed();

        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < leftEdge)
        {
            Destroy(gameObject);
        }
    }
}