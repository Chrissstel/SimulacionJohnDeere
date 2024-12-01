using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraMovementScript : MonoBehaviour
{
    public float moveSpeed = 10f;       // Speed of camera movement
    public float rotationSpeed = 50f;  // Speed of smooth rotation
    public float zoomSpeed = 10f;      // Speed of zooming
    public float minZoom = 5f;         // Minimum zoom distance
    public float maxZoom = 50f;        // Maximum zoom distance
    public float downwardAngle = 45f;  // Fixed downward angle (X-axis rotation)

    private float targetYRotation = 0f; // Target Y rotation for the camera
    private float targetZoom;           // Target zoom level (distance from the object)

    public Light mainSceneLight; // Referencia a la luz principal de la escena
    public float mainLightReducedIntensity = 0.2f; // Intensidad reducida de la luz principal
    private float originalMainLightIntensity = 1;
    private bool areTractorLightsOn = false;
    public List<Transform> targets; //los tractores
    public List<Transform> trucks; //los carritos

    private float globalDeltaTime;

    private void Start()
    {
        // Initialize the target Y rotation and zoom level
        targetYRotation = transform.eulerAngles.y;
        targetZoom = transform.position.y;

        // Set the fixed downward angle
        Vector3 rotation = transform.eulerAngles;
        rotation.x = downwardAngle;
        transform.eulerAngles = rotation;

        //para la luz
        originalMainLightIntensity = 1.0f;
    }

    private void Update()
    {
        globalDeltaTime = GameManager.Instance != null
            ? GameManager.Instance.globalTimeStep * Time.deltaTime
            : Time.deltaTime;

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

        HandleRotation();
        HandleZoom();
        MoveCamera();

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
            foreach (Transform truckTransform in trucks)
            {
                TruckScript truckScript = truckTransform.GetComponent<TruckScript>();
                if (truckScript != null)
                {
                    truckScript.TurnHeadlightsOn();
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
            foreach (Transform truckTransform in trucks)
            {
                TruckScript truckScript = truckTransform.GetComponent<TruckScript>();
                if (truckScript != null)
                {
                    truckScript.TurnHeadlightsOff();
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

        foreach (Transform truckTransform in trucks)
        {
            TruckScript truckScript = truckTransform.GetComponent<TruckScript>();
            if (truckScript != null)
            {
                // Si ya están encendidas, apagarlas. Si están apagadas, encenderlas
                if (areTractorLightsOn)
                {
                    truckScript.TurnHeadlightsOff();
                }
                else
                {
                    truckScript.TurnHeadlightsOn();
                }
            }
        }

        // Alternar el estado
        areTractorLightsOn = !areTractorLightsOn;
    }

    private void HandleRotation()
    {
        // Adjust the target Y rotation based on input
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            targetYRotation -= rotationSpeed * globalDeltaTime; // Rotate counterclockwise
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            targetYRotation += rotationSpeed * globalDeltaTime; // Rotate clockwise
        }

        // Smoothly rotate the camera around the Y-axis
        Quaternion targetRotation = Quaternion.Euler(downwardAngle, targetYRotation, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, globalDeltaTime * 5f);
    }

    private void HandleZoom()
    {
        // Adjust the zoom level based on input
        if (Input.GetKey(KeyCode.UpArrow))
        {
            targetZoom -= zoomSpeed * globalDeltaTime; // Zoom in
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            targetZoom += zoomSpeed * globalDeltaTime; // Zoom out
        }

        // Clamp the zoom level between minZoom and maxZoom
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        // Smoothly move the camera along the Y-axis for zoom
        Vector3 position = transform.position;
        position.y = Mathf.Lerp(position.y, targetZoom, globalDeltaTime * 5f);
        transform.position = position;
    }

    private void MoveCamera()
    {
        // Get input for movement
        float moveForward = Input.GetAxis("Vertical") * moveSpeed * globalDeltaTime;
        float moveRight = Input.GetAxis("Horizontal") * moveSpeed * globalDeltaTime;

        // Move the camera relative to its current rotation
        Vector3 forward = transform.forward * moveForward;
        Vector3 right = transform.right * moveRight;

        // Only move along the XZ plane (ignore Y component)
        forward.y = 0;
        right.y = 0;

        // Apply movement
        transform.position += forward + right;
    }
}
