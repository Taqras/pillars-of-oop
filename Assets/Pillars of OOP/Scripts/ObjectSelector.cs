using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSelector : MonoBehaviour
{
    public static GameObject GetClickedObject() {
        // Single Responsibility Principle: ObjectSelector focuses solely on detecting clicked objects
        // without being tied to any specific gameplay logic.

        if (EventSystem.current.IsPointerOverGameObject()) {
            Debug.Log("...on UI - let Unity handle it");
            return null; // Avoid handling clicks on UI elements
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.Log("Raycasting...");
        
        if (Physics.Raycast(ray, out hit)) {
            Debug.Log("...hit!");
            return hit.collider.gameObject; // Return the GameObject that was clicked on
        } else {
            Debug.Log("...missed");
        }

        return null; // If nothing was hit, return null
    }

}
