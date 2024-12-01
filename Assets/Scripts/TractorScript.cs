using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorScript : MonoBehaviour
{
    public List<Vector2> movementPath; 
    private int currentPathIndex = 0; 
    public int grassCollected = 0; 
    public TerrainScript terrainScript; 

    public float moveSpeed = 2f; 

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyLength;
    private float startTime;
    private bool isMoving = false;

    private Vector3 planeCenter;
    private Vector3 planeSize;
    private float gridWidth;
    private float gridHeight;

    // New light-related variables
    public Light leftHeadlight;   
    public Light rightHeadlight;  
    private bool areLightsOn = false;  

    // Nuevos campos para manejar el tiempo
    public float timePerPosition = 1f; 
    private float currentPositionTime = 0f;
    private bool isWaitingAtPosition = false;

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float rotationStartTime;
    public float rotationSpeed = 2f; 

    private void Start()
    {
        if (PlayerPrefs.GetInt("sizeX") == 0 || PlayerPrefs.GetInt("sizeY") == 0)
        {
            Debug.Log("Grid size not set correctly in PlayerPrefs!");
        }

        TurnHeadlightsOff();

        if (terrainScript != null)
        {
            GameObject plane = terrainScript.plane;
            planeSize = new Vector3(
                plane.transform.localScale.x * 10,
                plane.transform.localScale.y,
                plane.transform.localScale.z * 10
            );
            planeCenter = plane.transform.position;
            gridWidth = PlayerPrefs.GetInt("sizeX");
            gridHeight = PlayerPrefs.GetInt("sizeY");

            // Iniciar en la primera posición de la ruta
            if (movementPath.Count > 0)
            {
                // Calcular la posición inicial exacta basada en la primera coordenada de la ruta
                Vector2 firstGridPosition = movementPath[0];
                transform.position = new Vector3(
                    planeCenter.x - (planeSize.x / 2f) + (firstGridPosition.x * planeSize.x / gridWidth),
                    0.0f, // Altura inicial del tractor
                    planeCenter.z - (planeSize.z / 2f) + (firstGridPosition.y * planeSize.z / gridHeight)
                );

                // Preparar el primer movimiento
                currentPathIndex = 0;
                SetNextTargetPosition();
            }
        }
        else
        {
            Debug.LogError("TerrainScript no está asignado en TractorScript.");
        }
    }

    private void SetNextTargetPosition()
    {

        // Obtener la siguiente posición de la ruta
        Vector2 nextGridPosition = movementPath[currentPathIndex];

        // Verificar disponibilidad de la posición
        if (GameManager.Instance.IsPositionAvailable(nextGridPosition, Time.time))
        {
            // Proceder con el movimiento normalmente
            GameManager.Instance.OccupyPosition(nextGridPosition, Time.time);
        }
        else
        {
            // Obtener una posición alternativa
            nextGridPosition = GameManager.Instance.GetAlternativePosition(nextGridPosition, movementPath);
        }




        if (currentPathIndex >= movementPath.Count) return;

        startPosition = transform.position;
        // Convertir la siguiente posición de la cuadrícula a coordenadas del mundo
        Vector2 gridPosition = movementPath[currentPathIndex];

        // Calcular la posición objetivo
        targetPosition = new Vector3(
            planeCenter.x - (planeSize.x / 2f) + (gridPosition.x * planeSize.x / gridWidth),
            startPosition.y, // Mantener la misma altura
            planeCenter.z - (planeSize.z / 2f) + (gridPosition.y * planeSize.z / gridHeight)
        );


        // Verificar si la posición es repetida
        if (targetPosition == startPosition)
        {
            // Marcar para esperar en esta posición
            isWaitingAtPosition = true;
            currentPositionTime = 0f;
        }
        else
        {
            // Movimiento normal
            isWaitingAtPosition = false;

            // Calcular dirección y rotación (tu código existente)
            Vector3 movementDirection = (targetPosition - startPosition).normalized;
            if (movementDirection != Vector3.zero)
            {
                startRotation = transform.rotation;
                targetRotation = Quaternion.LookRotation(movementDirection);
                rotationStartTime = Time.time;
            }

            journeyLength = Vector3.Distance(startPosition, targetPosition);
            startTime = Time.time;
            isMoving = true;
        }
    }

    private void Update()
    {
        if (isWaitingAtPosition)
        {
            // Incrementar el tiempo de espera
            currentPositionTime += Time.deltaTime;

            // Si ha pasado el tiempo de espera
            if (currentPositionTime >= timePerPosition)
            {
                isWaitingAtPosition = false;
                currentPathIndex++;

                // Preparar la siguiente posición si está disponible
                if (currentPathIndex < movementPath.Count)
                {
                    SetNextTargetPosition();
                }
            }
        }
        else
        {
            MoveToNextPosition();
        }

        // Rotación suave
        if (currentPathIndex < movementPath.Count)
        {
            float rotationProgress = (Time.time - rotationStartTime) * rotationSpeed;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationProgress);
        }
    }

    private void MoveToNextPosition()
    {
        if (currentPathIndex < movementPath.Count && isMoving)
        {
            // Calcular la fracción del viaje completado
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            // Interpolar suavemente entre la posición inicial y el objetivo
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            // Cuando se alcanza el objetivo
            if (fractionOfJourney >= 1f)
            {
                transform.position = targetPosition; // Asegurar posición exacta
                currentPathIndex++;
                isMoving = false;

                // Preparar la siguiente posición si está disponible
                if (currentPathIndex < movementPath.Count)
                {
                    SetNextTargetPosition();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grass"))
        {
            StartCoroutine(DestroyGrassWithDelay(other.gameObject));
        }
    }

    private IEnumerator DestroyGrassWithDelay(GameObject grassObject)
    {
        // Opcional: puedes cambiar el color o hacer alguna animación antes de destruir
        Renderer grassRenderer = grassObject.GetComponent<Renderer>();
        if (grassRenderer != null)
        {
            grassRenderer.material.color = Color.red; // Cambiar a rojo para indicar que será destruido
        }

        // Esperar 1 segundo
        yield return new WaitForSeconds(1f);

        // Aumentar contador de grass recolectado
        grassCollected++;

        // Destruir el objeto de grass
        Destroy(grassObject);
    }

    public void ToggleHeadlights()
    {
        areLightsOn = !areLightsOn;

        if (leftHeadlight != null)
            leftHeadlight.enabled = areLightsOn;

        if (rightHeadlight != null)
            rightHeadlight.enabled = areLightsOn;
    }

    public void TurnHeadlightsOn()
    {
        areLightsOn = true;

        if (leftHeadlight != null)
            leftHeadlight.enabled = true;

        if (rightHeadlight != null)
            rightHeadlight.enabled = true;
    }

    public void TurnHeadlightsOff()
    {
        areLightsOn = false;

        if (leftHeadlight != null)
            leftHeadlight.enabled = false;

        if (rightHeadlight != null)
            rightHeadlight.enabled = false;
    }

}