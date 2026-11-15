using UnityEngine;

// Hace que el título del menú suba y baje despacio
public class TituloFlotante : MonoBehaviour
{
    // Cuánto sube y baja (en píxeles de la pantalla)
    public float altura = 10f;

    public float velocidad = 1.5f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition = posicionInicial + Vector3.up * Mathf.Sin(Time.time * velocidad) * altura;
    }
}
