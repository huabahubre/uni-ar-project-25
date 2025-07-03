using UnityEngine;
using TMPro;

public class Copy : MonoBehaviour
{
    [SerializeField] private TMP_Text displayCode;
    
    public void OnCopyButtonClicked()
    {
        Debug.Log("Copy Button Clicked");
        if (displayCode == null || string.IsNullOrEmpty(displayCode.text))
        {
            Debug.Log("No code to copy");
            return;
        }

        GUIUtility.systemCopyBuffer = displayCode.text;
        Debug.Log("Copied to clipboard: " + displayCode.text);
    }
}
