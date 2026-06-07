using UnityEngine;

/// <summary>
/// Simula la flotabilidad de una boya sobre las olas (Sinusoidal o Gerstner).
/// Fórmula: F = ρ * g * V_desplazado
/// donde V_desplazado depende de cuánto está sumergido el objeto.
/// </summary>
public class Boya : MonoBehaviour
{
    [Header("Referencias")]
    public SinusoidalWave sinusoidalWave;
    public GerstnerWave gerstnerWave;

    [Header("Física de flotabilidad")]
    public float waterDensity = 1000f;  // ρ: densidad del agua (kg/m³)
    public float objectVolume = 0.5f;   // Volumen total del objeto (m³)
    public float objectMass = 50f;    // Masa del objeto (kg)
    public float dragCoeff = 2f;     // Amortiguación vertical (evita rebote infinito)

    [Header("Tamaño del objeto")]
    public float objectHeight = 1f;     // Altura total del objeto (diámetro si es esfera)

    private Rigidbody rb;
    private const float gravity = 9.81f;

    void Awake()
    {
        // Obtener o añadir Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = objectMass;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 1f;
        // Bloqueamos rotación en X y Z para que no vuelque
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        float waterHeight = GetCurrentWaterHeight();
        ApplyBuoyancy(waterHeight);
    }

    /// <summary>
    /// Obtiene la altura del agua en la posición actual de la boya,
    /// según cuál simulación esté activa.
    /// </summary>
    float GetCurrentWaterHeight()
    {
        float x = transform.position.x;
        float z = transform.position.z;

        // Prioridad: Gerstner si está activo, sino Sinusoidal
        if (gerstnerWave != null && gerstnerWave.isActive)
            return gerstnerWave.GetWaterHeight(x, z);

        if (sinusoidalWave != null && sinusoidalWave.isActive)
            return sinusoidalWave.GetWaterHeight(x, z);

        return 0f;
    }

    /// <summary>
    /// Aplica la fuerza de flotabilidad: F = ρ * g * V_desplazado
    /// V_desplazado se calcula según qué fracción del objeto está sumergida.
    /// </summary>
    void ApplyBuoyancy(float waterHeight)
    {
        float bottomOfObject = transform.position.y - objectHeight / 2f;
        float topOfObject = transform.position.y + objectHeight / 2f;

        // Calcular qué fracción del objeto está bajo el agua
        float submergedDepth = waterHeight - bottomOfObject;
        submergedDepth = Mathf.Clamp(submergedDepth, 0f, objectHeight);

        float fractionSubmerged = submergedDepth / objectHeight;
        float volumeSubmerged = objectVolume * fractionSubmerged;

        // F = ρ * g * V_desplazado  (solo si hay algo sumergido)
        if (volumeSubmerged > 0f)
        {
            float buoyancyForce = waterDensity * gravity * volumeSubmerged;

            // Aplicar fuerza hacia arriba
            rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);

            // Amortiguación: reduce oscilación vertical
            float verticalVelocity = rb.linearVelocity.y;
            rb.AddForce(Vector3.down * verticalVelocity * dragCoeff, ForceMode.Force);
        }
    }

    /// <summary>
    /// Dibuja gizmos en el editor para visualizar la zona de flotabilidad.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, objectHeight / 2f);
    }
}