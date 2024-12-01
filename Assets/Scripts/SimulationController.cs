using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.UI; // For Button
using Newtonsoft.Json.Linq;


[System.Serializable]
public class SimulationPayload
{
    public int x;
    public int y;
    public int tractores;
    public int pasos;
    public int obstaculos;
}

public class SimulationController : MonoBehaviour
{
    // UI References (TMP Input Fields)
    public TMP_InputField xField;
    public TMP_InputField yField;
    //public TMP_InputField tractoresField;
    public TMP_Dropdown tractoresField;

    //public TMP_InputField pasosField;
    public TMP_InputField obstaculosField;

    // TMP Button reference (same as regular Button in Unity)
    public Button startButton;

    private void Start()
    {
        // Add a listener to the button
        startButton.onClick.AddListener(StartSimulation);
    }

    public void StartSimulation()
    {
        StartCoroutine(CallSimulationAPI());
    }

    private IEnumerator CallSimulationAPI()
    {
        // Log the raw input field values for debugging
        Debug.Log($"x: {xField.text}, y: {yField.text}, tractores: {tractoresField.value}, obstaculos: {obstaculosField.text}");
        // Parse values from the TMP input fields
        if (!int.TryParse(xField.text, out int x) ||
            !int.TryParse(yField.text, out int y) ||
            //!int.TryParse(tractoresField.value, out int tractores) ||
            //!int.TryParse(pasosField.text, out int pasos) ||
            !int.TryParse(obstaculosField.text, out int obstaculos))
        {
            Debug.LogError("Invalid input in one or more fields!");
            yield break;
        }

        string url = "http://127.0.0.1:5000/start-simulation";

        int tractores = tractoresField.value;
        // Create payload instance
        SimulationPayload payload = new SimulationPayload
        {
            x = x,
            y = y,
            tractores = tractores,
            pasos = 1200,
            obstaculos = obstaculos
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
            var tractorsPositions = responseData["tractors_positions"];
            var cartsPositions = responseData["carts_positions"];
            var posicionesDeObstaculos = responseData["posiciones_de_obstaculos"];

            Debug.Log("Tractors Positions: " + tractorsPositions);
            Debug.Log("Carts Positions: " + cartsPositions);
            Debug.Log("Obstacle Positions: " + posicionesDeObstaculos);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
}