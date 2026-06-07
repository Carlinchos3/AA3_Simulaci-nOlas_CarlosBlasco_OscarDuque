using UnityEngine;

[RequireComponent(typeof(WaterMeshGenerator))]
public class GerstnerWave : MonoBehaviour
{
    [System.Serializable]
    public class WaveParams
    {
        public float amplitude = 0.5f;
        public float wavelength = 5f;
        public float speed = 1.5f;
        public float phase = 0f;
        [Range(0f, 1f)]
        public float steepness = 0.5f;
        [Range(0f, 360f)]
        public float directionAngle = 0f;
    }

    //valor de las olas, luego las hemos ido modificando hasta conseguir el resultado correcto
    [Header("Parámetros de olas")]
    public WaveParams[] waves = new WaveParams[]
    {
        new WaveParams { amplitude = 0.4f,  wavelength = 6f, speed = 1.2f, phase = 0f,   steepness = 0.5f, directionAngle = 0f   },
        new WaveParams { amplitude = 0.2f,  wavelength = 3f, speed = 2f,   phase = 1.5f, steepness = 0.4f, directionAngle = 45f  },
        new WaveParams { amplitude = 0.15f, wavelength = 4f, speed = 1.8f, phase = 0.8f, steepness = 0.3f, directionAngle = -30f }
    };

    // Empieza desactivado para no tener las dos avtivas a la vez. con la UI lo controlas
    [Header("Control")]
    public bool isActive = false;

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
            // Posición base (en reposo) del vértice
            float x0 = baseVerts[i].x;
            float z0 = baseVerts[i].z;

            float displaceX = 0f;
            float displaceY = 0f;
            float displaceZ = 0f;

            foreach (var wave in waves)
            {
                float rad = wave.directionAngle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                float k = (2f * Mathf.PI) / wave.wavelength;  // número de onda
                float w = wave.speed * k;                       // frecuencia angular

                // Proyección del punto base sobre la dirección de la ola
                float dot = dir.x * x0 + dir.y * z0;
                float theta = k * dot - w * t + wave.phase;    // fase total

                // Q normalizado para evitar que los vértices se crucen
                // Q_max = 1 / (k * A * numWaves)
                float Q = wave.steepness / (k * wave.amplitude * waves.Length);

                // Desplazamiento horizontal (X y Z) — exclusivo de Gerstner
                displaceX += Q * wave.amplitude * dir.x * Mathf.Cos(theta);
                displaceZ += Q * wave.amplitude * dir.y * Mathf.Cos(theta);

                // Desplazamiento vertical (Y) — igual que sinusoidal
                displaceY += wave.amplitude * Mathf.Sin(theta);
            }

            verts[i] = new Vector3(x0 + displaceX, displaceY, z0 + displaceZ);
        }

        waterMesh.ApplyVertices();
    }

    public float GetWaterHeight(float worldX, float worldZ)
    {
        float t = Time.time;
        float y = 0f;

        foreach (var wave in waves)
        {
            float rad = wave.directionAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

            float k = (2f * Mathf.PI) / wave.wavelength;
            float w = wave.speed * k;

            float dot = dir.x * worldX + dir.y * worldZ;
            float theta = k * dot - w * t + wave.phase;

            y += wave.amplitude * Mathf.Sin(theta);
        }

        return y;
    }
}