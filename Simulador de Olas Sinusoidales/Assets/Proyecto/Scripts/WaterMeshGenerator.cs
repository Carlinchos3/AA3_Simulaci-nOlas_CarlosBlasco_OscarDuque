using UnityEngine;

/// <summary>
/// Genera una malla plana procedural que servirá de base para
/// las simulaciones de olas sinusoidal y de Gerstner.
/// Adjuntar a un GameObject vacío con MeshFilter y MeshRenderer.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterMeshGenerator : MonoBehaviour
{
    [Header("Dimensiones de la malla")]
    public int resolution = 60;       // Vértices por lado (más = más detalle)
    public float size = 20f;          // Tamaño total en unidades de Unity

    // Referencia pública para que los scripts de olas accedan
    [HideInInspector] public Mesh mesh;
    [HideInInspector] public Vector3[] baseVertices;   // Posiciones originales (en reposo)
    [HideInInspector] public Vector3[] vertices;       // Posiciones actuales (modificadas por olas)

    void Awake()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "WaterMesh";
        GetComponent<MeshFilter>().mesh = mesh;

        int vertCount = resolution * resolution;
        vertices = new Vector3[vertCount];
        baseVertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        // Triangulos: cada celda = 2 triángulos = 6 índices
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        float step = size / (resolution - 1);
        float halfSize = size / 2f;

        // Generar vértices en una grilla plana centrada en el origen
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * resolution + x;
                float px = x * step - halfSize;
                float pz = z * step - halfSize;

                vertices[i] = new Vector3(px, 0f, pz);
                baseVertices[i] = new Vector3(px, 0f, pz);
                uvs[i] = new Vector2((float)x / (resolution - 1),
                                              (float)z / (resolution - 1));
            }
        }

        // Generar triángulos
        int t = 0;
        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int i = z * resolution + x;
                // Triángulo superior izquierdo
                triangles[t++] = i;
                triangles[t++] = i + resolution;
                triangles[t++] = i + 1;
                // Triángulo inferior derecho
                triangles[t++] = i + 1;
                triangles[t++] = i + resolution;
                triangles[t++] = i + resolution + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Llama esto desde los scripts de olas después de modificar el array vertices[].
    /// </summary>
    public void ApplyVertices()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}