using UnityEngine;

public class Player : MonoBehaviour
{
    public float velocidad = 6f;

    public float fuerzaSalto = 14f;

    public bool enSuelo = false;

    public bool muerto = false;

    public Vector3 puntoReaparicion;

    // Los power-ups cambian estos valores (1 = normal)
    public float multiplicadorVelocidad = 1f;

    public float multiplicadorSalto = 1f;

    // Con el escudo los enemigos no te matan
    public bool escudo = false;

    // Nombre del power-up que tienes y el tiempo que le queda
    public string powerUpActivo = "";

    public float tiempoPowerUp = 0f;

    // Ataque con la espada
    public float radioAtaque = 0.7f;

    // Cuánto por delante del caballero está el golpe
    public float alcanceAtaque = 0.9f;

    // Lo que tarda la espada en bajar en la animación (el golpe cuenta en ese momento)
    public float retrasoGolpe = 0.15f;

    public float tiempoEntreAtaques = 0.3f;

    private float contadorAtaque = 0f;

    // Sonidos
    public AudioClip sonidoSalto;

    public AudioClip sonidoAtaque;

    public AudioClip sonidoDanio;

    public AudioClip sonidoCaida;

    public AudioClip sonidoMuerte;

    public AudioClip sonidoPaso1;

    public AudioClip sonidoPaso2;

    // Volumen del sonido de daño (de 0 a 1)
    public float volumenDanio = 0.8f;

    // Volumen de los pasos (suenan todo el rato, así que bajitos)
    public float volumenPasos = 0.4f;

    // Volumen de todos los efectos (salto, ataque, monedas...) para que se oiga bien la música
    public float volumenEfectos = 0.4f;

    // Color del caballero (blanco = el del dibujo). Se cambia en cada nivel desde el Inspector
    public Color colorNormal = Color.white;

    // Cada cuánto suena un paso al caminar
    public float tiempoEntrePasos = 0.3f;

    private float contadorPasos = 0f;

    private bool tocaPaso1 = true;

    // Al reaparecer eres invencible un rato (y parpadeas)
    public float tiempoInvencibleAlReaparecer = 2f;

    private float tiempoInvencible = 0f;

    private AudioSource audioSource;

    private Rigidbody2D rb;

    private Animator animator;

    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.freezeRotation = true; // Que el personaje no se caiga de lado al chocar

        animator = GetComponent<Animator>();

        sr = GetComponent<SpriteRenderer>();

        // Color del caballero en este nivel (en el de la niebla es distinto)
        sr.color = colorNormal;

        // Altavoz para los sonidos del jugador
        audioSource = gameObject.AddComponent<AudioSource>();

        puntoReaparicion = transform.position;
    }

    void Update()
    {
        if (muerto == true)
        {
            return;
        }

        // Si la partida ha terminado o está en pausa no nos movemos
        if (GameManager.Instance.partidaTerminada == true || GameManager.Instance.pausado == true)
        {
            return;
        }

        // Movimiento lateral
        float direccion = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            direccion = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            direccion = 1f;
        }

        rb.linearVelocity = new Vector2(direccion * velocidad * multiplicadorVelocidad, rb.linearVelocity.y);

        // Girar al personaje hacia donde camina
        if (direccion < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direccion > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Pasos: si camina por el suelo, suena un paso cada cierto tiempo (alternando los dos)
        contadorPasos -= Time.deltaTime;

        if (direccion != 0 && enSuelo == true && contadorPasos <= 0)
        {
            if (tocaPaso1 == true)
            {
                ReproducirSonido(sonidoPaso1, volumenPasos);
            }
            else
            {
                ReproducirSonido(sonidoPaso2, volumenPasos);
            }

            tocaPaso1 = !tocaPaso1;

            contadorPasos = tiempoEntrePasos;
        }

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && enSuelo == true)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto * multiplicadorSalto);

            enSuelo = false;

            ReproducirSonido(sonidoSalto);
        }

        // Ataque (tecla F o clic izquierdo)
        contadorAtaque -= Time.deltaTime;

        if ((Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)) && contadorAtaque <= 0)
        {
            Atacar();
        }

        // Invencible después de reaparecer: parpadea encendiendo y apagando el dibujo
        if (tiempoInvencible > 0)
        {
            tiempoInvencible -= Time.deltaTime;

            sr.enabled = (int)(tiempoInvencible * 10) % 2 == 0;

            if (tiempoInvencible <= 0)
            {
                sr.enabled = true;
            }
        }

        // Tiempo del power-up
        if (tiempoPowerUp > 0)
        {
            tiempoPowerUp -= Time.deltaTime;

            if (tiempoPowerUp <= 0)
            {
                QuitarPowerUp();
            }
        }

        // Animaciones
        if (animator != null)
        {
            animator.SetFloat("movimiento", Mathf.Abs(direccion));

            animator.SetBool("enSuelo", enSuelo);

            animator.SetFloat("velocidadY", rb.linearVelocity.y);
        }
    }

    // Mientras toca algo, miramos si ese algo está DEBAJO de los pies
    // (normal.y > 0.5 significa que la superficie mira hacia arriba, o sea, es suelo y no una pared)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            enSuelo = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        enSuelo = false;
    }

    void Atacar()
    {
        contadorAtaque = tiempoEntreAtaques;

        ReproducirSonido(sonidoAtaque);

        if (animator != null)
        {
            animator.SetTrigger("atacar");
        }

        // El golpe cuenta cuando la espada baja, no nada más pulsar
        Invoke("Golpear", retrasoGolpe);
    }

    void Golpear()
    {
        if (muerto == true)
        {
            return;
        }

        // Punto delante del caballero, a la altura del pecho
        // (localScale.x es 1 si mira a la derecha y -1 si mira a la izquierda)
        Vector3 puntoAtaque = transform.position + new Vector3(transform.localScale.x * alcanceAtaque, 0.6f, 0f);

        // Todo lo que toque ese círculo recibe el espadazo
        Collider2D[] golpeados = Physics2D.OverlapCircleAll(puntoAtaque, radioAtaque);

        foreach (Collider2D golpeado in golpeados)
        {
            EnemigoVolador enemigo = golpeado.GetComponent<EnemigoVolador>();

            if (enemigo != null)
            {
                enemigo.Morir();
            }

            // También se pueden romper las bolas de fuego
            Proyectil bola = golpeado.GetComponent<Proyectil>();

            if (bola != null)
            {
                Destroy(bola.gameObject);
            }
        }
    }

    // Lo llama el script PowerUp al cogerlo
    public void ActivarPowerUp(string tipo, float duracion)
    {
        QuitarPowerUp();

        powerUpActivo = tipo;

        tiempoPowerUp = duracion;

        if (tipo == "Velocidad")
        {
            multiplicadorVelocidad = 1.6f;

            sr.color = Color.yellow;
        }

        if (tipo == "Salto")
        {
            multiplicadorSalto = 1.35f;

            sr.color = Color.green;
        }

        if (tipo == "Escudo")
        {
            escudo = true;

            sr.color = Color.cyan;
        }
    }

    public void QuitarPowerUp()
    {
        multiplicadorVelocidad = 1f;

        multiplicadorSalto = 1f;

        escudo = false;

        powerUpActivo = "";

        tiempoPowerUp = 0f;

        sr.color = colorNormal;
    }

    // Lo llama el enemigo cuando te toca
    public void RecibirGolpe()
    {
        if (escudo == false)
        {
            RecibirDanio();
        }
    }

    // Saltito al pisar a un enemigo
    public void Rebotar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto * 0.8f);
    }

    // Lo usan las monedas y el propio jugador para que suene algo
    public void ReproducirSonido(AudioClip clip)
    {
        ReproducirSonido(clip, 1f);
    }

    // Igual, pero eligiendo el volumen (de 0 a 1)
    public void ReproducirSonido(AudioClip clip, float volumen)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volumen * volumenEfectos);
        }
    }

    // Lo llaman los enemigos, las bolas de fuego y la niebla
    public void RecibirDanio()
    {
        // Si está muerto o acaba de reaparecer, no le pasa nada
        if (muerto == true || tiempoInvencible > 0)
        {
            return;
        }

        ReproducirSonido(sonidoDanio, volumenDanio);

        Morir();
    }

    // Lo llama la cámara cuando te caes por debajo de la pantalla
    public void Caer()
    {
        if (muerto == true)
        {
            return;
        }

        ReproducirSonido(sonidoCaida);

        Morir();
    }

    void Morir()
    {
        muerto = true;

        rb.linearVelocity = Vector2.zero;

        rb.simulated = false;

        QuitarPowerUp();

        tiempoInvencible = 0f;

        sr.enabled = true;

        // Animación de muerte
        if (animator != null)
        {
            animator.SetBool("muerto", true);
        }

        GameManager.Instance.JugadorMuerto();

        if (GameManager.Instance.partidaTerminada == false)
        {
            Invoke("Respawn", 1f);
        }
        else
        {
            // Era la última vida
            ReproducirSonido(sonidoMuerte);
        }
    }

    void Respawn()
    {
        transform.SetParent(null); // por si murió encima de una plataforma móvil

        transform.position = puntoReaparicion;

        rb.simulated = true;

        rb.linearVelocity = Vector2.zero;

        sr.color = colorNormal;

        if (animator != null)
        {
            animator.SetBool("muerto", false);
        }

        // Unos segundos de invencibilidad para que no te maten nada más aparecer
        tiempoInvencible = tiempoInvencibleAlReaparecer;

        muerto = false;
    }
}
