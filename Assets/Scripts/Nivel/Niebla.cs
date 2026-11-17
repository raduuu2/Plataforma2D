using UnityEngine;

// La niebla que sube desde abajo: si te toca, mueres
public class Niebla : MonoBehaviour
{
    public float velocidad = 0.5f;

    public Player jugador;

    // Al morir, la niebla baja hasta esta distancia por debajo del checkpoint
    // (la niebla mide 40 de alto, así que 20 es la mitad + 7 de margen)
    public float distanciaReaparicion = 27f;

    // Lo más lejos que puede estar el centro de la niebla por debajo de la cámara
    // (la cámara ve 7 hacia abajo y la niebla mide 20 desde el centro, así que con 25.5 se ve un poquito)
    public float distanciaMaxima = 25.5f;

    void Update()
    {
        if (GameManager.Instance.partidaTerminada == true)
        {
            return;
        }

        if (jugador.muerto == true)
        {
            // Bajamos la niebla para que el jugador pueda reaparecer
            float limite = jugador.puntoReaparicion.y - distanciaReaparicion;

            if (transform.position.y > limite)
            {
                transform.position = new Vector3(transform.position.x, limite, 0f);
            }
        }
        else
        {
            transform.position += Vector3.up * velocidad * Time.deltaTime;

            // Si el jugador sube muy rápido, la niebla se queda abajo y ya no se ve.
            // Así que nunca la dejamos más lejos que esto por debajo de la cámara
            float minimo = Camera.main.transform.position.y - distanciaMaxima;

            if (transform.position.y < minimo)
            {
                transform.position = new Vector3(transform.position.x, minimo, 0f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().RecibirDanio();
        }
    }
}
