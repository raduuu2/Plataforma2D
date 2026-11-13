using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool activado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && activado == false)
        {
            activado = true;

            other.GetComponent<Player>().puntoReaparicion = transform.position;

            // El cartel empieza gris (apagado) y se ilumina al activarlo
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
