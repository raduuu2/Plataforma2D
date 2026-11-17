using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject panelControles;

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

        // Máximo 60 FPS. Sin esto el juego va a miles de FPS y en ordenadores flojos se cuelga
        Application.targetFrameRate = 60;
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

    void SonarBoton()
    {
        if (sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton, volumenBoton);
        }
    }
}
