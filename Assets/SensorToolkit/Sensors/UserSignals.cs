﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {
    [AddComponentMenu("Sensors/User Signals")]
    [HelpURL("https://micosmo.com/sensortoolkit2/docs/manual/sensors/user-signals")]
    public class UserSignals : Sensor {

        [Serializable]
        public class ObservableSignalList : ObservableList<Signal> { }

        [SerializeField]
        ObservableSignalList inputSignals = new ObservableSignalList();
        public ObservableSignalList InputSignals => inputSignals;

#pragma warning disable
        public override event Action OnPulsed;

        public override void PulseAll() => Pulse();

        protected override PulseJob GetPulseJob() {
            InputSignalsChangeHandler();
            return default;
        }

        protected override void Awake() {
            base.Awake();
            inputSignals.OnChanged += InputSignalsChangeHandler;
            InputSignalsChangeHandler();
        }

        void OnDestroy() {
            inputSignals.OnChanged -= InputSignalsChangeHandler;
        }

        void OnValidate() {
            if (inputSignals == null) {
                inputSignals = new ObservableSignalList();
            }
            inputSignals.OnValidate();
        }

        List<Signal> workList = new List<Signal>();
        void InputSignalsChangeHandler() {
            workList.Clear();
            foreach (var signal in inputSignals) {
                workList.Add(signal);
            }
            UpdateAllSignals(workList);
        }
    }
}