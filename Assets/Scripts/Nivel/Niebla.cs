using UnityEngine;

// La niebla que sube desde abajo: si te toca, mueres
public class Niebla : MonoBehaviour
{
    public float velocidad = 0.5f;

    public Player jugador;

    // Al morir, la niebla baja hasta esta distancia por debajo del checkpoint
    // (la niebla mide 40 de alto, así que 20 es la mitad + 7 de margen)
    public float distanciaReaparicion = 27f;

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
