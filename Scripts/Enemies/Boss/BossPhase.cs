using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BossPhase
{
    [Tooltip("Nombre solo para identificación en el Inspector")]
    public string phaseName = "Phase 1";

    [Tooltip("1.0 = 100% de vida, 0.5 = 50%, etc.")]
    [Range(0f, 1f)]
    public float healthThreshold = 1f;

    [Header("Attacks de esta fase")]
    [Tooltip("Arrastrá aquí los componentes de ataque que se pueden usar en esta fase")]
    public List<MonoBehaviour> allowedAttacks = new List<MonoBehaviour>();

    [Header("Transición (opcional)")]
    [Tooltip("Tiempo de transición / invulnerabilidad al entrar en esta fase")]
    public float transitionDuration = 0f;
}