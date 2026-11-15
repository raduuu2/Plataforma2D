using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool activado = false;

    public AudioClip sonido;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && activado == false)
        {
            activado = true;

            Player jugador = other.GetComponent<Player>();

            jugador.puntoReaparicion = transform.position;

            jugador.ReproducirSonido(sonido);

            // El cartel empieza gris (apagado) y se ilumina al activarlo
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
