using UnityEngine;

// La niebla que sube desde abajo: si te toca, mueres
public class Niebla : MonoBehaviour
{
    public float velocidad = 0.5f;

    // Si se queda muy atrás, sube un poco más rápido: esto por cada unidad que le falta.
    // Así alcanza poco a poco, sin tirones
    public float alcancePorDistancia = 0.5f;

    // A qué velocidad baja cuando el jugador muere
    public float velocidadBajada = 40f;

    public Player jugador;

    // Al morir, la niebla baja hasta esta distancia por debajo del checkpoint
    // (la niebla mide 40 de alto, así que 20 es la mitad + 7 de margen)
    public float distanciaReaparicion = 27f;

    // Lo más lejos que puede estar el centro de la niebla por debajo de la cámara
    // (la cámara ve 7 hacia abajo y la niebla mide 20 desde el centro, así que con 26 se ve 1)
    public float distanciaMaxima = 26f;

    // Una copia más clara de la niebla que está detrás y se balancea (es hija de la niebla)
    public Transform capaAtras;

    // Cuánto se balancea la capa de atrás hacia los lados
    public float balanceo = 0.6f;

    // Trocitos de niebla que salen por arriba
    public GameObject jironPrefab;

    // Cada cuánto sale un jirón
    public float tiempoEntreJirones = 0.15f;

    private float contadorJirones = 0f;

    private SpriteRenderer sr;

    private Color colorInicial;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        colorInicial = sr.color;
    }

    void Update()
    {
        Animar();

        if (GameManager.Instance.partidaTerminada == true)
        {
            return;
        }

        if (jugador.muerto == true)
        {
            // Bajamos la niebla (rápido, pero sin teletransportarla) para que el jugador pueda reaparecer
            float limite = jugador.puntoReaparicion.y - distanciaReaparicion;

            if (transform.position.y > limite)
            {
                transform.position += Vector3.down * velocidadBajada * Time.deltaTime;

                // Que no se pase del límite
                if (transform.position.y < limite)
                {
                    transform.position = new Vector3(transform.position.x, limite, 0f);
                }
            }

            return;
        }

        float velocidadActual = velocidad;

        // Si el jugador sube muy rápido, la niebla se queda abajo y ya no se ve.
        // Entonces va un poco más rápida cuanto más lejos está
        float minimo = Camera.main.transform.position.y - distanciaMaxima;

        if (transform.position.y < minimo)
        {
            velocidadActual = velocidadActual + (minimo - transform.position.y) * alcancePorDistancia;
        }

        transform.position += Vector3.up * velocidadActual * Time.deltaTime;

        SoltarJiron();
    }

    // La niebla se mueve un poco para que parezca viva
    void Animar()
    {
        // Respira: se aclara y se oscurece un poco
        Color color = colorInicial;

        color.a = colorInicial.a * (0.9f + 0.1f * Mathf.Sin(Time.time * 1.5f));

        sr.color = color;

        // La capa de atrás se balancea hacia los lados y sube y baja
        if (capaAtras != null)
        {
            float x = Mathf.Sin(Time.time * 0.7f) * balanceo;

            float y = 0.45f + Mathf.Sin(Time.time * 1.3f) * 0.15f;

            capaAtras.localPosition = new Vector3(x, y, 0f);
        }
    }

    // Cada poco tiempo sale un jirón por arriba, en un sitio al azar de la pantalla
    void SoltarJiron()
    {
        if (jironPrefab == null)
        {
            return;
        }

        contadorJirones -= Time.deltaTime;

        if (contadorJirones > 0)
        {
            return;
        }

        contadorJirones = tiempoEntreJirones;

        // La parte de arriba de la niebla (mide 40, así que 20 desde el centro)
        float arriba = transform.position.y + 20f;

        // Si la parte de arriba no se ve en pantalla, no hace falta
        float abajoPantalla = Camera.main.transform.position.y - Camera.main.orthographicSize;

        if (arriba < abajoPantalla - 1f)
        {
            return;
        }

        // La pantalla mide unos 25 de ancho, así que de -12 a 12
        Vector3 posicion = new Vector3(Random.Range(-12f, 12f), arriba - 0.3f, 0f);

        GameObject jiron = Instantiate(jironPrefab, posicion, Quaternion.identity);

        float tamano = Random.Range(0.3f, 0.7f);

        jiron.transform.localScale = new Vector3(tamano, tamano, 1f);
    }

    // Stay en vez de Enter: si el jugador estaba invencible al entrar, le mata cuando se le acaba
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().RecibirDanio();
        }
    }
}
