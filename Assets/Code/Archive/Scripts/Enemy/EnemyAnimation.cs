using UnityEngine;

public class EnemyAnimation : MonoBehaviour {

    [SerializeField] private float layerTransitionSpeed = 5f;
    private float targetLayerWeight;
    private Enemy enemy;
    private Animator animator;

    private int ROAR_TRIGGER_HASH;
    private int HURT_HASH;
    private int SPEED_BLEND_HASH;

    private void Awake() {
        enemy = GetComponent<Enemy>();
        animator = GetComponentInChildren<Animator>();

        ROAR_TRIGGER_HASH = Animator.StringToHash("Roar");
        HURT_HASH = Animator.StringToHash("IsHurt");
        SPEED_BLEND_HASH = Animator.StringToHash("Speed");
    }

    private void Start() {
        enemy.OnEnemyEnteredGiveUpState += Enemy_OnEnemyEnteredGiveUpState;    
    }

    private void Enemy_OnEnemyEnteredGiveUpState(object sender, System.EventArgs e) {
        animator.SetTrigger(ROAR_TRIGGER_HASH);
    }

    private void Update() {
        animator.SetFloat(SPEED_BLEND_HASH, Mathf.Clamp01(enemy.GetCurrentSpeed() / enemy.GetMaxSpeed()));

        if (enemy.IsAffectedByLight) {
            targetLayerWeight = 1f;
            animator.SetBool(HURT_HASH, true);
        }
        else {
            targetLayerWeight = 0f;
            animator.SetBool(HURT_HASH, false);
        }

        float currentLayerWeight = animator.GetLayerWeight(1);
        float newLayerWeight = Mathf.Lerp(currentLayerWeight, targetLayerWeight, Time.deltaTime * layerTransitionSpeed);
        animator.SetLayerWeight(1, newLayerWeight);
    }
}
