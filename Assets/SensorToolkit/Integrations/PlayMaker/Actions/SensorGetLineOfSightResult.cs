#if PLAYMAKER

using System.Collections;
using System.Linq;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("For a LOS Sensor this will give more detailed results from the line-of-sight test performed on an object. You will rarely need this action. To know an objects visibility it's better to use the 'Sensor Get Signal' action and use the 'Store Strength' output.")]
    public class SensorGetLineOfSightResult : SensorToolkitAction3DOr2D<LOSSensor, LOSSensor2D> {

        [ActionSection("Inputs")]

        [RequiredField]
        [Tooltip("Retrieves LOS result for this GameObject")]
        public FsmGameObject targetObject;

        [Tooltip("Runs the action every frame.")]
        public bool everyFrame;

        [ActionSection("Outputs")]

        [UIHint(UIHint.Variable)]
        public FsmFloat storeVisibility;

        [UIHint(UIHint.Variable)]
        public FsmBool storeIsVisible;

        [Tooltip("Store the array of visible LOSTarget Transforms here.")]
        [UIHint(UIHint.Variable)]
        [ArrayEditor(VariableType.GameObject)]
        public FsmArray storeVisibleTransforms;

        [Tooltip("Store the array of visible target positions here.")]
        [UIHint(UIHint.Variable)]
        [ArrayEditor(VariableType.Vector3)]
        public FsmArray storeVisiblePositions;

        public override void Reset() {
            base.Reset();
            targetObject = null;
            storeVisibility = null;
            storeIsVisible = null;
            storeVisibleTransforms = null;
            storeVisiblePositions = null;
            everyFrame = false;
        }

        public override void OnEnter2D(LOSSensor2D sensor) {
            OnUpdate2D(sensor);
            if (!everyFrame) {
                Finish();
            }
        }

        public override void OnEnter3D(LOSSensor sensor) {
            OnUpdate3D(sensor);
            if (!everyFrame) {
                Finish();
            }
        }

        public override void OnExit3D(LOSSensor sensor) { }

        public override void OnExit2D(LOSSensor2D sensor) { }

        public override void OnUpdate2D(LOSSensor2D sensor) {
            var result = sensor.GetResult(targetObject.Value);
            if (result != null) {
                storeVisibility.Value = result.Visibility;
                storeIsVisible.Value = result.IsVisible;
                if (!storeVisibleTransforms.IsNone) {
                    storeVisibleTransforms.Values = result.Rays
                        .Where(r => r.TargetTransform != null && r.Visibility > 0)
                        .Select(r => r.TargetTransform).ToArray();
                }
                if (!storeVisiblePositions.IsNone) {
                    var visiblePositions = result.Rays.Where(r => r.Visibility > 0).Select(r => r.TargetPoint).ToList();
                    var boxedPositions = new object[visiblePositions.Count];
                    for (int i = 0; i < visiblePositions.Count; i++) {
                        boxedPositions[i] = visiblePositions[i];
                    }
                    storeVisiblePositions.Values = boxedPositions;
                }
            }
        }

        public override void OnUpdate3D(LOSSensor sensor) {
            var result = sensor.GetResult(targetObject.Value);
            if (result != null) {
                storeVisibility.Value = result.Visibility;
                storeIsVisible.Value = result.IsVisible;
                if (!storeVisibleTransforms.IsNone) {
                    storeVisibleTransforms.Values = result.Rays
                        .Where(r => r.TargetTransform != null && r.Visibility > 0)
                        .Select(r => r.TargetTransform).ToArray();
                }
                if (!storeVisiblePositions.IsNone) {
                    var visiblePositions = result.Rays.Where(r => r.Visibility > 0).Select(r => r.TargetPoint).ToList();
                    var boxedPositions = new object[visiblePositions.Count];
                    for (int i = 0; i < visiblePositions.Count; i++) {
                        boxedPositions[i] = visiblePositions[i];
                    }
                    storeVisiblePositions.Values = boxedPositions;
                }
            }
        }
    }

}

#endif