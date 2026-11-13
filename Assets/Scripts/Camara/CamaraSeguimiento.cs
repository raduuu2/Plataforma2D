using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    public Player jugador;

    public float alturaExtra = 1f;

    public float suavizado = 5f;

    // Si el jugador baja más que esto por debajo del centro de la cámara, muere
    public float distanciaMuerte = 8.5f;

    private float alturaMaxima;

    void Start()
    {
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
        if (jugador.transform.position.y < transform.position.y - distanciaMuerte)
        {
            jugador.Caer();
        }
    }
}
