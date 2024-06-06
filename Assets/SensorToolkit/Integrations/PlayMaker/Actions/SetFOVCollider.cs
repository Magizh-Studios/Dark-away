#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker
{
    [ActionCategory("SensorToolkit")]
    [Tooltip ("Sets the properties of a FOVCollider object. Note that rebuilding the collider can incur a large performance cost, so it is not recommended on a per-frame basis.")]
    public class SetFOVCollider : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(FOVCollider))]
        public FsmObject fovCollider;

        public FsmFloat length;
        public FsmFloat nearDistance;
        [HasFloatSlider(0,180f)]
        public FsmFloat FOVAngle;
        [HasFloatSlider(0, 180f)]
        public FsmFloat elevationAngle;
        public FsmInt resolution;

        public FOVCollider _fovCollider => fovCollider.Value as FOVCollider;

        public override void Reset() {
            OwnerDefaultSensor();
            length = 5f;
            nearDistance = 0.1f;
            FOVAngle = 90f;
            resolution = 1;
            elevationAngle = 90f;
	    }

        void OwnerDefaultSensor() {
            if (Owner != null) {
                fovCollider = new FsmObject() { Value = Owner.GetComponent(typeof(FOVCollider)) };
            } else {
                fovCollider = null;
            }
        }

        public override void OnEnter() {
            setCollider();
            Finish();
	    }
	   
	    void setCollider() {
            _fovCollider.Length = length.Value;
            _fovCollider.NearDistance = nearDistance.Value;
            _fovCollider.FOVAngle = FOVAngle.Value;
            _fovCollider.ElevationAngle = elevationAngle.Value;
            _fovCollider.Resolution = resolution.Value;
            _fovCollider.CreateCollider();
        }
    }
}

#endif