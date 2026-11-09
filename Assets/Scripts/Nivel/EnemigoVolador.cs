using UnityEngine;

public class EnemigoVolador : MonoBehaviour
{
    public float velocidad = 2f;

    // Cuánto se aleja a cada lado de donde empieza
    public float distancia = 3f;

    public int points = 50;

    // Ataque: bola de fuego
    public GameObject proyectilPrefab;

    public float tiempoEntreDisparos = 3f;

    // Solo dispara si el jugador está más cerca que esto
    public float distanciaDisparo = 7f;

    // Sonido al morir (opcional)
    public AudioClip sonidoMuerte;

    private bool muerto = false;

    private float contadorDisparo;

    private Vector3 inicio;

    private int direccion = 1;

    private SpriteRenderer sr;

    private Player player;

    void Start()
    {
        inicio = transform.position;

        sr = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        // Así no disparan todos a la vez
        contadorDisparo = Random.Range(1f, tiempoEntreDisparos);
    }

    void Update()
    {
        // Si está muriéndose ya no se mueve ni dispara
        if (muerto == true)
        {
            return;
        }

        // Va de un lado a otro como la pelota del Pong
        transform.position += Vector3.right * direccion * velocidad * Time.deltaTime;

        if (transform.position.x > inicio.x + distancia)
        {
            direccion = -1;
        }

        if (transform.position.x < inicio.x - distancia)
        {
            direccion = 1;
        }

        // El dibujo del demonio mira a la izquierda, así que lo giramos al ir a la derecha
        sr.flipX = direccion == 1;

        // Disparar cada cierto tiempo si el jugador está cerca
        contadorDisparo -= Time.deltaTime;

        if (contadorDisparo <= 0)
        {
            contadorDisparo = tiempoEntreDisparos;

            float distanciaAlJugador = Vector2.Distance(transform.position, player.transform.position);

            if (distanciaAlJugador < distanciaDisparo && player.isDead == false)
            {
                Disparar();
            }
        }
    }

    void Disparar()
    {
        GameObject bola = Instantiate(proyectilPrefab, transform.position, Quaternion.identity);

        // Apuntamos al pecho del jugador (su posición está en los pies)
        Vector3 objetivo = player.transform.position + new Vector3(0f, 0.6f, 0f);

        bola.GetComponent<Proyectil>().direccion = (objetivo - transform.position).normalized;
    }

    // Lo llama el caballero al darle con la espada, o al pisarlo
    public void Morir()
    {
        if (muerto == true)
        {
            return;
        }

        muerto = true;

        GameManager.Instance.AddPoints(points);

        player.ReproducirSonido(sonidoMuerte);

        // Ya no puede hacer daño
        GetComponent<Collider2D>().enabled = false;

        // Animación de muerte y lo borramos cuando termina
        GetComponent<Animator>().SetTrigger("morir");

        Destroy(gameObject, 0.7f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player jugador = other.GetComponent<Player>();

            // Si el jugador le cae encima, muere el enemigo
            if (jugador.transform.position.y > transform.position.y)
            {
                jugador.Rebotar();

                Morir();
            }
            else
            {
                jugador.RecibirGolpe();
            }
        }
    }
}
