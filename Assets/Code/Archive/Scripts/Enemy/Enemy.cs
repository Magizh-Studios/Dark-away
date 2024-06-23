using StarterAssets;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, ILightAffectable {
    public static Enemy Instance { get; private set; }

    private CircleManager TEST_CircleManager;
    private Transform playerTransform = null;

    [SerializeField] private State currentState;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float sightRadius = 5f;
    [SerializeField] private float fovThreshold = 2f;
    [SerializeField] private float enemyNormalSpeed = 5f;
    [SerializeField] private float enemySprintSpeed = 10f;
    [SerializeField] private float speedIncreaseRatio = 0.8f;
    [SerializeField] private float slowDownDistanceThreshold = 5;
    [SerializeField] private LayerMask sightLayer;

    private NavMeshAgent agent;
    private Transform targetTransform;
    private Action OnPlayerReached;

    [SerializeField] private bool isAffectedByLight;
    public bool IsAffectedByLight {
        get => isAffectedByLight;
        set {
            isAffectedByLight = value;
            if (isAffectedByLight) {
                currentState = State.Scared;
            }
        }
    }

    public enum State {
        Idle,
        Patrol,
        Chasing,
        Attack,
        Scared,
        WaitingForTime
    }

    private void Awake() {
        Instance = this;
        agent = GetComponent<NavMeshAgent>();
        playerTransform = FindObjectOfType<ThirdPersonController>().transform;
        TEST_CircleManager = FindObjectOfType<CircleManager>();

        currentState = State.Idle;
    }

    private void Update() {
        switch (currentState) {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Patrol:
                HandlePatrolState();
                break;
            case State.Chasing:
                HandleChasingState();
                break;
            case State.Attack:
                HandleAttackState();
                break;
            case State.Scared:
                HandleScaredState();
                break;
            case State.WaitingForTime:
                HandleWaitingForTimeState();
                break;
        }

        agent.speed = Mathf.Clamp(agent.speed, enemyNormalSpeed, enemySprintSpeed);
        Debug.Log($"Current Agent Speed: {agent.speed}"); // Debugging current speed
    }

    private void HandleIdleState() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRadius, sightLayer);
        foreach (var collider in colliders) {
            if (collider.gameObject.TryGetComponent(out Player _)) {
                currentState = State.Chasing;
            }
        }
    }

    private void HandlePatrolState() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach (var collider in colliders) {
            if (collider.gameObject.TryGetComponent(out Player _)) {
                currentState = State.Chasing;
                return;
            }
            if (collider.TryGetComponent(out PathNode pathNode) && !pathNode.InspectedArea) {
                Vector3 directionToNode = (pathNode.transform.position - transform.position).normalized;
                float angleToNode = Vector3.Angle(transform.forward, directionToNode);

                if (angleToNode < sightRadius / fovThreshold) {
                    pathNode.InspectedArea = true;
                }
            }
        }
        MoveToNextFarthestPoint();
    }

    private void MoveToNextFarthestPoint() {
        PathNode farthestPathNode = TEST_CircleManager.GetFarthestPathNodeNotInspected(transform.position);

        if (farthestPathNode != null) {
            Setarget(farthestPathNode.transform, () => {
                farthestPathNode.InspectedArea = true;
                MoveToNextFarthestPoint();
            });
        }
        else {
            Debug.Log("No more uninspected farthest points available.");
        }
    }

    private void HandleChasingState() {

        targetTransform = playerTransform;

        if (targetTransform != null) {
            agent.SetDestination(targetTransform.position);
            agent.speed += Time.deltaTime * speedIncreaseRatio;
            Debug.Log($"Speed after increase: {agent.speed}"); // Debugging speed increase

            float distance = Vector3.Distance(transform.position, targetTransform.position);
            if (distance < slowDownDistanceThreshold) {
                float lerpedSpeed = Mathf.Lerp(0, enemySprintSpeed, distance / slowDownDistanceThreshold);
                agent.speed = lerpedSpeed;
            }

            if (agent.remainingDistance < 0.1f && !agent.pathPending) {
                targetTransform = null;
                OnPlayerReached?.Invoke();
                agent.speed = enemyNormalSpeed;
            }
        }

        if (Vector3.Distance(transform.position, playerTransform.position) < attackRadius) {
            currentState = State.Attack;
        }
    }

    private void HandleAttackState() {
        // Implement attack logic
    }

    private void HandleScaredState() {
        agent.speed = enemySprintSpeed;
        Debug.Log($"Speed in scared state: {agent.speed}"); // Debugging speed in scared state
        if (!isAffectedByLight) {
            currentState = State.Idle;
        }
    }

    private void HandleWaitingForTimeState() {
        float playerTorchRadius = Torch.Instance.FOVCollider.Length;

        if (playerTorchRadius <= 2f) {
            currentState = State.Chasing;
        }
    }

    public void Setarget(Transform targetTransform, Action OnPlayerReached = null) {
        this.targetTransform = targetTransform;
        this.OnPlayerReached = OnPlayerReached;
    }

    public void SetState(State state) {
        currentState = state;
    }

    public float GetCurrentSpeed() => agent.velocity.magnitude;
    public float GetMaxSpeed() => enemySprintSpeed;
}
