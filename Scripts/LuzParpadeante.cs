using UnityEngine;
using UnityEngine.Rendering.Universal; // Necesario para controlar luces URP 2D

public class LuzParpadeante : MonoBehaviour
{
    private Light2D luz;
    public float intensidadMinima = 0.8f;
    public float intensidadMaxima = 1.5f;
    public float velocidadParpadeo = 0.1f;

    void Start()
    {
        luz = GetComponent<Light2D>();
    }

    void Update()
    {
        if (luz != null)
        {
            // Mathf.PerlinNoise genera una onda suave y aleatoria, perfecta para fuego
            float ruido = Mathf.PerlinNoise(Time.time / velocidadParpadeo, 0);
            luz.intensity = Mathf.Lerp(intensidadMinima, intensidadMaxima, ruido);
        }
    }
}