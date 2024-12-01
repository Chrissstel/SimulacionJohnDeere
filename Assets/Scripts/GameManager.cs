using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Configuraci�n global
    [Header("Movement Settings")]
    public float globalTimeStep = 1f;  // Tiempo base para todos los agentes
    public float movementBuffer = 0.1f;  // Buffer de separaci�n entre agentes

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

    // Registrar agentes al inicio de la simulaci�n
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

    // M�todo para verificar si una posici�n est� ocupada
    public bool IsPositionAvailable(Vector2 position, float checkTime)
    {
        // Si la posici�n no est� en el diccionario, est� libre
        if (!occupiedPositions.ContainsKey(position))
            return true;

        // Verificar si el tiempo de ocupaci�n ha expirado
        return (checkTime - occupiedPositions[position]) > globalTimeStep;
    }

    // M�todo para marcar una posici�n como ocupada
    public void OccupyPosition(Vector2 position, float occupationTime)
    {
        // Actualizar o agregar la posici�n ocupada
        occupiedPositions[position] = occupationTime;
    }

    // Liberar posiciones antiguas
    private void CleanupOccupiedPositions()
    {
        // Eliminar posiciones ocupadas hace m�s de X tiempo
        occupiedPositions = occupiedPositions
            .Where(x => (currentGlobalTime - x.Value) <= globalTimeStep * 2)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    // M�todo para obtener una posici�n alternativa cercana
    public Vector2 GetAlternativePosition(Vector2 originalPosition, List<Vector2> movementPath)
    {
        // Buscar una posici�n cercana libre
        Vector2[] directions = new Vector2[]
        {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1,1), new Vector2(1,-1), new Vector2(-1,1), new Vector2(-1,-1)
        };

        foreach (Vector2 dir in directions)
        {
            Vector2 newPosition = originalPosition + dir;

            // Verificar si la nueva posici�n est� en el path y disponible
            if (movementPath.Contains(newPosition) && IsPositionAvailable(newPosition, currentGlobalTime))
            {
                return newPosition;
            }
        }

        // Si no hay alternativa, devolver la posici�n original
        return originalPosition;
    }

    // M�todo para reiniciar la simulaci�n
    public void ResetSimulation()
    {
        tractors.Clear();
        trucks.Clear();
        occupiedPositions.Clear();
        currentGlobalTime = 0f;
    }

    // M�todo de depuraci�n para visualizar posiciones ocupadas
    public void DebugOccupiedPositions()
    {
        foreach (var position in occupiedPositions)
        {
            Debug.Log($"Occupied Position: {position.Key} at time {position.Value}");
        }
    }
}
