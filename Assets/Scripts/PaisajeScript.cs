using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaisajeScript : MonoBehaviour
{
    [Header("Terrain Reference")]
    public Terrain terrain; // Referencia al terreno

    [Header("Tree Prefabs")]
    public List<GameObject> treePrefabs; // Lista de prefabs de �rboles para spawning

    [Header("Spawning Parameters")]
    [Range(0, 1000)]
    public int treeCount = 100; // N�mero de �rboles a spawnar
    [Range(0f, 1f)]
    public float density = 0.5f; // Densidad de �rboles (afecta probabilidad de spawn)

    [Header("Spawn Area Restrictions")]
    public float centerClearRadius = 10f; // Radio desde el centro del terreno que quedar� libre de �rboles
    public Vector3 centerPoint; // Punto central para el radio libre (opcional)

    [Header("Tree Placement Options")]
    public float minTreeScale = 0.8f; // Escala m�nima aleatoria
    public float maxTreeScale = 1.2f; // Escala m�xima aleatoria
    public float maxSlopeAngle = 45f; // �ngulo m�ximo de pendiente donde se pueden spawnar �rboles

    private void Start()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }

        if (centerPoint == Vector3.zero)
        {
            // Si no se especifica, usar el centro del terreno
            centerPoint = terrain.transform.position + terrain.terrainData.size / 2;
        }

        SpawnTrees();
    }

    void SpawnTrees()
    {
        // Limpiar �rboles existentes
        terrain.terrainData.treeInstances = new TreeInstance[0];

        // Obtener datos del terreno
        TerrainData terrainData = terrain.terrainData;
        float width = terrainData.size.x;
        float height = terrainData.size.z;

        // Spawning de �rboles
        for (int i = 0; i < treeCount; i++)
        {
            // Saltar si no pasa la densidad
            if (Random.value > density) continue;

            // Generar posici�n random en el terreno
            Vector3 randomPosition = new Vector3(
                Random.Range(0f, width),
                0,
                Random.Range(0f, height)
            );

            // Obtener altura del terreno en esta posici�n
            float terrainHeight = terrain.SampleHeight(randomPosition);
            randomPosition.y = terrainHeight;

            // Convertir a coordenadas normalizadas del terreno (0-1)
            Vector3 normalizedPos = new Vector3(
                randomPosition.x / terrainData.size.x,
                randomPosition.y / terrainData.size.y,
                randomPosition.z / terrainData.size.z
            );

            // Verificar distancia desde el centro
            Vector3 worldPos = terrain.transform.position + randomPosition;
            float distanceFromCenter = Vector3.Distance(worldPos, centerPoint);
            if (distanceFromCenter < centerClearRadius) continue;

            // Verificar �ngulo de pendiente
            Vector3 normal = terrain.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.z);
            float slope = Vector3.Angle(normal, Vector3.up);
            if (slope > maxSlopeAngle) continue;

            // Seleccionar un �rbol random de la lista
            if (treePrefabs.Count == 0) continue;
            GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Count)];

            // Crear instancia de �rbol
            TreeInstance treeInstance = new TreeInstance
            {
                position = normalizedPos,
                widthScale = Random.Range(minTreeScale, maxTreeScale),
                heightScale = Random.Range(minTreeScale, maxTreeScale),
                color = Color.white,
                lightmapColor = Color.white
            };

            // A�adir instancia de �rbol
            terrain.AddTreeInstance(treeInstance);
        }
    }

    // M�todo para regenerar �rboles (�til para pruebas)
    [ContextMenu("Regenerate Trees")]
    public void RegenerateTrees()
    {
        SpawnTrees();
    }

    // Dibujar gizmo para visualizar el radio libre de �rboles en el editor
    private void OnDrawGizmosSelected()
    {
        if (terrain != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centerPoint, centerClearRadius);
        }
    }
}
