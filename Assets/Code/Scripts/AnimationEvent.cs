using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DarkAwayUtility {
    [RequireComponent(typeof(Animator))]
    public class AnimationEvent : MonoBehaviour {
        public List<Eventata> eventDatas;

        public void TriggerAnimationEvent(string eventName) {
            Eventata eventData = eventDatas.Where(_event => (_event.EventName == eventName)).FirstOrDefault();
            eventData.Event?.Invoke();
        }


        [System.Serializable]
        public class Eventata {
            public UnityEvent Event;
            public string EventName;
        }
    }
}
