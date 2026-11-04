using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && activated == false)
        {
            activated = true;

            other.GetComponent<Player>().respawnPoint = transform.position;

            GetComponent<SpriteRenderer>().color = Color.green;
        }
    }
}
