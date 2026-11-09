using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int points = 0;

    public int lives = 3;

    public int monedas = 0;

    public float tiempo = 0f;

    public bool gameOver = false;

    public bool paused = false;

    public Player player;

    // Sonido al llegar a la cima (opcional)
    public AudioClip sonidoVictoria;

    // Textos del HUD
    public Text textoPuntos;

    public Text textoVidas;

    public Text textoMonedas;

    public Text textoTiempo;

    public Text textoPowerUp;

    // Paneles de la interfaz
    public GameObject winPanel;

    public GameObject losePanel;

    public GameObject pausePanel;

    // Texto con los puntos finales en cada panel
    public Text textoFinalVictoria;

    public Text textoFinalDerrota;

    // No usamos DontDestroyOnLoad: al reiniciar el nivel queremos un GameManager nuevo
    // con los puntos y las vidas a cero.
    private void Awake()
    {
        Instance = this;

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (gameOver == false && paused == false)
        {
            tiempo += Time.deltaTime;
        }

        // Pausa con Escape
        if (Input.GetKeyDown(KeyCode.Escape) && gameOver == false)
        {
            if (paused == true)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }

        // Actualizar el HUD
        textoPuntos.text = "Puntos: " + points;

        textoVidas.text = "Vidas: " + lives;

        textoMonedas.text = "Monedas: " + monedas;

        textoTiempo.text = "Tiempo: " + (int)tiempo;

        if (player.powerUpActivo != "")
        {
            textoPowerUp.text = player.powerUpActivo + ": " + (int)player.tiempoPowerUp + "s";
        }
        else
        {
            textoPowerUp.text = "";
        }
    }

    public void AddPoints(int amount)
    {
        points += amount;
    }

    public void MonedaRecogida(int amount)
    {
        monedas += 1;

        points += amount;
    }

    public void PlayerDied()
    {
        lives -= 1;

        if (lives <= 0)
        {
            lives = 0;

            Lose();
        }
    }

    public void Win()
    {
        gameOver = true;

        textoFinalVictoria.text = "Puntos: " + points + "\nMonedas: " + monedas + "\nTiempo: " + (int)tiempo + " s";

        player.ReproducirSonido(sonidoVictoria);

        winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Lose()
    {
        gameOver = true;

        textoFinalDerrota.text = "Puntos: " + points + "\nMonedas: " + monedas + "\nTiempo: " + (int)tiempo + " s";

        losePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Pausar()
    {
        paused = true;

        pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Reanudar()
    {
        paused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // Para los botones de los menús
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void GoToLava()
    {
        SceneManager.LoadScene("NivelLava");
    }
}
