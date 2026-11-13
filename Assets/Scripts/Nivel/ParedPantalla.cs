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

    void Start()
    {
        cam = Camera.main;

        sr = GetComponent<SpriteRenderer>();

        col = GetComponent<BoxCollider2D>();

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

        if (lado == 0)
        {
            // El suelo llega hasta debajo de las dos paredes
            float anchoSuelo = (bordeInterior + ancho) * 2f;

            sr.size = new Vector2(anchoSuelo, sr.size.y);

            col.size = new Vector2(anchoSuelo, col.size.y);
        }
        else
        {
            transform.position = new Vector3(lado * (bordeInterior + ancho / 2f), transform.position.y, 0f);

            sr.size = new Vector2(ancho, sr.size.y);

            col.size = new Vector2(ancho, col.size.y);
        }
    }
}
