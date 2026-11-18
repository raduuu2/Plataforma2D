using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int puntos = 0;

    public int vidas = 3;

    public int monedas = 0;

    public float tiempo = 0f;

    public bool partidaTerminada = false;

    public bool pausado = false;

    public Player jugador;

    // Sonido al llegar a la cima (opcional)
    public AudioClip sonidoVictoria;

    // Textos del HUD
    public Text textoPuntos;

    public Text textoVidas;

    public Text textoMonedas;

    public Text textoTiempo;

    public Text textoPowerUp;

    // Paneles de la interfaz
    public GameObject panelVictoria;

    public GameObject panelDerrota;

    public GameObject panelPausa;

    // Texto con los puntos finales en cada panel
    public Text textoFinalVictoria;

    public Text textoFinalDerrota;

    // No usamos DontDestroyOnLoad: al reiniciar el nivel queremos un GameManager nuevo
    // con los puntos y las vidas a cero.
    private void Awake()
    {
        Instance = this;

        Time.timeScale = 1f;

        // Máximo 60 FPS. Sin esto el juego va a miles de FPS y en ordenadores flojos se cuelga
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (partidaTerminada == false && pausado == false)
        {
            tiempo += Time.deltaTime;
        }

        // Pausa con Escape
        if (Input.GetKeyDown(KeyCode.Escape) && partidaTerminada == false)
        {
            if (pausado == true)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }

        // Actualizar el HUD
        textoPuntos.text = "Puntos: " + puntos;

        textoVidas.text = "Vidas: " + vidas;

        textoMonedas.text = "Monedas: " + monedas;

        textoTiempo.text = "Tiempo: " + (int)tiempo;

        if (jugador.powerUpActivo != "")
        {
            textoPowerUp.text = jugador.powerUpActivo + ": " + (int)jugador.tiempoPowerUp + "s";
        }
        else
        {
            textoPowerUp.text = "";
        }
    }

    public void SumarPuntos(int cantidad)
    {
        puntos += cantidad;
    }

    public void MonedaRecogida(int cantidad)
    {
        monedas += 1;

        puntos += cantidad;
    }

    public void JugadorMuerto()
    {
        vidas -= 1;

        if (vidas <= 0)
        {
            vidas = 0;

            Perder();
        }
    }

    public void Ganar()
    {
        partidaTerminada = true;

        textoFinalVictoria.text = "Puntos: " + puntos + "\nMonedas: " + monedas + "\nTiempo: " + (int)tiempo + " s";

        // Canción de victoria (la música de fondo se para sola en el script Musica)
        // (a tope, porque es una canción y no un efecto)
        jugador.ReproducirSonido(sonidoVictoria, 1f);

        panelVictoria.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Perder()
    {
        partidaTerminada = true;

        textoFinalDerrota.text = "Puntos: " + puntos + "\nMonedas: " + monedas + "\nTiempo: " + (int)tiempo + " s";

        panelDerrota.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Pausar()
    {
        pausado = true;

        panelPausa.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Reanudar()
    {
        pausado = false;

        panelPausa.SetActive(false);

        Time.timeScale = 1f;
    }

    // Para los botones de los menús
    public void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenu()
    {
        // Al perder o ganar el tiempo se queda parado, lo volvemos a poner normal
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuPrincipal");
    }

    public void IrANivelNiebla()
    {
        SceneManager.LoadScene("NivelNiebla");
    }
}
