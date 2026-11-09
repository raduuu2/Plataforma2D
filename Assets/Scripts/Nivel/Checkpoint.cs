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

            // El cartel empieza gris (apagado) y se ilumina al activarlo
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
