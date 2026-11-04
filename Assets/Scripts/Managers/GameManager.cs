using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int points = 0;

    public int lives = 3;

    public bool gameOver = false;

    // Paneles de la interfaz (se crean el fin de semana con el HUD)
    public GameObject winPanel;

    public GameObject losePanel;

    // No usamos DontDestroyOnLoad: al reiniciar el nivel queremos un GameManager nuevo
    // con los puntos y las vidas a cero.
    private void Awake()
    {
        Instance = this;

        Time.timeScale = 1f;
    }

    public void AddPoints(int amount)
    {
        points += amount;
    }

    public void PlayerDied()
    {
        lives -= 1;

        if (lives <= 0)
        {
            Lose();
        }
    }

    public void Win()
    {
        gameOver = true;

        Debug.Log("VICTORIA - Puntos: " + points);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void Lose()
    {
        gameOver = true;

        Debug.Log("DERROTA - Puntos: " + points);

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        Time.timeScale = 0f;
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
}
