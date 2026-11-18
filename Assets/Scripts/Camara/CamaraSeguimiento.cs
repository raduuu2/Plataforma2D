using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    public Player jugador;

    public float alturaExtra = 1f;

    public float suavizado = 5f;

    // Cuánto tiene que bajar por debajo del borde de la pantalla para morir
    // (1 = justo cuando el caballero ya no se ve)
    public float margenMuerte = 1f;

    private float alturaMaxima;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        alturaMaxima = jugador.transform.position.y + alturaExtra;
    }

    void LateUpdate()
    {
        // Si ha muerto, la cámara vuelve a la altura del checkpoint
        if (jugador.muerto == true)
        {
            alturaMaxima = jugador.puntoReaparicion.y + alturaExtra;
        }

        // Solo sube cuando el jugador está en el suelo y más alto que antes (nunca baja)
        if (jugador.enSuelo == true && jugador.transform.position.y + alturaExtra > alturaMaxima)
        {
            alturaMaxima = jugador.transform.position.y + alturaExtra;
        }

        // En X se queda en el centro de la torre, solo se mueve en Y
        float y = Mathf.Lerp(transform.position.y, alturaMaxima, suavizado * Time.deltaTime);

        transform.position = new Vector3(0f, y, -10f);

        // ¿Se ha caído por debajo de la pantalla?
        // Se mira el borde de abajo de donde va la cámara (alturaMaxima), así muere
        // en cuanto sale de la pantalla, aunque caiga encima de una plataforma que no se ve
        float bordeAbajo = alturaMaxima - cam.orthographicSize;

        if (jugador.muerto == false && jugador.transform.position.y < bordeAbajo - margenMuerte)
        {
            jugador.Caer();
        }
    }
}
