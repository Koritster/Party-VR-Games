using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartBoard : MonoBehaviour
{
    public Transform[] scoringZones; // Asigna los colliders de las zonas de puntuación

    public int CalculateScore(Vector3 hitPoint)
    {
        // Lógica para calcular el puntaje basado en la posición del impacto
        // Esto es un ejemplo simplificado
        float distanceFromCenter = Vector3.Distance(hitPoint, transform.position);

        if (distanceFromCenter < 0.05f) return 50; // Centro
        if (distanceFromCenter < 0.15f) return 25; // Anillo exterior
        if (distanceFromCenter < 0.3f) return 10;  // Zona media
        return 5; // Zona externa
    }
}
