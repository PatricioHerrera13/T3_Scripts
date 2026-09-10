using UnityEngine;
using System.Collections.Generic;

public class WeaknessSystem : MonoBehaviour
{
    [Header("Debilidades")]
    [SerializeField] private List<ItemType> weaknesses = new List<ItemType>();

    [Header("Resistencias (opcional)")]
    [SerializeField] private List<ItemType> resistances = new List<ItemType>();

    // Efectos de estado activos
    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    private EnemyHealth health;
    private EnemyCore core;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        core = GetComponent<EnemyCore>();
    }

    private void Update()
    {
        // Actualizar y limpiar efectos expirados
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Tick(Time.deltaTime);

            if (activeEffects[i].IsExpired)
            {
                RemoveEffect(activeEffects[i]);
                activeEffects.RemoveAt(i);
            }
        }
    }

    // ---------- API pública ----------

    public bool IsWeakTo(ItemType item)
    {
        return weaknesses.Contains(item);
    }

    public bool IsResistantTo(ItemType item)
    {
        return resistances.Contains(item);
    }

    public void ApplyItem(ItemType item)
    {
        if (item == ItemType.None) return;

        if (IsWeakTo(item))
        {
            // Aquí decides qué efecto fuerte aplicar según el ítem
            // Por ahora dejamos un ejemplo genérico
            ApplyStatusEffect(new StatusEffect(StatusEffectType.Vulnerable, 4f, 2f));
            Debug.Log($"{gameObject.name} es débil a {item} → Vulnerable aplicado");
        }
        else if (IsResistantTo(item))
        {
            Debug.Log($"{gameObject.name} es resistente a {item}");
        }
        else
        {
            // Efecto normal (opcional)
            Debug.Log($"{gameObject.name} recibió {item} (sin debilidad especial)");
        }
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        // Evitar duplicados del mismo tipo (opcional)
        activeEffects.RemoveAll(e => e.type == effect.type);
        activeEffects.Add(effect);
    }

    public bool HasStatusEffect(StatusEffectType type)
    {
        return activeEffects.Exists(e => e.type == type);
    }

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;

        foreach (var effect in activeEffects)
        {
            if (effect.type == StatusEffectType.Vulnerable)
                multiplier *= effect.strength;
        }

        return multiplier;
    }

    private void RemoveEffect(StatusEffect effect)
    {
        // Aquí puedes poner lógica al terminar un efecto (ej: quitar slow)
    }
}