using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class intentoMover : MonoBehaviour
{
    public GameObject targetObject; // Objeto que se moverá
    public Vector2 gridSize = new Vector2(10, 10); // Tamaño de la cuadrícula en celdas
    public float cellSize = 1f; // Tamaño de cada celda (asume cuadrada)
    public Vector2Int currentGridPosition = new Vector2Int(0, 0); // Posición actual en la cuadrícula

    private Vector3 gridOrigin; // Origen del plano en el espacio

    void Start()
    {
        // Calcular el origen del plano (la esquina inferior izquierda)
        gridOrigin = new Vector3(-gridSize.x / 2 * cellSize, 0, -gridSize.y / 2 * cellSize);
        MoveToGridPosition(currentGridPosition); // Mover al inicio
    }

    void Update()
    {
        // Detectar entrada para mover el objeto
        if (Input.GetKeyDown(KeyCode.W)) MoveToGridPosition(currentGridPosition + Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) MoveToGridPosition(currentGridPosition + Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) MoveToGridPosition(currentGridPosition + Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) MoveToGridPosition(currentGridPosition + Vector2Int.right);
    }

    void MoveToGridPosition(Vector2Int newPosition)
    {
        // Validar que la posición esté dentro de los límites de la cuadrícula
        if (newPosition.x < 0 || newPosition.x >= gridSize.x || newPosition.y < 0 || newPosition.y >= gridSize.y)
        {
            Debug.Log("La posición está fuera de los límites de la cuadrícula.");
            return;
        }

        // Actualizar la posición actual
        currentGridPosition = newPosition;

        // Calcular la posición en el espacio del mundo
        Vector3 worldPosition = gridOrigin + new Vector3(newPosition.x * cellSize, 0, newPosition.y * cellSize);

        // Mover el objeto
        if (targetObject != null)
        {
            targetObject.transform.position = worldPosition;
        }
    }
}
