using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public List<Transform> targets; // El objeto al que la cámara seguirá
    public float orbitSpeed = 50f; // Velocidad de movimiento orbital (izquierda/derecha)
    public float zoomSpeed = 10f; // Velocidad de acercamiento/alejamiento
    public float minDistance = 2f; // Distancia mínima al objeto
    public float maxDistance = 20f; // Distancia máxima al objeto
    public float verticalLookOffset = 1.5f;

    public Transform referencePlane;

    private int currentTargetIndex = 0; // Índice del objetivo actual
    private float currentDistance; // Distancia actual al objeto
    private Vector3 offset; // Desplazamiento relativo al objetivo
    private bool isTopDownView = false;
    private bool isCornerView = false;

    public Light mainSceneLight; // Referencia a la luz principal de la escena
    public float mainLightReducedIntensity = 0.2f; // Intensidad reducida de la luz principal
    private float originalMainLightIntensity = 1;
    private bool areTractorLightsOn = false;


    void Start()
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("Asigna un objeto al 'target' en el inspector.");
            return;
        }

        SetTarget(targets[currentTargetIndex]);

        // Guardar la intensidad original de la luz principal
        if (mainSceneLight != null)
        {
            //originalMainLightIntensity = mainSceneLight.intensity;
            originalMainLightIntensity = 1.0f;
        }
    }

    void Update()
    {
        if (targets == null || targets.Count == 0 || targets[currentTargetIndex] == null) return;

        // Cambiar de objetivo al presionar Espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchToNextTarget();
        }

        // Nuevo: Cambiar a vista desde arriba con la tecla T
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTopDownView();
        }

        // Nuevo: Cambiar a vista de esquina con la tecla Y
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ToggleCornerView();
        }

        // Nuevo: Controlar luces con la tecla N
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleNightMode();
        }

        // Nuevo: Encender luces manualmente con la M
        if (Input.GetKeyDown(KeyCode.M))
        {
            TurnOnTractorLights();
        }

        // Rotación alrededor del objeto
        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal != 0)
        {
            transform.RotateAround(targets[currentTargetIndex].position, Vector3.up, horizontal * orbitSpeed * Time.deltaTime);
            offset = transform.position - targets[currentTargetIndex].position;
        }

        // Zoom (acercar/alejar)
        float vertical = Input.GetAxis("Vertical");
        if (vertical != 0)
        {
            currentDistance = Mathf.Clamp(currentDistance - vertical * zoomSpeed * Time.deltaTime, minDistance, maxDistance);
            offset = offset.normalized * currentDistance;
            transform.position = targets[currentTargetIndex].position + offset;
        }

        // Lógica de enfoque condicional
        if (isTopDownView)
        {
            // Si se especificó un plano de referencia, usar su posición
            Vector3 lookTarget = referencePlane != null
                ? referencePlane.position
                : targets[currentTargetIndex].position;

            // Posicionar la cámara directamente arriba
            Vector3 topDownPosition = new Vector3(
                lookTarget.x,
                lookTarget.y + currentDistance,
                lookTarget.z
            );

            transform.position = topDownPosition;
            transform.LookAt(lookTarget);
        }
        else if (isCornerView)
        {
            // Nueva vista de esquina
            SetCornerView();
        }
        else
        {
            // Vista normal con offset vertical
            Vector3 lookAtPosition = targets[currentTargetIndex].position + new Vector3(0, verticalLookOffset, 0);
            transform.LookAt(lookAtPosition);
        }
    }

    void SetTarget(Transform newTarget)
    {
        if (newTarget == null) return;

        // Calcular desplazamiento inicial
        offset = transform.position - newTarget.position;
        currentDistance = offset.magnitude;
    }

    void SwitchToNextTarget()
    {
        // Incrementar índice y reiniciar si es necesario
        currentTargetIndex = (currentTargetIndex + 1) % targets.Count;
        SetTarget(targets[currentTargetIndex]);
    }

    void ToggleTopDownView()
    {
        isTopDownView = !isTopDownView;
        Debug.Log(isTopDownView ? "Top-Down View Activated" : "Normal View Restored");
    }

    void ToggleCornerView()
    {
        isCornerView = !isCornerView;
        isTopDownView = false; // Desactivar vista desde arriba
        Debug.Log(isCornerView ? "Corner View Activated" : "Normal View Restored");
    }

    void SetCornerView()
    {
        // Verificar si hay un plano de referencia
        if (referencePlane == null)
        {
            Debug.LogWarning("No se ha asignado un plano de referencia. Usando el primer target.");
            return;
        }

        // Obtener los límites del plano (asumiendo que el plano tiene un Renderer)
        Renderer planeRenderer = referencePlane.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogWarning("El plano de referencia no tiene un Renderer para calcular sus límites.");
            return;
        }

        // Calcular las esquinas del plano
        Bounds bounds = planeRenderer.bounds;
        Vector3 minCorner = bounds.min;
        Vector3 maxCorner = bounds.max;

        // Posicionar la cámara en una esquina
        Vector3 cornerPosition = new Vector3(
            minCorner.x - currentDistance,
            maxCorner.y + currentDistance,
            maxCorner.z + currentDistance
        );

        // Posicionar la cámara
        transform.position = cornerPosition;

        // Mirar hacia el centro del plano
        transform.LookAt(bounds.center);
    }

    void ToggleNightMode()
    {
        // Verificar si hay luces principales y targets
        if (mainSceneLight == null || targets == null || targets.Count == 0)
        {
            Debug.LogWarning("No se han asignado luces principales o targets.");
            return;
        }

        // Alternar estado de las luces
        if (mainSceneLight.intensity > mainLightReducedIntensity)
        {
            // Reducir luz principal y encender luces de tractores
            mainSceneLight.intensity = mainLightReducedIntensity;

            // Encender luces de todos los tractores
            foreach (Transform tractorTransform in targets)
            {
                TractorScript tractorScript = tractorTransform.GetComponent<TractorScript>();
                if (tractorScript != null)
                {
                    tractorScript.TurnHeadlightsOn();
                }
            }
            areTractorLightsOn = true;
        }
        else
        {
            // Restaurar luz principal y apagar luces de tractores
            mainSceneLight.intensity = originalMainLightIntensity;

            // Apagar luces de todos los tractores
            foreach (Transform tractorTransform in targets)
            {
                TractorScript tractorScript = tractorTransform.GetComponent<TractorScript>();
                if (tractorScript != null)
                {
                    tractorScript.TurnHeadlightsOff();
                }
            }
            areTractorLightsOn = false;
        }
    }

    void TurnOnTractorLights()
    {
        // Verificar si hay targets
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("No se han asignado targets.");
            return;
        }

        // Encender luces de todos los tractores
        foreach (Transform tractorTransform in targets)
        {
            TractorScript tractorScript = tractorTransform.GetComponent<TractorScript>();
            if (tractorScript != null)
            {
                // Si ya están encendidas, apagarlas. Si están apagadas, encenderlas
                if (areTractorLightsOn)
                {
                    tractorScript.TurnHeadlightsOff();
                }
                else
                {
                    tractorScript.TurnHeadlightsOn();
                }
            }
        }

        // Alternar el estado
        areTractorLightsOn = !areTractorLightsOn;
    }
}

