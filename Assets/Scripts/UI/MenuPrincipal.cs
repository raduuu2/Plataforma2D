using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject panelControles;

    public void Jugar()
    {
        SceneManager.LoadScene("Nivel1");
    }

    public void JugarNiebla()
    {
        SceneManager.LoadScene("NivelNiebla");
    }

    public void MostrarControles()
    {
        panelControles.SetActive(true);
    }

    public void OcultarControles()
    {
        panelControles.SetActive(false);
    }

    public void Salir()
    {
        Application.Quit();
    }
}
