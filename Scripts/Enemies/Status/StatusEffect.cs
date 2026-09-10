using UnityEngine;

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public float duration = 3f;
    public float strength = 1f;          // Multiplicador o intensidad
    public float remainingTime;

    public StatusEffect(StatusEffectType type, float duration, float strength = 1f)
    {
        this.type = type;
        this.duration = duration;
        this.strength = strength;
        this.remainingTime = duration;
    }

    public bool IsExpired => remainingTime <= 0f;

    public void Tick(float deltaTime)
    {
        remainingTime -= deltaTime;
    }
}