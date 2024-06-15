using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private const string SPEED_BLEND = "Speed";
    private Enemy enemy;
    private Animator animator;

    private void Awake() {
        enemy = GetComponent<Enemy>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update() {
        animator.SetFloat(SPEED_BLEND, Mathf.Clamp01(enemy.GetCurrentSpeed() / enemy.GetSprintSpeed()));
    }
}
