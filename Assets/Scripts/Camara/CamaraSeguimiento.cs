using UnityEngine;

/// <summary>
/// Cámara que sube con el jugador pero NUNCA baja.
/// Si el jugador sale por la parte de abajo de la pantalla, muere.
/// Al reaparecer en un checkpoint, la cámara se recoloca a esa altura.
/// Ponlo en la Main Camera (que debe ser Orthographic).
/// </summary>
[RequireComponent(typeof(Camera))]
public class CamaraSeguimiento : MonoBehaviour
{
    public PlayerHealth jugador;

    [Tooltip("Cuánto por encima del jugador queda el centro de la cámara (positivo = ves más hacia arriba).")]
    public float desplazamientoY = 2f;
    [Tooltip("Suavizado del movimiento (más alto = más rápido).")]
    public float suavizado = 5f;
    [Tooltip("Margen por debajo del borde inferior antes de morir.")]
    public float margenMuerte = 0.5f;

    [Header("Horizontal")]
    [Tooltip("Desactívalo si el nivel cabe a lo ancho en pantalla (cámara fija en X).")]
    public bool seguirEnX = false;

    private Camera cam;
    private float alturaMaxima;
    private bool estabaMuerto;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (jugador == null) jugador = FindAnyObjectByType<PlayerHealth>();
    }

    void Start()
    {
        if (jugador != null) Recolocar(jugador.transform.position);
        else alturaMaxima = transform.position.y;
    }

    void LateUpdate()
    {
        if (jugador == null) return;

        // Acaba de reaparecer (estaba muerto y ya no): bajamos la cámara al checkpoint
        if (estabaMuerto && !jugador.IsDead) Recolocar(jugador.transform.position);
        estabaMuerto = jugador.IsDead;
        if (jugador.IsDead) return;

        // Solo sube: el objetivo nunca es más bajo que lo máximo alcanzado
        float objetivoY = jugador.transform.position.y + desplazamientoY;
        if (objetivoY > alturaMaxima) alturaMaxima = objetivoY;

        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, alturaMaxima, suavizado * Time.deltaTime);
        if (seguirEnX) pos.x = Mathf.Lerp(pos.x, jugador.transform.position.x, suavizado * Time.deltaTime);
        transform.position = pos;

        // ¿Ha salido por abajo?
        float bordeInferior = transform.position.y - cam.orthographicSize;
        if (jugador.transform.position.y < bordeInferior - margenMuerte)
        {
            jugador.Die();
        }
    }

    /// <summary>Coloca la cámara de golpe a la altura del punto indicado.</summary>
    void Recolocar(Vector3 punto)
    {
        alturaMaxima = punto.y + desplazamientoY;
        Vector3 pos = transform.position;
        pos.y = alturaMaxima;
        if (seguirEnX) pos.x = punto.x;
        transform.position = pos;
    }
}
