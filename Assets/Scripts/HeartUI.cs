using UnityEngine;

public class HeartUI : MonoBehaviour
{
    public GameObject[] heartIcons;

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            heartIcons[i].SetActive(i < currentHealth);
        }
    }
}