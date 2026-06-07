using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la UI para alternar entre simulación Sinusoidal y Gerstner.
/// Gestiona que solo una simulación esté activa a la vez.
/// </summary>
public class WaveUIController : MonoBehaviour
{
    [Header("Referencias a simulaciones")]
    public SinusoidalWave sinusoidalWave;
    public GerstnerWave gerstnerWave;

    [Header("Elementos UI")]
    public Toggle sinusoidalToggle;
    public Toggle gerstnerToggle;
    public TextMeshProUGUI activeWaveLabel;  // Texto que muestra cuál está activa

    void Start()
    {
        // Estado inicial: sinusoidal activa, Gerstner inactiva
        SetSinusoidal(true);

        // Listeners de los toggles
        sinusoidalToggle.onValueChanged.AddListener(OnSinusoidalToggle);
        gerstnerToggle.onValueChanged.AddListener(OnGerstnerToggle);

        UpdateLabel();
    }

    void OnSinusoidalToggle(bool value)
    {
        if (value) SetSinusoidal(true);
    }

    void OnGerstnerToggle(bool value)
    {
        if (value) SetGerstner(true);
    }

    void SetSinusoidal(bool active)
    {
        sinusoidalWave.isActive = active;
        gerstnerWave.isActive = !active;

        // Sincronizar toggles sin disparar listeners
        sinusoidalToggle.SetIsOnWithoutNotify(active);
        gerstnerToggle.SetIsOnWithoutNotify(!active);

        // Resetear malla al cambiar de modo
        ResetMesh();
        UpdateLabel();
    }

    void SetGerstner(bool active)
    {
        gerstnerWave.isActive = active;
        sinusoidalWave.isActive = !active;

        sinusoidalToggle.SetIsOnWithoutNotify(!active);
        gerstnerToggle.SetIsOnWithoutNotify(active);

        ResetMesh();
        UpdateLabel();
    }

    /// <summary>
    /// Resetea los vértices al cambiar de simulación para evitar
    /// artefactos visuales por posiciones residuales.
    /// </summary>
    void ResetMesh()
    {
        WaterMeshGenerator water = sinusoidalWave.GetComponent<WaterMeshGenerator>();
        if (water == null) return;

        for (int i = 0; i < water.vertices.Length; i++)
            water.vertices[i] = water.baseVertices[i];

        water.ApplyVertices();
    }

    void UpdateLabel()
    {
        if (activeWaveLabel == null) return;
        activeWaveLabel.text = gerstnerWave.isActive
            ? "Modo: Gerstner"
            : "Modo: Sinusoidal";
    }
}