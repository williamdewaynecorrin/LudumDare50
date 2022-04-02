using UnityEngine;
using System;
using System.Collections;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute
{
    public string conditionalSourceField;
    public int enumIndex;
    public int[] enumIndices;
    public bool boolValue;

    public ConditionalHideAttribute(string boolVariableName, bool boolValue)
    {
        conditionalSourceField = boolVariableName;
        this.boolValue = boolValue;
    }

    public ConditionalHideAttribute(string enumVariableName, int enumIndex)
    {
        conditionalSourceField = enumVariableName;
        this.enumIndex = enumIndex;
    }

    public ConditionalHideAttribute(string enumVariableName, int[] enumIndices)
    {
        conditionalSourceField = enumVariableName;
        this.enumIndices = enumIndices;
    }
}
