using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // "Velocidad", "Salto" o "Escudo"
    public string tipo = "Velocidad";

    public float duracion = 8f;

    public AudioClip sonido;

    // Color del power-up (el mismo que se pone el jugador al cogerlo)
    public Color color = Color.yellow;

    // Animación: los dibujos del power-up (se pasan uno detrás de otro)
    public Sprite[] fotogramas;

    public float fotogramasPorSegundo = 8f;

    // Además flota arriba y abajo
    public float alturaFlote = 0.2f;

    public float velocidadFlote = 2f;

    // Brillo que tiene detrás (es un hijo del prefab)
    public SpriteRenderer halo;

    // Efectos al cogerlo
    public GameObject chispaPrefab;

    public GameObject aroPrefab;

    private Vector3 posicionInicial;

    private SpriteRenderer sr;

    void Start()
    {
        posicionInicial = transform.position;

        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Arriba y abajo
        transform.position = posicionInicial + Vector3.up * Mathf.Sin(Time.time * velocidadFlote) * alturaFlote;

        // Cambiamos de dibujo
        int fotograma = (int)(Time.time * fotogramasPorSegundo) % fotogramas.Length;

        sr.sprite = fotogramas[fotograma];

        // El halo crece y se apaga poco a poco, como si latiera
        float pulso = Mathf.Sin(Time.time * 3f);

        halo.transform.localScale = Vector3.one * (2f + 0.3f * pulso);

        Color colorHalo = halo.color;

        colorHalo.a = 0.35f + 0.2f * pulso;

        halo.color = colorHalo;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player jugador = other.GetComponent<Player>();

            jugador.ActivarPowerUp(tipo, duracion);

            // El power-up se destruye, así que el sonido lo hace sonar el jugador
            jugador.ReproducirSonido(sonido);

            // Chispas y un aro de su color
            Chispa.Explosion(chispaPrefab, transform.position, color, 14);

            GameObject aro = Instantiate(aroPrefab, transform.position, Quaternion.identity);

            aro.GetComponent<SpriteRenderer>().color = color;

            Destroy(gameObject);
        }
    }
}
