using UnityEngine;

// Partícula de decoración del fondo: luciérnagas y motas de polvo.
// Va como hija de la cámara, así que siempre está dentro de la pantalla.
// Si se sale por un lado, vuelve a entrar por el otro.
public class ParticulaFondo : MonoBehaviour
{
    public Vector2 velocidad = new Vector2(0.3f, 0f);

    // Cuánto hace eses al moverse
    public float ondulacion = 0.5f;

    // Las luciérnagas se encienden y se apagan; el polvo no
    public bool parpadea = false;

    private float desfase;

    private float alfaInicial;

    private SpriteRenderer sr;

    private Camera cam;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        cam = Camera.main;

        alfaInicial = sr.color.a;

        // Para que no se muevan todas igual
        desfase = Random.Range(0f, 10f);
    }

    void Update()
    {
        Vector3 posicion = transform.localPosition;

        posicion.x += (velocidad.x + Mathf.Sin(Time.time + desfase) * ondulacion) * Time.deltaTime;

        posicion.y += (velocidad.y + Mathf.Cos(Time.time * 0.8f + desfase) * ondulacion) * Time.deltaTime;

        // Tamaño de la pantalla
        float mitadAlto = cam.orthographicSize + 0.5f;

        float mitadAncho = cam.orthographicSize * cam.aspect + 0.5f;

        // Si se sale por un lado, aparece por el contrario
        if (posicion.x > mitadAncho)
        {
            posicion.x = -mitadAncho;
        }

        if (posicion.x < -mitadAncho)
        {
            posicion.x = mitadAncho;
        }

        if (posicion.y > mitadAlto)
        {
            posicion.y = -mitadAlto;
        }

        if (posicion.y < -mitadAlto)
        {
            posicion.y = mitadAlto;
        }

        transform.localPosition = posicion;

        // Parpadeo: la transparencia sube y baja
        if (parpadea == true)
        {
            Color color = sr.color;

            color.a = alfaInicial * (0.5f + 0.5f * Mathf.Sin(Time.time * 2f + desfase));

            sr.color = color;
        }
    }
}
