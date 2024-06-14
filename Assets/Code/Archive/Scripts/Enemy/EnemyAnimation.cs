using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private const string CRAWLING = "IsCrawling";
    private Enemy enemy;
    private Animator animator;

    private void Awake() {
        enemy = GetComponent<Enemy>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update() {
        animator.SetBool(CRAWLING, enemy.IsCrawling());
    }
}
