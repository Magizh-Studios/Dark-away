using UnityEngine;

public class EnemyAnimation : MonoBehaviour {
    private const string SPEED_BLEND = "Speed";
    private const string HURT_TRIGGER = "IsHurt";

    [SerializeField] private float layerTransitionSpeed = 5f;
    private float targetLayerWeight;
    private Enemy enemy;
    private Animator animator;

    
    private void Awake() {
        enemy = GetComponent<Enemy>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update() {
        animator.SetFloat(SPEED_BLEND, Mathf.Clamp01(enemy.GetCurrentSpeed() / enemy.GetMaxSpeed()));

        if (enemy.IsAffectedByLight) {
            targetLayerWeight = 1f;
            animator.SetBool(HURT_TRIGGER, true);
        }
        else {
            targetLayerWeight = 0f;
            animator.SetBool(HURT_TRIGGER, false);
        }

        float currentLayerWeight = animator.GetLayerWeight(1);
        float newLayerWeight = Mathf.Lerp(currentLayerWeight, targetLayerWeight, Time.deltaTime * layerTransitionSpeed);
        animator.SetLayerWeight(1, newLayerWeight);
    }
}
