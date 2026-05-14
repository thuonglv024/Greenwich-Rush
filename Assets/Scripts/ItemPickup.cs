using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemType itemType;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.CollectItem(itemType);
        }

        Destroy(gameObject);
    }
}