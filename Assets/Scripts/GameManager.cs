using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Configuración global
    [Header("Movement Settings")]
    public float globalTimeStep = 1f;  // Tiempo base para todos los agentes
    public float movementBuffer = 0.1f;  // Buffer de separación entre agentes

    // Listas de agentes
    private List<TractorScript> tractors = new List<TractorScript>();
    private List<TruckScript> trucks = new List<TruckScript>();

    // Estados de movimiento
    private Dictionary<Vector2, float> occupiedPositions = new Dictionary<Vector2, float>();
    private float currentGlobalTime = 0f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Incrementar tiempo global
        currentGlobalTime += Time.deltaTime;
    }

    // Registrar agentes al inicio de la simulación
    public void RegisterTractor(TractorScript tractor)
    {
        if (!tractors.Contains(tractor))
        {
            tractors.Add(tractor);
        }
    }

    public void RegisterTruck(TruckScript truck)
    {
        if (!trucks.Contains(truck))
        {
            trucks.Add(truck);
        }
    }

    // Método para verificar si una posición está ocupada
    public bool IsPositionAvailable(Vector2 position, float checkTime)
    {
        // Si la posición no está en el diccionario, está libre
        if (!occupiedPositions.ContainsKey(position))
            return true;

        // Verificar si el tiempo de ocupación ha expirado
        return (checkTime - occupiedPositions[position]) > globalTimeStep;
    }

    // Método para marcar una posición como ocupada
    public void OccupyPosition(Vector2 position, float occupationTime)
    {
        // Actualizar o agregar la posición ocupada
        occupiedPositions[position] = occupationTime;
    }

    // Liberar posiciones antiguas
    private void CleanupOccupiedPositions()
    {
        // Eliminar posiciones ocupadas hace más de X tiempo
        occupiedPositions = occupiedPositions
            .Where(x => (currentGlobalTime - x.Value) <= globalTimeStep * 2)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    // Método para obtener una posición alternativa cercana
    public Vector2 GetAlternativePosition(Vector2 originalPosition, List<Vector2> movementPath)
    {
        // Buscar una posición cercana libre
        Vector2[] directions = new Vector2[]
        {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1,1), new Vector2(1,-1), new Vector2(-1,1), new Vector2(-1,-1)
        };

        foreach (Vector2 dir in directions)
        {
            Vector2 newPosition = originalPosition + dir;

            // Verificar si la nueva posición está en el path y disponible
            if (movementPath.Contains(newPosition) && IsPositionAvailable(newPosition, currentGlobalTime))
            {
                return newPosition;
            }
        }

        // Si no hay alternativa, devolver la posición original
        return originalPosition;
    }

    // Método para reiniciar la simulación
    public void ResetSimulation()
    {
        tractors.Clear();
        trucks.Clear();
        occupiedPositions.Clear();
        currentGlobalTime = 0f;
    }

    // Método de depuración para visualizar posiciones ocupadas
    public void DebugOccupiedPositions()
    {
        foreach (var position in occupiedPositions)
        {
            Debug.Log($"Occupied Position: {position.Key} at time {position.Value}");
        }
    }
}
