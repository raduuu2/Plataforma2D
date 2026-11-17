using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject panelControles;

    // Panel del volumen (se abre con el botón "Volumen")
    public GameObject panelVolumen;

    // Barra para el volumen de todo el juego
    public Slider sliderVolumen;

    // Texto con el porcentaje ("80%")
    public Text textoPorcentaje;

    // Sonido al pulsar los botones
    public AudioClip sonidoBoton;

    // Volumen del sonido (de 0 a 1)
    public float volumenBoton = 0.4f;

    private AudioSource audioSource;

    // Escena que se carga después de que suene el botón
    private string escenaACargar;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        // Si venimos de perder o ganar, el tiempo estaba parado (timeScale = 0)
        Time.timeScale = 1f;

        // Máximo 60 FPS. Sin esto el juego va a miles de FPS y en ordenadores flojos se cuelga
        Application.targetFrameRate = 60;

        // Cargamos el volumen guardado (si no hay ninguno, al máximo)
        float volumen = PlayerPrefs.GetFloat("Volumen", 1f);

        AudioListener.volume = volumen;

        if (sliderVolumen != null)
        {
            sliderVolumen.value = volumen;
        }

        ActualizarPorcentaje(volumen);
    }

    // La llama la barra de volumen cada vez que se mueve
    public void CambiarVolumen(float valor)
    {
        // AudioListener.volume es el volumen de todo el juego (de 0 a 1)
        AudioListener.volume = valor;

        // Lo guardamos para la próxima vez que se abra el juego
        PlayerPrefs.SetFloat("Volumen", valor);

        ActualizarPorcentaje(valor);
    }

    void ActualizarPorcentaje(float valor)
    {
        if (textoPorcentaje != null)
        {
            // 0.8 -> "80%"
            textoPorcentaje.text = (int)(valor * 100) + "%";
        }
    }

    public void Jugar()
    {
        SonarBoton();

        escenaACargar = "Nivel1";

        // Esperamos un poquito para que se oiga el sonido antes de cambiar de escena
        Invoke("CargarEscena", 0.3f);
    }

    public void JugarNiebla()
    {
        SonarBoton();

        escenaACargar = "NivelNiebla";

        Invoke("CargarEscena", 0.3f);
    }

    void CargarEscena()
    {
        SceneManager.LoadScene(escenaACargar);
    }

    public void MostrarControles()
    {
        SonarBoton();

        panelControles.SetActive(true);
    }

    public void OcultarControles()
    {
        SonarBoton();

        panelControles.SetActive(false);
    }

    public void MostrarVolumen()
    {
        SonarBoton();

        panelVolumen.SetActive(true);
    }

    public void OcultarVolumen()
    {
        SonarBoton();

        panelVolumen.SetActive(false);
    }

    void SonarBoton()
    {
        if (sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton, volumenBoton);
        }
    }
}
