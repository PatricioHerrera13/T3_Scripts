public enum StatusEffectType
{
    None = 0,
    Vulnerable,     // Recibe más daño
    Stunned,        // No se puede mover ni atacar
    Slowed,         // Se mueve más lento
    Marked,         // El proyectil del player le hace más daño
    Burning,        // Daño por tiempo
    Frozen,
    Execute         // Lo mata si tiene poca vida
}