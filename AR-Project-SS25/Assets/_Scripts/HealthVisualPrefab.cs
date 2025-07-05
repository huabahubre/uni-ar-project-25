using Sirenix.OdinInspector;
using UnityEngine;

public class HealthVisualPrefab : MonoBehaviour
{
    [BoxGroup("References")] public GameObject hoverParent;

    [BoxGroup("Hover Settings")]
    public float hoverAmplitude = 0.2f; // How high it moves up and down

    [BoxGroup("Hover Settings")]
    public float hoverFrequency = 1f; // Speed of the hover

    [BoxGroup("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up; // Default Y-axis rotation

    [BoxGroup("Rotation Settings")]
    public float rotationSpeed = 30f; // Degrees per second

    private Vector3 initialPosition;
    private ElementVisualData elementVisualData;


    public void Init(bool isLocalPlayer, ElementType elementType)
    {
        if (hoverParent == null)
        {
            Debug.LogError("Hover Parent is not assigned.");
            return;
        }

        // Set the initial position for hover effect
        initialPosition = hoverParent.transform.localPosition;

        // Get the visual data for the element type
        elementVisualData = DataManagement.Instance.GetElementVisualData(elementType);
        if (elementVisualData == null)
        {
            Debug.LogError($"No visual data found for element type: {elementType}");
            return;
        }

        // Set the visual prefab
        GameObject visualPrefab = elementVisualData.CrystalPrefab;
        if (visualPrefab != null)
        {
            Instantiate(visualPrefab, hoverParent.transform);
        }
    }
    
    
    private void Start()
    {
        if (hoverParent != null)
            initialPosition = hoverParent.transform.localPosition;
    }

    private void Update()
    {
        if (hoverParent == null) return;

        // Hover effect
        float hoverOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        Vector3 newPosition = initialPosition + new Vector3(0f, hoverOffset, 0f);
        hoverParent.transform.localPosition = newPosition;

        // Rotate effect
        hoverParent.transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
    }
}