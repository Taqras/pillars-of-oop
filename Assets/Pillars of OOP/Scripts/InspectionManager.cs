using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectionManager : MonoBehaviour
{
    public UIManager uiManager; // Assign in Inspector or find at runtime

    private void Awake() {
        if (uiManager == null) {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null) {
                Debug.LogError("UIManager not found in the scene. Make sure there's a UIManager in the scene.");
            }
        }
    }

    public void Inspect(GameObject target) {
        if (target != null) {
            
            IInspectable inspectable = target.GetComponent<IInspectable>();
            if (inspectable != null) {
                // Retrieve information from the model (target object)
                Dictionary<string, string> info = inspectable.GetInfo();

                // Pass the data to the UIManager to display it
                if (uiManager != null) {
                    uiManager.DisplayInfo(info);
                } else {
                    Debug.Log("uiManager is null");
                }

            } else {
                Debug.Log("Target has no IInspectable component");
            }
        } else {
            Debug.Log("Target is null");
        }
    }
}
