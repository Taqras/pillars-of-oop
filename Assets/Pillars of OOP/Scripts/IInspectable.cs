using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InspectionKey
{
    Name,
    Description
}
public interface IInspectable {
// Interface Segregation: Allows objects to define only the behaviors they support,
// ensuring flexibility and reducing unnecessary dependencies.
    Dictionary<string, string> GetInfo();
}