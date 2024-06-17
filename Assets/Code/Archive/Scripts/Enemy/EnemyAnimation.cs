using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private const string SPEED_BLEND = "Speed";
    private const string HURT_TRIGGER = "IsHurt";
    private Enemy enemy;
    private Animator animator;

    private void Awake() {
        enemy = GetComponent<Enemy>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update() {
        animator.SetFloat(SPEED_BLEND, Mathf.Clamp01(enemy.GetCurrentSpeed() / enemy.GetSprintSpeed()));

        if(enemy.IsAffectedByLight) {
            animator.SetLayerWeight(1, 1);
            animator.SetBool(HURT_TRIGGER, true);
        }
        else {
            animator.SetLayerWeight(1, 0);
            animator.SetBool(HURT_TRIGGER, false);
        }
       
    }
}
