using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Micosmo.SensorToolkit.Editors {
    [CustomPropertyDrawer(typeof(SteerSeek))]
    public class SteerSeekDrawer : PropertyDrawer {

        bool foldout = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            foldout = EditorGUI.Foldout(foldoutRect, foldout, label, true);

            if (!foldout) {
                return;
            }
            // Indent child fields
            EditorGUI.indentLevel++;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Find properties
            var seekMode = property.FindPropertyRelative("SeekMode");
            var seekPosition = property.FindPropertyRelative("SeekPosition");
            var seekDirection = property.FindPropertyRelative("SeekDirection");
            var arriveDistanceThreshold = property.FindPropertyRelative("ArriveDistanceThreshold");
            var stoppingDistance = property.FindPropertyRelative("StoppingDistance");

            var arriveDistanceThresholdPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(arriveDistanceThresholdPosition, arriveDistanceThreshold);
            position.y += arriveDistanceThresholdPosition.height + EditorGUIUtility.standardVerticalSpacing;

            var stoppingDistancePosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(stoppingDistancePosition, stoppingDistance);
            position.y += stoppingDistancePosition.height + EditorGUIUtility.standardVerticalSpacing;

            var seekModePosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(seekModePosition, seekMode);
            position.y += seekModePosition.height + EditorGUIUtility.standardVerticalSpacing;

            // Conditionally draw SeekPosition field based on SeekMode value
            if (seekMode.enumValueIndex == (int)SeekMode.Position) {
                var seekPositionPosition = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(seekPosition, label, true));
                EditorGUI.PropertyField(seekPositionPosition, seekPosition, true);
                position.y += seekPositionPosition.height + EditorGUIUtility.standardVerticalSpacing;
            } else if (seekMode.enumValueIndex == (int)SeekMode.Direction) {
                var seekDirectionPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(seekDirectionPosition, seekDirection);
                position.y += seekDirectionPosition.height + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!foldout) {
                return EditorGUIUtility.singleLineHeight;
            }

            var height = EditorGUIUtility.singleLineHeight;

            var seekMode = property.FindPropertyRelative("SeekMode");
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // seekMode

            if (seekMode.enumValueIndex == (int)SeekMode.Position) {
                var seekPosition = property.FindPropertyRelative("SeekPosition");
                height +=  EditorGUI.GetPropertyHeight(seekPosition, true) + EditorGUIUtility.standardVerticalSpacing;
            } else if (seekMode.enumValueIndex == (int)SeekMode.Direction) {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // seekDirection
            }

            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // ArriveDistanceThreshold
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // StoppingDistance

            return height;
        }
    }
}