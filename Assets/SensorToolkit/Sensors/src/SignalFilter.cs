using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public class TagSelectorAttribute : PropertyAttribute { }

    [Serializable]
    public class SignalFilter {

        [Tooltip("Any GameObject in this list will not be detected by this sensor.")]
        public List<GameObject> IgnoreList = new List<GameObject>();

        [Tooltip("When set to true the sensor will only detect objects whose tags are in the 'withTag' array.")]
        public bool EnableTagFilter;

        [Tooltip("Array of tags that will be detected by the sensor.")]
        [TagSelector]
        public string[] AllowedTags;

        public bool IsNull() {
            var ignoreListLength = IgnoreList.Count;
            for (int i = 0; i < ignoreListLength; i++) {
                if (!(IgnoreList[i] is null)) {
                    return false;
                }
            }
            if (AllowedTags == null) {
                return true;
            }
            var allowedTagsLength = AllowedTags.Length;
            for (int i = 0; i < allowedTagsLength; i++) {
                if (AllowedTags[i] != null) {
                    return false;
                }
            }
            return true;
        }

        public bool TestCollider(Collider col) => TestCollider(col.gameObject, col.attachedRigidbody?.gameObject);
        public bool TestCollider(Collider2D col) => TestCollider(col.gameObject, col.attachedRigidbody?.gameObject);

        bool TestCollider(GameObject go, GameObject rbGo) {
            if (!IsPassingIgnoreList(go) || (!(rbGo is null) && !IsPassingIgnoreList(rbGo))) {
                return false;
            }
            return true;
        }

        public bool IsPassingTagFilter(GameObject go) {
            if (EnableTagFilter) {
                var tagFound = false;
                for (int i = 0; i < AllowedTags.Length; i++) {
                    if (AllowedTags[i] != "" && go != null && go.CompareTag(AllowedTags[i])) {
                        tagFound = true;
                        break;
                    }
                }
                if (!tagFound) {
                    return false;
                }
            }
            return true;
        }

        public bool IsPassingIgnoreList(GameObject go) {
            var ignoreListCount = IgnoreList.Count;
            for (int i = 0; i < ignoreListCount; i++) {
                if (ReferenceEquals(IgnoreList[i], go)) {
                    return false;
                }
            }
            return true;
        }
    }

}