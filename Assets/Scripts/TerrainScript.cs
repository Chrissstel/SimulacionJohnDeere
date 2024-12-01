using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainScript : MonoBehaviour
{
    // Referencia al plano
    [SerializeField] public GameObject plane;

    // Prefab del objeto a instanciar
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject fillPrefab;

    /// <summary>
    /// Coloca objetos en las posiciones especificadas sobre el plano.
    /// </summary>
    /// <param name="positions">Arreglo de posiciones (Vector2) en el plano</param>
    public void PlaceObjects(Vector2[] obstaclePositions)
    {
        if (plane == null || obstaclePrefab == null || fillPrefab == null)
        {
            Debug.LogError("Asegúrate de asignar el plano y el prefab en el inspector.");
            return;
        }

        // Tamaño físico del plano
        Vector3 planeSize = new Vector3(
            plane.transform.localScale.x * 10,
            plane.transform.localScale.y,
            plane.transform.localScale.z * 10
        );

        // Coordenadas base del plano (centro como origen)
        Vector3 planeCenter = plane.transform.position;

        float gridWidth = PlayerPrefs.GetInt("sizeX");
        float gridHeight = PlayerPrefs.GetInt("sizeY");

        HashSet<Vector2> occupiedPositions = new HashSet<Vector2>(obstaclePositions);

        // Iterar por todas las posiciones del grid
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 gridPosition = new Vector2(x, y);

                // Mapear posición de la cuadrícula al espacio mundial
                Vector3 worldPosition = new Vector3(
                    planeCenter.x - (planeSize.x / 2f) + (gridPosition.x * planeSize.x / gridWidth), // X
                    planeCenter.y, // Y (altura fija)
                    planeCenter.z - (planeSize.z / 2f) + (gridPosition.y * planeSize.z / gridHeight) // Z
                );

                // Verificar si la posición está ocupada por un obstáculo
                if (occupiedPositions.Contains(gridPosition))
                {
                    // Instanciar un obstáculo
                    Instantiate(obstaclePrefab, worldPosition, Quaternion.identity);
                }
                else
                {
                    // Instanciar el objeto de relleno
                    Instantiate(fillPrefab, worldPosition, Quaternion.identity);
                }
            }
        }
    }
}
