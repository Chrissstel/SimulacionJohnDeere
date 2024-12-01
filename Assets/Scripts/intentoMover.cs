using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class intentoMover : MonoBehaviour
{
    public GameObject targetObject; // Objeto que se mover�
    public Vector2 gridSize = new Vector2(10, 10); // Tama�o de la cuadr�cula en celdas
    public float cellSize = 1f; // Tama�o de cada celda (asume cuadrada)
    public Vector2Int currentGridPosition = new Vector2Int(0, 0); // Posici�n actual en la cuadr�cula

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
        // Validar que la posici�n est� dentro de los l�mites de la cuadr�cula
        if (newPosition.x < 0 || newPosition.x >= gridSize.x || newPosition.y < 0 || newPosition.y >= gridSize.y)
        {
            Debug.Log("La posici�n est� fuera de los l�mites de la cuadr�cula.");
            return;
        }

        // Actualizar la posici�n actual
        currentGridPosition = newPosition;

        // Calcular la posici�n en el espacio del mundo
        Vector3 worldPosition = gridOrigin + new Vector3(newPosition.x * cellSize, 0, newPosition.y * cellSize);

        // Mover el objeto
        if (targetObject != null)
        {
            targetObject.transform.position = worldPosition;
        }
    }
}
