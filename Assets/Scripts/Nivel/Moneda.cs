using UnityEngine;

public class Moneda : MonoBehaviour
{
    public int puntos = 10;

    public AudioClip sonido;

    // Volumen del sonido (de 0 a 1)
    public float volumen = 0.8f;

    // Animación: los dibujos de la moneda girando (se pasan uno detrás de otro)
    public Sprite[] fotogramas;

    public float fotogramasPorSegundo = 10f;

    // Además flota arriba y abajo
    public float alturaFlote = 0.12f;

    public float velocidadFlote = 2.5f;

    // Chispas al cogerla
    public GameObject chispaPrefab;

    public Color colorChispas = new Color(1f, 0.85f, 0.25f, 1f);

    private Vector3 posicionInicial;

    private SpriteRenderer sr;

    // Para que no floten todas a la vez
    private float desfase;

    // Al cogerla sube y se encoge antes de desaparecer
    private bool recogida = false;

    private float tiempoRecogida = 0f;

    void Start()
    {
        posicionInicial = transform.position;

        sr = GetComponent<SpriteRenderer>();

        desfase = posicionInicial.x + posicionInicial.y;
    }

    void Update()
    {
        if (recogida == true)
        {
            // Sube un poco y se hace pequeña en 0.3 segundos
            tiempoRecogida += Time.deltaTime;

            transform.position += Vector3.up * 2f * Time.deltaTime;

            transform.localScale = Vector3.one * (1f - tiempoRecogida / 0.3f);

            if (tiempoRecogida >= 0.3f)
            {
                Destroy(gameObject);
            }

            return;
        }

        float t = Time.time + desfase;

        // Arriba y abajo
        transform.position = posicionInicial + Vector3.up * Mathf.Sin(t * velocidadFlote) * alturaFlote;

        // Cambiamos de dibujo para que parezca que gira
        int fotograma = (int)(t * fotogramasPorSegundo) % fotogramas.Length;

        sr.sprite = fotogramas[fotograma];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recogida == false && other.CompareTag("Player"))
        {
            recogida = true;

            GameManager.Instance.MonedaRecogida(puntos);

            // La moneda se destruye, así que el sonido lo hace sonar el jugador
            other.GetComponent<Player>().ReproducirSonido(sonido, volumen);

            Chispa.Explosion(chispaPrefab, transform.position, colorChispas, 8);
        }
    }
}
