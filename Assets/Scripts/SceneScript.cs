using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SceneScript : MonoBehaviour
{
    public TMP_InputField inputFieldX;
    public TMP_InputField inputFieldY;
    public TMP_InputField inputFieldObs;
    public TMP_Dropdown tractoresField;
    public Text grassCollectedText;
    public Button playButton;
    public GameObject panel;

    public GameObject plane;

    public TerrainScript terrainScript;

    public CameraMovementScript mainCamera; // Referencia a tu script de cámara

    [SerializeField] private GameObject tractorPrefab;
    [SerializeField] private GameObject truckPrefab;
    private List<TractorScript> instantiatedTractors = new List<TractorScript>();

    void Start()
    {
        playButton.onClick.AddListener(StartSimulation);
    }

    public void StartSimulation()
    {
        StartCoroutine(CallSimulationAPI());
    }

    private IEnumerator CallSimulationAPI()
    {
        // Validar y obtener valores de los InputFields
        if (int.TryParse(inputFieldX.text, out int sizeX) &&
            int.TryParse(inputFieldY.text, out int sizeY) &&
            int.TryParse(inputFieldObs.text, out int numObs))
        {
            int numTractores = tractoresField.value+1;
            PlayerPrefs.SetInt("numTractores", numTractores);
            PlayerPrefs.SetInt("numObstaculos", numObs);
            PlayerPrefs.SetInt("sizeX", sizeX);
            PlayerPrefs.SetInt("sizeY", sizeY);

            string url = "http://127.0.0.1:5000/start-simulation";
            Debug.Log(sizeX + " " + sizeY + " " + numTractores + " " + numObs);
            SimulationPayload payload = new SimulationPayload
            {
                x = sizeX,
                y = sizeY,
                tractores = numTractores,
                pasos = 1200,
                obstaculos = numObs
            };

            // Serialize payload to JSON
            string jsonData = JsonUtility.ToJson(payload);
            Debug.Log("JSON Payload: " + jsonData);

            // Send POST request
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Response: " + request.downloadHandler.text);

                // Decode JSON response
                var responseData = JObject.Parse(request.downloadHandler.text);

                // Convertir posiciones de tractores
                var tractorsPositionsJson = responseData["tractors_positions"].ToString();
                List<List<Vector2>> tractorPaths = ConvertJsonToVector2Paths(tractorsPositionsJson);

                // Convertir posiciones de carritos
                var cartsPositionsJson = responseData["carts_positions"].ToString();
                List<List<Vector2>> cartPaths = ConvertJsonToVector2Paths(cartsPositionsJson);

                // Convertir posiciones de obstáculos
                var obstaclePositionsJson = responseData["posiciones_de_obstaculos"].ToString();
                Debug.Log("Obstacle Positions: " + obstaclePositionsJson);
                Vector2[] obstaclePositions = ConvertJsonToVector2Array(obstaclePositionsJson);

                // Cambiar el tamaño del plano
                if (plane != null)
                {
                    plane.transform.localScale = new Vector3(sizeX, 1, sizeY);
                    Debug.Log($"Plano cambiado a tamaño: {sizeX}x{sizeY}. Selección Dropdown: {numTractores}");
                }

                // Usar estos vectores en el resto de tu código
                terrainScript.PlaceObjects(obstaclePositions);

                // Instanciar objetos
                StartCoroutine(InstantiateVehicles(tractorPaths, cartPaths));
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
        else
        {
            Debug.LogError("Los valores ingresados en los TextFields no son válidos.");
        }
    }

    private IEnumerator InstantiateVehicles(List<List<Vector2>> tractorPaths, List<List<Vector2>> cartPaths)
    {
        // Limpiar la lista de tractores existentes
        instantiatedTractors.Clear();

        // Lista para almacenar los tractores
        List<Transform> tractorTransforms = new List<Transform>();
        List<Transform> truckTransforms = new List<Transform>();

        // Instanciar tractores y asignarles sus rutas
        for (int i = 0; i < tractorPaths.Count; i++)
        {
            Vector2 firstPosition = tractorPaths[i][0];
            Vector3 startPosition = new Vector3(
                terrainScript.plane.transform.position.x - (terrainScript.plane.transform.localScale.x * 5f) + (firstPosition.x * terrainScript.plane.transform.localScale.x * 10f / PlayerPrefs.GetInt("sizeX")),
                0.5f, // Altura inicial del tractor
                terrainScript.plane.transform.position.z - (terrainScript.plane.transform.localScale.z * 5f) + (firstPosition.y * terrainScript.plane.transform.localScale.z * 10f / PlayerPrefs.GetInt("sizeY"))
            );

            GameObject tractor = Instantiate(tractorPrefab, startPosition, Quaternion.identity);
            TractorScript tractorScript = tractor.GetComponent<TractorScript>();
            if (tractorScript != null)
            {
                tractorScript.movementPath = tractorPaths[i]; // Asignar ruta
                tractorScript.terrainScript = terrainScript; // Asegurar que TerrainScript esté asignado

                // Añadir el transform del tractor a la lista de objetivos de la cámara
                tractorTransforms.Add(tractor.transform);

                instantiatedTractors.Add(tractorScript);
                GameManager.Instance.RegisterTractor(tractorScript);
            }
            else
            {
                Debug.LogWarning("El prefab del tractor no tiene el script TractorScript asignado.");
            }

            // Opcional: agregar un pequeño retraso entre instanciaciones
            yield return new WaitForSeconds(0.1f);

            StartCoroutine(UpdateGrassCollectedText());
        }

        // Instanciar carritos y asignarles sus rutas
        for (int i = 0; i < cartPaths.Count; i++)
        {
            Vector2 firstPosition = cartPaths[i][0];
            Vector3 startPosition = new Vector3(
                terrainScript.plane.transform.position.x - (terrainScript.plane.transform.localScale.x * 5f) + (firstPosition.x * terrainScript.plane.transform.localScale.x * 10f / PlayerPrefs.GetInt("sizeX")),
                0.5f, // Altura inicial del tractor
                terrainScript.plane.transform.position.z - (terrainScript.plane.transform.localScale.z * 5f) + (firstPosition.y * terrainScript.plane.transform.localScale.z * 10f / PlayerPrefs.GetInt("sizeY"))
            );

            GameObject truck = Instantiate(truckPrefab, startPosition, Quaternion.identity);
            TruckScript truckScript = truck.GetComponent<TruckScript>();
            if (truckScript != null)
            {
                truckScript.movementPath = cartPaths[i]; // Asignar ruta
                truckScript.terrainScript = terrainScript; // Asegurar que TerrainScript esté asignado

                // Añadir el transform del tractor a la lista de objetivos de la cámara
                truckTransforms.Add(truck.transform);

                GameManager.Instance.RegisterTruck(truckScript);

            }
            else
            {
                Debug.LogWarning("El prefab del truck no tiene el script TruckScript asignado.");
            }

            // Opcional: agregar un pequeño retraso entre instanciaciones
            yield return new WaitForSeconds(0.1f);
        }

        // Configurar la cámara
        if (mainCamera != null)
        {
            mainCamera.targets = tractorTransforms;
            mainCamera.trucks = truckTransforms;
        }

        // Ocultar el panel
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // Método para convertir JSON a lista de rutas de Vector2
    private List<List<Vector2>> ConvertJsonToVector2Paths(string jsonString)
    {
        var paths = JArray.Parse(jsonString);
        var vector2Paths = new List<List<Vector2>>();

        foreach (var path in paths)
        {
            var currentPath = new List<Vector2>();
            foreach (var point in path)
            {
                float x = point[0].Value<float>();
                float y = point[1].Value<float>();
                Debug.Log($"Parsed Point: ({x}, {y})");
                currentPath.Add(new Vector2(x, y));
            }
            vector2Paths.Add(currentPath);
        }

        return vector2Paths;
    }

    // Método para convertir JSON a array de Vector2
    private Vector2[] ConvertJsonToVector2Array(string jsonString)
    {
        var pointsArray = JArray.Parse(jsonString);
        var vector2Array = new Vector2[pointsArray.Count];

        for (int i = 0; i < pointsArray.Count; i++)
        {
            float x = pointsArray[i][0].Value<float>();
            float y = pointsArray[i][1].Value<float>();
            vector2Array[i] = new Vector2(x, y);
        }

        return vector2Array;
    }

    private IEnumerator UpdateGrassCollectedText()
    {
        while (true)
        {
            int totalGrassCollected = 0;
            foreach (var tractor in instantiatedTractors)
            {
                totalGrassCollected += tractor.grassCollected;
            }

            if (grassCollectedText != null)
            {
                grassCollectedText.text = totalGrassCollected.ToString();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

}
