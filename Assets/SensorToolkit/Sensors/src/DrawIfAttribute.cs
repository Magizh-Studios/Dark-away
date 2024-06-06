using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Micosmo.SensorToolkit {

    /// <summary>
    /// Draws the field/property ONLY if the compared property compared by the comparison type with the value of comparedValue returns true.
    /// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    internal class DrawIfAttribute : PropertyAttribute {
        #region Fields

        public string comparedPropertyName { get; private set; }
        public object comparedValue { get; private set; }
        public DisablingType disablingType { get; private set; }

        /// <summary>
        /// Types of comperisons.
        /// </summary>
        public enum DisablingType {
            ReadOnly = 2,
            DontDraw = 3
        }

        #endregion

        /// <summary>
        /// Only draws the field only if a condition is met. Supports enum and bools.
        /// </summary>
        /// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
        /// <param name="comparedValue">The value the property is being compared to.</param>
        /// <param name="disablingType">The type of disabling that should happen if the condition is NOT met. Defaulted to DisablingType.DontDraw.</param>
        public DrawIfAttribute(string comparedPropertyName, object comparedValue, DisablingType disablingType = DisablingType.DontDraw) {
            this.comparedPropertyName = comparedPropertyName;
            this.comparedValue = comparedValue;
            this.disablingType = disablingType;
        }
    }

}

#if UNITY_EDITOR
namespace Micosmo.SensorToolkit.Editors {

    /// <summary>
    /// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
    /// </summary>
    [CustomPropertyDrawer(typeof(DrawIfAttribute))]
    public class DrawIfPropertyDrawer : PropertyDrawer {
        #region Fields

        // Reference to the attribute on the property.
        DrawIfAttribute drawIf;

        // Field that is being compared.
        SerializedProperty comparedField;

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!ShowMe(property) && drawIf.disablingType == DrawIfAttribute.DisablingType.DontDraw) {
                return -EditorGUIUtility.standardVerticalSpacing;
            } else {
                if (property.propertyType == SerializedPropertyType.Generic) {
                    int numChildren = 0;
                    float totalHeight = 0.0f;
                    
                    var children = property.GetEnumerator();

                    while (children.MoveNext()) {
                        SerializedProperty child = children.Current as SerializedProperty;

                        GUIContent childLabel = new GUIContent(child.displayName);

                        totalHeight += EditorGUI.GetPropertyHeight(child, childLabel) + EditorGUIUtility.standardVerticalSpacing;
                        numChildren++;
                    }

                    // Remove extra space at end, (we only want spaces between items)
                    totalHeight -= EditorGUIUtility.standardVerticalSpacing;

                    return totalHeight;
                }

                return EditorGUI.GetPropertyHeight(property, label);
            }
        }

        /// <summary>
        /// Errors default to showing the property.
        /// </summary>
        private bool ShowMe(SerializedProperty property) {
            drawIf = attribute as DrawIfAttribute;
            // Replace propertyname to the value from the parameter
            string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawIf.comparedPropertyName) : drawIf.comparedPropertyName;

            comparedField = property.serializedObject.FindProperty(path);

            if (comparedField == null) {
                Debug.LogError("Cannot find property with name: " + path);
                return true;
            }

            // get the value & compare based on types
            switch (comparedField.type) { // Possible extend cases to support your own type
                case "bool":
                    return comparedField.boolValue.Equals(drawIf.comparedValue);
                case "Enum":
                    return comparedField.enumValueIndex.Equals((int)drawIf.comparedValue);
                default:
                    Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                    return true;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // If the condition is met, simply draw the field.
            if (ShowMe(property)) {
                // A Generic type means a custom class...
                if (property.propertyType == SerializedPropertyType.Generic) {
                    var children = property.GetEnumerator();

                    Rect offsetPosition = position;

                    while (children.MoveNext()) {
                        SerializedProperty child = children.Current as SerializedProperty;

                        GUIContent childLabel = new GUIContent(child.displayName);

                        float childHeight = EditorGUI.GetPropertyHeight(child, childLabel);
                        offsetPosition.height = childHeight;

                        EditorGUI.PropertyField(offsetPosition, child, childLabel);

                        offsetPosition.y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                } else {
                    EditorGUI.PropertyField(position, property, label);
                }

            } //...check if the disabling type is read only. If it is, draw it disabled
            else if (drawIf.disablingType == DrawIfAttribute.DisablingType.ReadOnly) {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = true;
            }
        }

    }

}
#endif