using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Micosmo.SensorToolkit {

    public class STUtils {

        public static List<Collider> GetColliders(Rigidbody ofRB, List<Collider> storage) {
            storage.Clear();
            return GetCollidersRecursive(ofRB, ofRB.transform, storage);
        }

        static List<Collider> results = new List<Collider>();
        static List<Collider> GetCollidersRecursive(Rigidbody ofRB, Transform node, List<Collider> storage) {
            results.Clear();
            
            if (!(node.TryGetComponent<Rigidbody>(out var rb) && ReferenceEquals(rb, ofRB))) {
                return storage;
            }

            node.GetComponents(results);
            storage.AddRange(results);

            var childCount = node.childCount;
            for (int i = 0; i < childCount; i++) {
                var child = node.GetChild(i);
                GetCollidersRecursive(ofRB, child, storage);
            }

            return storage;
        }

        public static Bounds GetBoundsOfColliders(List<Collider> colliders) {
            Bounds bounds = new Bounds();
            bool isFirst = true;
            foreach (var collider in colliders) {
                if (isFirst) {
                    bounds = collider.bounds;
                    isFirst = false;
                } else {
                    bounds.Encapsulate(collider.bounds);
                }
            }
            return bounds;
        }
    }
}