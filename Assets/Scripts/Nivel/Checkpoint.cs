using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool activado = false;

    public AudioClip sonido;

    // Volumen del sonido (de 0 a 1)
    public float volumen = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && activado == false)
        {
            activado = true;

            Player jugador = other.GetComponent<Player>();

            jugador.puntoReaparicion = transform.position;

            jugador.ReproducirSonido(sonido, volumen);

            // El cartel empieza gris (apagado) y se ilumina al activarlo
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
