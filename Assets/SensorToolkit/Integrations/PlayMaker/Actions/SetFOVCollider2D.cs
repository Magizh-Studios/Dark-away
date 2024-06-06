#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker
{
    [ActionCategory("SensorToolkit")]
    [Tooltip ("Sets the properties of a FOVCollider2D object. Note that rebuilding the collider can incur a large performance cost, so it is not recommended on a per-frame basis.")]
    public class SetFOVCollider2D : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(FOVCollider2D))]
        public FsmObject fovCollider;

        public FsmFloat length;
        public FsmFloat nearDistance;
        [HasFloatSlider(0,180f)]
        public FsmFloat FOVAngle;
        public FsmInt resolution;

        public FOVCollider2D _fovCollider => fovCollider.Value as FOVCollider2D;

        public override void Reset() {
            OwnerDefaultSensor();
            length = 5f;
            nearDistance = 0.1f;
            FOVAngle = 90f;
            resolution = 1;
	    }

        void OwnerDefaultSensor() {
            if (Owner != null) {
                fovCollider = new FsmObject() { Value = Owner.GetComponent(typeof(FOVCollider2D)) };
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
            _fovCollider.Resolution = resolution.Value;
            _fovCollider.CreateCollider();
        }
    }
}

#endif