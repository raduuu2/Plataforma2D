using UnityEngine;

public class EnemigoVolador : MonoBehaviour
{
    public float velocidad = 2f;

    // Cuánto se aleja a cada lado de donde empieza
    public float distancia = 3f;

    public int puntos = 50;

    // Ataque: bola de fuego
    public GameObject proyectilPrefab;

    public float tiempoEntreDisparos = 3f;

    // Solo dispara si el jugador está más cerca que esto
    public float distanciaDisparo = 7f;

    // Tiempo que tarda en cargar la bola antes de dispararla (así el jugador ve que va a disparar)
    public float tiempoCarga = 0.7f;

    // Sonido al morir (opcional)
    public AudioClip sonidoMuerte;

    // Sonido al escupir la bola (opcional)
    public AudioClip sonidoDisparo;

    // Efectos del disparo (los mismos que usan las monedas y los power-ups)
    public GameObject chispaPrefab;

    public GameObject aroPrefab;

    public Color colorFuego = new Color(1f, 0.55f, 0.2f);

    // Color del demonio (blanco = el del dibujo). Se cambia en cada nivel desde el Inspector
    public Color colorNormal = Color.white;

    // Cuánto sube y baja mientras vuela
    public float alturaFlote = 0.25f;

    public float velocidadFlote = 3f;

    // Animación hecha pasando dibujos, como la moneda (la usa la calavera, el demonio usa el Animator)
    public Sprite[] fotogramas;

    public float fotogramasPorSegundo = 12f;

    private float contadorFotogramas = 0f;

    private bool muerto = false;

    private float contadorDisparo;

    // Si es mayor que 0, está cargando la bola
    private float contadorCarga = 0f;

    private Vector3 inicio;

    private float posX;

    private int direccion = 1;

    // Para que no floten todos a la vez
    private float desfase;

    private SpriteRenderer sr;

    private Player jugador;

    void Start()
    {
        inicio = transform.position;

        posX = inicio.x;

        sr = GetComponent<SpriteRenderer>();

        // Color del demonio en este nivel (en el de la niebla es distinto)
        sr.color = colorNormal;

        jugador = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        // Así no disparan todos a la vez
        contadorDisparo = Random.Range(1f, tiempoEntreDisparos);

        desfase = Random.Range(0f, 10f);
    }

    void Update()
    {
        // Si está muriéndose ya no se mueve ni dispara
        if (muerto == true)
        {
            return;
        }

        // Si tiene dibujos sueltos, los vamos pasando (mientras carga, el doble de rápido)
        if (fotogramas.Length > 0)
        {
            float rapidez = fotogramasPorSegundo;

            if (contadorCarga > 0)
            {
                rapidez = fotogramasPorSegundo * 2f;
            }

            contadorFotogramas += Time.deltaTime * rapidez;

            sr.sprite = fotogramas[(int)contadorFotogramas % fotogramas.Length];
        }

        // Mientras carga la bola casi se para
        float velocidadActual = velocidad;

        if (contadorCarga > 0)
        {
            velocidadActual = velocidad * 0.2f;
        }

        // Va de un lado a otro como la pelota del Pong
        posX += direccion * velocidadActual * Time.deltaTime;

        if (posX > inicio.x + distancia)
        {
            direccion = -1;
        }

        if (posX < inicio.x - distancia)
        {
            direccion = 1;
        }

        // Y además sube y baja suavemente, como si aleteara
        float y = inicio.y + Mathf.Sin(Time.time * velocidadFlote + desfase) * alturaFlote;

        transform.position = new Vector3(posX, y, 0f);

        if (contadorCarga > 0)
        {
            Cargar();
        }
        else
        {
            // El dibujo del demonio mira a la izquierda, así que lo giramos al ir a la derecha
            sr.flipX = direccion == 1;

            // Disparar cada cierto tiempo si el jugador está cerca
            contadorDisparo -= Time.deltaTime;

            if (contadorDisparo <= 0)
            {
                contadorDisparo = tiempoEntreDisparos;

                float distanciaAlJugador = Vector2.Distance(transform.position, jugador.transform.position);

                if (distanciaAlJugador < distanciaDisparo && jugador.muerto == false)
                {
                    // Empieza a cargar la bola
                    contadorCarga = tiempoCarga;
                }
            }
        }
    }

    // Mientras carga: mira al jugador, se hincha y parpadea en naranja
    void Cargar()
    {
        contadorCarga -= Time.deltaTime;

        // De 0 (empieza a cargar) a 1 (dispara)
        float progreso = 1f - contadorCarga / tiempoCarga;

        // Mira hacia el jugador (el dibujo mira a la izquierda)
        sr.flipX = jugador.transform.position.x > transform.position.x;

        // Se va hinchando
        float escala = 1f + 0.2f * progreso;

        transform.localScale = new Vector3(escala, escala, 1f);

        // Tiembla cada vez más, como si fuera a explotar
        float temblor = Mathf.Sin(Time.time * 60f) * 0.06f * progreso;

        transform.position += new Vector3(temblor, 0f, 0f);

        // Parpadea entre blanco y naranja
        float parpadeo = (Mathf.Sin(Time.time * 30f) + 1f) / 2f;

        sr.color = Color.Lerp(colorNormal, colorFuego, parpadeo);

        if (contadorCarga <= 0)
        {
            // Vuelve a la normalidad y dispara
            transform.localScale = Vector3.one;

            sr.color = colorNormal;

            if (jugador.muerto == false)
            {
                Disparar();
            }
        }
    }

    void Disparar()
    {
        // Apuntamos al pecho del jugador (su posición está en los pies)
        Vector3 objetivo = jugador.transform.position + new Vector3(0f, 0.6f, 0f);

        Vector3 haciaJugador = (objetivo - transform.position).normalized;

        // La bola sale un poco por delante, de la boca
        Vector3 boca = transform.position + haciaJugador * 0.7f;

        GameObject bola = Instantiate(proyectilPrefab, boca, Quaternion.identity);

        bola.GetComponent<Proyectil>().direccion = haciaJugador;

        // Fogonazo: un aro y unas chispas
        if (aroPrefab != null)
        {
            GameObject aro = Instantiate(aroPrefab, boca, Quaternion.identity);

            aro.GetComponent<Aro>().tamanoFinal = 1.5f;

            aro.GetComponent<Aro>().duracion = 0.3f;

            aro.GetComponent<SpriteRenderer>().color = colorFuego;
        }

        if (chispaPrefab != null)
        {
            Chispa.Explosion(chispaPrefab, boca, colorFuego, 8);
        }

        jugador.ReproducirSonido(sonidoDisparo, 0.8f);
    }

    // Lo llama el caballero al darle con la espada, o al pisarlo
    public void Morir()
    {
        if (muerto == true)
        {
            return;
        }

        muerto = true;

        // Lo dejamos con su color y tamaño normal para que la animación se vea bien
        sr.color = colorNormal;

        transform.localScale = Vector3.one;

        GameManager.Instance.SumarPuntos(puntos);

        jugador.ReproducirSonido(sonidoMuerte);

        // Ya no puede hacer daño
        GetComponent<Collider2D>().enabled = false;

        if (fotogramas.Length > 0)
        {
            // La calavera no tiene animación de morir: revienta en chispas y un aro
            if (chispaPrefab != null)
            {
                Chispa.Explosion(chispaPrefab, transform.position, colorFuego, 16);
            }

            if (aroPrefab != null)
            {
                GameObject aro = Instantiate(aroPrefab, transform.position, Quaternion.identity);

                aro.GetComponent<Aro>().tamanoFinal = 2f;

                aro.GetComponent<SpriteRenderer>().color = colorFuego;
            }

            Destroy(gameObject);
        }
        else
        {
            // Animación de muerte del demonio y lo borramos cuando termina
            GetComponent<Animator>().SetTrigger("morir");

            Destroy(gameObject, 0.7f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Si el jugador le cae encima, muere el enemigo
            // (los pies tienen que estar por encima del demonio y el jugador tiene que ir cayendo, no subiendo)
            bool estaEncima = jugador.transform.position.y > transform.position.y + 0.2f;

            bool cayendo = other.attachedRigidbody.linearVelocity.y <= 0.5f;

            if (estaEncima == true && cayendo == true)
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
