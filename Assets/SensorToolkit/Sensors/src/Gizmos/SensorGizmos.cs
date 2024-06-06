﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Micosmo.SensorToolkit {

    public static partial class SensorGizmos {

        public static void FOVGizmo(ReferenceFrame frame, Vector3 pos, float length, float horizAngle, float vertAngle) {
            var m = Matrix4x4.TRS(pos, Quaternion.LookRotation(frame.Forward, frame.Up), Vector3.one);
            PushMatrix(m);

            if (horizAngle != 0) {
                PushColor(STPrefs.LOSFovColour);
                CircleSector(pos, frame.Forward, frame.Up, horizAngle, length);
                PopColor();
            }
            
            if (vertAngle != 0) {
                PushColor(STPrefs.LOSFovColour);
                CircleSector(pos, frame.Forward, frame.Right, vertAngle, length);
                PopColor();
            }

            PopMatrix();
        }

        public static void DetectedObjectGizmo(Bounds bounds) {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.identity);
            PushColor(STPrefs.SignalBoundsColour);

            if (bounds.extents != Vector3.zero) {
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Gizmos/SensorToolkit/LOS-VISIBLE.png");
            var size = new Vector2(texture.width, texture.height) / HandleUtility.GetHandleSize(bounds.center) / 3f;
            size = Vector2.Min(size, new Vector2(texture.width, texture.height));
            //size = Vector2.Max(size, Vector2.one * 12f);

            if (size.x > 8f && STPrefs.ShowEyeIconInSignal) {
                Handles.Label(bounds.center, new GUIContent() { image = texture }, new GUIStyle() { contentOffset = new Vector2(-size.x / 2f, -size.y / 2f), fixedHeight = size.x, fixedWidth = size.y });
            }

            PopColor();
            PopMatrix();
#endif
        }

        public static void RaycastHitGizmo(Vector3 point, Vector3 normal, bool isObstruction) {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.identity);
            PushColor(isObstruction ? STPrefs.CastingBlockedRayColour : STPrefs.CastingRayColour);

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            var screenSize = HandleUtility.GetHandleSize(point);
            var offset = (normal * screenSize * 0.01f);

            Handles.DrawSolidDisc(point + offset, normal, screenSize * 0.1f);
            SetColor(SetA(Color, 0.2f));
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
            Handles.DrawSolidDisc(point + offset, normal, screenSize * 0.1f);

            SetColor(STPrefs.RayHitNormalColour);
            Gizmos.DrawRay(point, normal * screenSize * 0.5f);

            PopColor();
            PopMatrix();
#endif
        }

        public static void SpherecastGizmo(Ray ray, float length, Quaternion rotation, float radius, bool isObstructed) {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.TRS(ray.origin, rotation, Vector3.one));

            PushColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingShapeColour);
            BackfaceSphereHandle(Vector3.zero, radius);
            SetColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingRayColour);
            BackfaceSphereHandle(ray.direction * length, radius);

            var localLeft = Vector3.Cross(Vector3.up, ray.direction).normalized;
            var localUp = Vector3.Cross(localLeft, ray.direction).normalized;

            Gizmos.DrawRay(localLeft * radius, ray.direction * length);
            Gizmos.DrawRay(-localLeft * radius, ray.direction * length);
            Gizmos.DrawRay(localUp * radius, ray.direction * length);
            Gizmos.DrawRay(-localUp * radius, ray.direction * length);

            PopColor();
            PopMatrix();
#endif
        }

        public static void CirclecastGizmo(Ray ray, float length, Quaternion rotation, float radius, bool isObstructed) {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.TRS(ray.origin, rotation, Vector3.one));
            PushColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingShapeColour);

            var height = length + radius;

            var pt1 = Vector3.zero;
            var pt2 = ray.direction*length;
            Handles.DrawWireDisc(pt1, -Vector3.forward, radius);

            SetColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingRayColour);

            Handles.DrawWireDisc(pt2, -Vector3.forward, radius);
            var right = Quaternion.LookRotation(ray.direction, Vector3.forward) * Vector3.right;
            Gizmos.DrawRay(pt1 + right * radius, ray.direction * (height - radius));
            Gizmos.DrawRay(pt1 - right * radius, ray.direction * (height - radius));

            PopColor();
            PopMatrix();
#endif
        }

        public static void BoxcastGizmo(Ray ray, float length, Quaternion rotation, Vector3 halfExtents, bool isObstructed) {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.TRS(ray.origin, rotation, Vector3.one));
            PushColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingShapeColour);

            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);

            SetColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingRayColour);

            Gizmos.DrawWireCube(ray.direction * length, halfExtents * 2f);

            Gizmos.DrawRay(Vector3.up * halfExtents.y + Vector3.right * halfExtents.x + Vector3.forward * halfExtents.z, ray.direction * length);
            Gizmos.DrawRay(Vector3.up * halfExtents.y + Vector3.right * halfExtents.x - Vector3.forward * halfExtents.z, ray.direction * length);
            Gizmos.DrawRay(Vector3.up * halfExtents.y - Vector3.right * halfExtents.x + Vector3.forward * halfExtents.z, ray.direction * length);
            Gizmos.DrawRay(Vector3.up * halfExtents.y - Vector3.right * halfExtents.x - Vector3.forward * halfExtents.z, ray.direction * length);

            Gizmos.DrawRay(-Vector3.up * halfExtents.y + Vector3.right * halfExtents.x + Vector3.forward * halfExtents.z, ray.direction * length);
            Gizmos.DrawRay(-Vector3.up * halfExtents.y + Vector3.right * halfExtents.x - Vector3.forward * halfExtents.z, ray.direction * length);
            Gizmos.DrawRay(-Vector3.up * halfExtents.y - Vector3.right * halfExtents.x + Vector3.forward * halfExtents.z, ray.direction * length);
            Gizmos.DrawRay(-Vector3.up * halfExtents.y - Vector3.right * halfExtents.x - Vector3.forward * halfExtents.z, ray.direction * length);

            PopColor();
            PopMatrix();
#endif
        }

        public static void CapsulecastGizmo(Ray ray, float length, Quaternion rotation, float radius, float height, bool isObstructed) {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.TRS(ray.origin, rotation, Vector3.one));
            PushColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingShapeColour);

            height = Mathf.Abs(height);

            CapsuleGizmo(Vector3.zero, radius, height);

            SetColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingRayColour);
            CapsuleGizmo(ray.direction * length, radius, height);

            var pt1 = Vector3.up * height / 2f;
            var pt2 = -pt1;

            Gizmos.DrawRay(pt1 + Vector3.right * radius, ray.direction * length);
            Gizmos.DrawRay(pt1 - Vector3.right * radius, ray.direction * length);
            Gizmos.DrawRay(pt1 + Vector3.up * radius, ray.direction * length);
            Gizmos.DrawRay(pt1 + Vector3.forward * radius, ray.direction * length);
            Gizmos.DrawRay(pt1 - Vector3.forward * radius, ray.direction * length);

            Gizmos.DrawRay(pt2 + Vector3.right * radius, ray.direction * length);
            Gizmos.DrawRay(pt2 - Vector3.right * radius, ray.direction * length);
            Gizmos.DrawRay(pt2 - Vector3.up * radius, ray.direction * length);
            Gizmos.DrawRay(pt2 + Vector3.forward * radius, ray.direction * length);
            Gizmos.DrawRay(pt2 - Vector3.forward * radius, ray.direction * length);

            PopColor();
            PopMatrix();
#endif
        }

        public static void Capsule2DcastGizmo(Ray ray, float length, Quaternion rotation, float radius, float height, bool isObstructed) {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.TRS(ray.origin, rotation, Vector3.one));
            PushColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingShapeColour);

            height = Mathf.Max(radius * 2, height);

            Capsule2DGizmo(Vector3.zero, radius, height);

            SetColor(isObstructed ? STPrefs.CastingBlockedRayColour : STPrefs.CastingRayColour);
            Capsule2DGizmo(ray.direction * length, radius, height);

            var pt1 = Vector3.up * (height / 2f - radius);
            var pt2 = -pt1;

            Gizmos.DrawRay(pt1 + Vector3.right * radius, ray.direction * length);
            Gizmos.DrawRay(pt1 - Vector3.right * radius, ray.direction * length);
            Gizmos.DrawRay(pt1 + Vector3.up * radius, ray.direction * length);

            Gizmos.DrawRay(pt2 + Vector3.right * radius, ray.direction * length);
            Gizmos.DrawRay(pt2 - Vector3.right * radius, ray.direction * length);
            Gizmos.DrawRay(pt2 - Vector3.up * radius, ray.direction * length);

            PopColor();
            PopMatrix();
#endif
        }
    }

}