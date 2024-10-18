using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InspectionKey
{
    Name,
    Description
}
public interface IInspectable
{
    Dictionary<string, string> GetInfo();
}