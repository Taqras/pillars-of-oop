using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable {
// Interface Segregation: Allows objects to define only the behaviors they support,
// ensuring flexibility and reducing unnecessary dependencies.
void Interact();  // Interaction request. the target must decide how to react

}
