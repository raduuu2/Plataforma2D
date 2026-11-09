using UnityEngine;

public class Moneda : MonoBehaviour
{
    public int points = 10;

    public AudioClip sonido;

    // Volumen del sonido (de 0 a 1)
    public float volumen = 0.4f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.MonedaRecogida(points);

            // La moneda se destruye, así que el sonido lo hace sonar el jugador
            other.GetComponent<Player>().ReproducirSonido(sonido, volumen);

            Destroy(gameObject);
        }
    }
}
