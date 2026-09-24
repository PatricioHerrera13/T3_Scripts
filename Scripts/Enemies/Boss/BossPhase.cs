using UnityEngine;

[System.Serializable]
public class BossPhase
{
    [Tooltip("Nombre solo para identificación en el Inspector")]
    public string phaseName = "Phase 1";

    [Tooltip("Porcentaje de vida en el que se activa esta fase (1 = 100%, 0.5 = 50%, etc.)")]
    [Range(0f, 1f)]
    public float healthThreshold = 1f;
}