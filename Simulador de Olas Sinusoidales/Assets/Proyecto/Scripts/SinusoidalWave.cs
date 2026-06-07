using UnityEngine;

/// <summary>
/// Simula olas sinusoidales modificando SOLO la coordenada Y de los vértices.
/// Fórmula: y = A * sin((2π/L) * (x - v*t) + φ)
/// Adjuntar al mismo GameObject que WaterMeshGenerator.
/// </summary>
[RequireComponent(typeof(WaterMeshGenerator))]
public class SinusoidalWave : MonoBehaviour
{
    [System.Serializable]
    public class WaveParams
    {
        public float amplitude = 0.5f;   // A: altura de la ola
        public float wavelength = 5f;     // L: longitud de onda
        public float speed = 1.5f;   // v: velocidad de propagación
        public float phase = 0f;     // φ: desfase inicial
        [Range(0f, 360f)]
        public float directionAngle = 0f; // Dirección en grados (0 = eje X)
    }

    [Header("Parámetros de olas (puedes añadir varias)")]
    public WaveParams[] waves = new WaveParams[]
    {
        new WaveParams { amplitude = 0.4f, wavelength = 6f,  speed = 1.2f, phase = 0f,   directionAngle = 0f   },
        new WaveParams { amplitude = 0.2f, wavelength = 3f,  speed = 2f,   phase = 1.5f, directionAngle = 45f  },
        new WaveParams { amplitude = 0.15f,wavelength = 4f,  speed = 1.8f, phase = 0.8f, directionAngle = -30f }
    };

    [Header("Control")]
    public bool isActive = true;

    private WaterMeshGenerator waterMesh;

    void Awake()
    {
        waterMesh = GetComponent<WaterMeshGenerator>();
    }

    void Update()
    {
        if (!isActive) return;
        UpdateWaves();
    }

    void UpdateWaves()
    {
        Vector3[] verts = waterMesh.vertices;
        Vector3[] baseVerts = waterMesh.baseVertices;
        float t = Time.time;

        for (int i = 0; i < verts.Length; i++)
        {
            float x = baseVerts[i].x;
            float z = baseVerts[i].z;

            // Resetear Y a la posición base
            float y = 0f;

            // Sumar contribución de cada ola
            foreach (var wave in waves)
            {
                float rad = wave.directionAngle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                // Proyección del vértice sobre la dirección de la ola
                float projection = dir.x * x + dir.y * z;

                float k = (2f * Mathf.PI) / wave.wavelength; // número de onda
                float v = wave.speed;

                // Fórmula sinusoidal: A * sin(k*(proj - v*t) + φ)
                y += wave.amplitude * Mathf.Sin(k * (projection - v * t) + wave.phase);
            }

            // Solo modificamos Y
            verts[i] = new Vector3(baseVerts[i].x, y, baseVerts[i].z);
        }

        waterMesh.ApplyVertices();
    }
    /// <summary>
    /// Devuelve la altura del agua en una posición XZ del mundo.
    /// Usado por la boya para calcular flotabilidad.
    /// </summary>
    public float GetWaterHeight(float worldX, float worldZ)
    {
        float t = Time.time;
        float y = 0f;

        foreach (var wave in waves)
        {
            float rad = wave.directionAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

            float projection = dir.x * worldX + dir.y * worldZ;
            float k = (2f * Mathf.PI) / wave.wavelength;

            y += wave.amplitude * Mathf.Sin(k * (projection - wave.speed * t) + wave.phase);
        }

        return y;
    }
}