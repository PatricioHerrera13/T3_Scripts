using UnityEngine;

public interface IChargePattern
{
    /// <summary>
    /// Calcula el recorrido del dash.
    /// Se llama al inicio del Windup.
    /// </summary>
    void CalculatePath(
        EnemyCore core,
        Vector3 currentPosition,
        Vector3 playerPosition,
        out Vector3 dashStart,
        out Vector3 dashEnd,
        out Vector3 dashDirection
    );
}