using UnityEngine;

public class RevealTrigger : MonoBehaviour
{
    public GameObject platform; // Assign HiddenPlatform here

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            platform.SetActive(true);
        }
    }
}
