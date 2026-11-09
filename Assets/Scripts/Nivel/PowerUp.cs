using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // "Velocidad", "Salto" o "Escudo"
    public string tipo = "Velocidad";

    public float duracion = 8f;

    public AudioClip sonido;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            player.ActivarPowerUp(tipo, duracion);

            // El power-up se destruye, así que el sonido lo hace sonar el jugador
            player.ReproducirSonido(sonido);

            Destroy(gameObject);
        }
    }
}
