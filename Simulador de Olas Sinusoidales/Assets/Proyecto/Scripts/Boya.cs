using UnityEngine;

/// <summary>
/// Simula la flotabilidad de una boya sobre las olas (Sinusoidal o Gerstner).
/// Fórmula: F = ρ * g * V_desplazado
/// donde V_desplazado depende de cuánto está sumergido el objeto.
/// Ajusta el drag y la masa automáticamente según el modo de ola activo.
/// </summary>
public class Boya : MonoBehaviour
{
    [Header("Referencias")]
    public SinusoidalWave sinusoidalWave;
    public GerstnerWave gerstnerWave;

    [Header("Física de flotabilidad")]
    public float waterDensity = 1000f;  // ρ: densidad del agua (kg/m³)
    public float objectVolume = 0.5f;   // Volumen total del objeto (m³)

    [Header("Masa por modo")]
    public float objectMassSinusoidal = 100f;    // Masa para sinusoidal (kg)
    public float objectMassGerstner = 1000f;   // Masa para Gerstner (kg)

    [Header("Drag por modo")]
    public float dragSinusoidal = 16f;   // Drag calibrado para sinusoidal
    public float dragGerstner = 35f;   // Drag mayor para Gerstner (olas más agresivas)

    [Header("Tamaño del objeto")]
    public float objectHeight = 1f;     // Altura total del objeto

    private Rigidbody rb;
    private const float gravity = 9.81f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        // Masa inicial según modo activo al arrancar
        rb.mass = objectMassSinusoidal;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 1f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        // Actualizar masa y drag según modo activo
        UpdatePhysicsParams();

        float waterHeight = GetCurrentWaterHeight();
        float activeDrag = GetActiveDrag();
        ApplyBuoyancy(waterHeight, activeDrag);
    }

    /// <summary>
    /// Sincroniza la masa del Rigidbody con el modo de ola activo.
    /// </summary>
    void UpdatePhysicsParams()
    {
        if (gerstnerWave != null && gerstnerWave.isActive)
            rb.mass = objectMassGerstner;
        else
            rb.mass = objectMassSinusoidal;
    }

    float GetCurrentWaterHeight()
    {
        float x = transform.position.x;
        float z = transform.position.z;

        if (gerstnerWave != null && gerstnerWave.isActive)
            return gerstnerWave.GetWaterHeight(x, z);

        if (sinusoidalWave != null && sinusoidalWave.isActive)
            return sinusoidalWave.GetWaterHeight(x, z);

        return 0f;
    }

    /// <summary>
    /// Devuelve el drag correspondiente al modo activo.
    /// </summary>
    float GetActiveDrag()
    {
        if (gerstnerWave != null && gerstnerWave.isActive)
            return dragGerstner;

        return dragSinusoidal;
    }

    void ApplyBuoyancy(float waterHeight, float dragCoeff)
    {
        float bottomOfObject = transform.position.y - objectHeight / 2f;

        float submergedDepth = waterHeight - bottomOfObject;
        submergedDepth = Mathf.Clamp(submergedDepth, 0f, objectHeight);

        float fractionSubmerged = submergedDepth / objectHeight;
        float volumeSubmerged = objectVolume * fractionSubmerged;

        if (volumeSubmerged > 0f)
        {
            float buoyancyForce = waterDensity * gravity * volumeSubmerged;

            rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);

            float verticalVelocity = rb.linearVelocity.y;
            rb.AddForce(Vector3.down * verticalVelocity * dragCoeff, ForceMode.Force);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, objectHeight / 2f);
    }
}