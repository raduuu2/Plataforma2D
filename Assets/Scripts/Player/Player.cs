using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla al caballero: movimiento lateral, salto, animaciones y muerte/reaparición.
/// Requiere Rigidbody2D y un Collider2D en el mismo objeto.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 6f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    [Tooltip("Margen (segundos) para saltar justo después de salir de una plataforma.")]
    public float coyoteTime = 0.1f;
    [Tooltip("Margen (segundos) para que cuente un salto pulsado justo antes de tocar el suelo.")]
    public float jumpBuffer = 0.1f;
    [Tooltip("Al soltar el botón de salto en subida, la velocidad vertical se multiplica por esto (salto variable).")]
    [Range(0f, 1f)] public float cortarSalto = 0.5f;

    [Header("Detección de suelo")]
    [Tooltip("Capas que cuentan como suelo. Si se deja en Nothing, cuenta cualquier collider.")]
    public LayerMask capaSuelo;

    [Header("Referencias (opcional)")]
    public Animator animator;

    // Multiplicadores que usarán los power-ups (1 = normal)
    [HideInInspector] public float multVelocidad = 1f;
    [HideInInspector] public float multSalto = 1f;

    public bool EnSuelo { get; private set; }
    public bool EstaMuerto { get; private set; }

    private Rigidbody2D rb;
    private Collider2D col;
    private float inputX;
    private float tiempoDesdeSuelo;
    private float tiempoDesdeSaltoPulsado = float.MaxValue;
    private Vector3 puntoReaparicion;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        puntoReaparicion = transform.position;
    }

    void Update()
    {
        if (EstaMuerto) return;

        LeerInput();
        ComprobarSuelo();

        // Salto con coyote time + buffer
        if (tiempoDesdeSaltoPulsado <= jumpBuffer && tiempoDesdeSuelo <= coyoteTime)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto * multSalto);
            tiempoDesdeSaltoPulsado = float.MaxValue;
            tiempoDesdeSuelo = float.MaxValue;
        }

        // Salto variable: si sueltas el botón mientras subes, salta menos
        if (SaltoSoltado() && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * cortarSalto);
        }

        // Girar el sprite
        if (inputX > 0.01f) transform.localScale = new Vector3(1, 1, 1);
        else if (inputX < -0.01f) transform.localScale = new Vector3(-1, 1, 1);

        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        if (EstaMuerto) return;
        rb.linearVelocity = new Vector2(inputX * velocidad * multVelocidad, rb.linearVelocity.y);
    }

    // ---------------- Input (teclado y mando) ----------------

    void LeerInput()
    {
        inputX = 0f;
        var kb = Keyboard.current;
        var pad = Gamepad.current;

        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) inputX -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) inputX += 1f;
        }
        if (pad != null && Mathf.Abs(pad.leftStick.x.ReadValue()) > 0.2f)
        {
            inputX = pad.leftStick.x.ReadValue();
        }

        bool saltoPulsado =
            (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)) ||
            (pad != null && pad.buttonSouth.wasPressedThisFrame);

        if (saltoPulsado) tiempoDesdeSaltoPulsado = 0f;
        else tiempoDesdeSaltoPulsado += Time.deltaTime;
    }

    bool SaltoSoltado()
    {
        var kb = Keyboard.current;
        var pad = Gamepad.current;
        return (kb != null && (kb.spaceKey.wasReleasedThisFrame || kb.wKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame)) ||
               (pad != null && pad.buttonSouth.wasReleasedThisFrame);
    }

    // ---------------- Suelo ----------------

    void ComprobarSuelo()
    {
        // ¿Estamos tocando algo cuya superficie mira hacia arriba? (= tenemos algo debajo de los pies)
        // Si "Capa Suelo" está en Nothing, vale cualquier collider sólido.
        ContactFilter2D filtro = new ContactFilter2D();
        filtro.useTriggers = false;
        filtro.SetNormalAngle(45f, 135f);
        if (capaSuelo.value != 0) filtro.SetLayerMask(capaSuelo);

        EnSuelo = rb.IsTouching(filtro);

        if (EnSuelo) tiempoDesdeSuelo = 0f;
        else tiempoDesdeSuelo += Time.deltaTime;
    }

    // ---------------- Animaciones ----------------

    void ActualizarAnimaciones()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.SetFloat("movement", Mathf.Abs(inputX));
        animator.SetBool("enSuelo", EnSuelo);
        animator.SetFloat("velocidadY", rb.linearVelocity.y);
    }

    // ---------------- Muerte y checkpoints ----------------

    /// <summary>Lo llamará el checkpoint al tocarlo.</summary>
    public void SetCheckpoint(Vector3 posicion)
    {
        puntoReaparicion = posicion;
    }

    /// <summary>Se lanza al reaparecer (la cámara lo usa para volver a bajar al checkpoint).</summary>
    public event System.Action<Vector3> AlReaparecer;

    /// <summary>Lo llamarán los enemigos, la cámara (caída) o la lava.</summary>
    public void Morir()
    {
        if (EstaMuerto) return;
        EstaMuerto = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // se queda quieto y no choca con nada mientras está muerto

        if (GameManager.Instance != null) GameManager.Instance.JugadorMuerto();

        bool partidaTerminada = GameManager.Instance != null && GameManager.Instance.PartidaTerminada;
        if (!partidaTerminada) Invoke(nameof(Reaparecer), 0.6f);
    }

    public void Reaparecer()
    {
        transform.position = puntoReaparicion;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        multVelocidad = 1f;
        multSalto = 1f;
        EstaMuerto = false;
        AlReaparecer?.Invoke(puntoReaparicion);
    }

    void OnDrawGizmosSelected()
    {
        var c = GetComponent<Collider2D>();
        if (c == null) return;
        Bounds b = c.bounds;
        Gizmos.color = EnSuelo ? Color.green : Color.red;
        Gizmos.DrawWireCube(new Vector3(b.center.x, b.min.y - 0.025f), new Vector3(b.size.x * 0.9f, 0.05f));
    }
}
