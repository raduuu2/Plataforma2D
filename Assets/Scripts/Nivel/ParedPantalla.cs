using UnityEngine;

// Va en el prefab Pared, que se usa para las dos paredes de la torre y para el suelo del principio.
//  - Paredes: se pegan a los bordes de la pantalla, sea cual sea su tamaño.
//  - Suelo (el que está en el centro): ocupa todo el ancho y queda detrás de las paredes.
public class ParedPantalla : MonoBehaviour
{
    // Ancho de la pared (cada bloque del castillo mide 1)
    public float ancho = 2f;

    // Las plataformas llegan hasta x = 9, así que la pared nunca se mete más adentro
    public float interiorMinimo = 9f;

    // Dibujos de cada pared (con el borde de piedra mirando hacia dentro) y del suelo
    public Sprite spriteIzquierda;

    public Sprite spriteDerecha;

    public Sprite spriteSuelo;

    // -1 = pared izquierda, 1 = pared derecha, 0 = suelo
    private int lado;

    private Camera cam;

    private SpriteRenderer sr;

    private BoxCollider2D col;

    // Borde de arriba (no se mueve nunca) y borde de abajo (baja si hace falta)
    private float arriba;

    private float abajo;

    void Start()
    {
        cam = Camera.main;

        sr = GetComponent<SpriteRenderer>();

        col = GetComponent<BoxCollider2D>();

        arriba = transform.position.y + sr.size.y / 2f;

        abajo = transform.position.y - sr.size.y / 2f;

        // Según dónde esté sabemos qué es
        if (transform.position.x < -1f)
        {
            lado = -1;

            sr.sprite = spriteIzquierda;
        }
        else if (transform.position.x > 1f)
        {
            lado = 1;

            sr.sprite = spriteDerecha;
        }
        else
        {
            lado = 0;

            sr.sprite = spriteSuelo;

            // El suelo por detrás de las paredes
            sr.sortingOrder = -1;
        }
    }

    void Update()
    {
        // Mitad del ancho de la pantalla (la cámara siempre está en x = 0)
        float mitadPantalla = cam.orthographicSize * cam.aspect;

        // Lo más adentro que llega la pared
        float bordeInterior = Mathf.Max(interiorMinimo, mitadPantalla - ancho);

        // Que nunca se vea el final por abajo: si la pantalla llega más abajo, la estiramos
        // (solo crece, así los ladrillos no se mueven cada fotograma)
        float abajoPantalla = cam.transform.position.y - cam.orthographicSize - 1f;

        if (abajoPantalla < abajo)
        {
            abajo = abajoPantalla;
        }

        float alto = arriba - abajo;

        float centroY = (arriba + abajo) / 2f;

        if (lado == 0)
        {
            // El suelo llega hasta debajo de las dos paredes
            float anchoSuelo = (bordeInterior + ancho) * 2f;

            transform.position = new Vector3(transform.position.x, centroY, 0f);

            sr.size = new Vector2(anchoSuelo, alto);

            col.size = new Vector2(anchoSuelo, alto);
        }
        else
        {
            transform.position = new Vector3(lado * (bordeInterior + ancho / 2f), centroY, 0f);

            sr.size = new Vector2(ancho, alto);

            col.size = new Vector2(ancho, alto);
        }
    }
}
