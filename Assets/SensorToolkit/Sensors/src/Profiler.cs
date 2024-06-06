using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace Micosmo.SensorToolkit.Profiling {
    public class Profiler {

        static Dictionary<string, ProfilerMarker> markerMap = new Dictionary<string, ProfilerMarker>();

        public static void Begin(string name) => Get(name).Begin();
        public static void End(string name) => Get(name).End();

        static string prevChainName;
        static bool isChaining;
        public static void Chain(string name) {
            EndChain();
            Begin(name);
            prevChainName = name;
            isChaining = true;
        }
        public static void EndChain() {
            if (isChaining) {
                End(prevChainName);
                isChaining = false;
            }
        }

        static ProfilerMarker Get(string name) {
            ProfilerMarker m;
            if (!markerMap.TryGetValue(name, out m)) {
                m = new ProfilerMarker(name);
                markerMap[name] = m;
            }
            return m;
        }

    }

}