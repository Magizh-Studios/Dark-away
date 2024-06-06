using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example
{
    public enum Teams { Yellow, Magenta, None };

    public class TeamMember : MonoBehaviour {
        public Teams StartTeam;
        public Material YellowMaterial;
        public Material MagentaMaterial;

        private Teams team;
        public Teams Team {
            get => initialised ? team : StartTeam;
            set => SetTeam(value);
        }

        bool initialised = false;

        public bool IsEnemy(GameObject other) => IsEnemy(other.GetComponent<TeamMember>());
        public bool IsEnemy(TeamMember other) => other != null && other.team != Teams.None && other.team != team;

        public bool IsFriendly(GameObject other) => IsFriendly(other.GetComponent<TeamMember>());
        public bool IsFriendly(TeamMember other) => other != null && other.team != Teams.None && other.team == team;

        void Start() {
            Team = StartTeam;
            initialised = true;
        }

        void SetTeam(Teams x) {
            team = x;
            var targetMat = team == Teams.Yellow ? YellowMaterial : MagentaMaterial;
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers) {
                var mat = renderer.sharedMaterial;
                if (mat == YellowMaterial || mat == MagentaMaterial) {
                    renderer.sharedMaterial = targetMat;
                }
            }
        }
    }
}