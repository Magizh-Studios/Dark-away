using StarterAssets;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, ILightAffectable {
    public static Enemy Instance { get; private set; }

    public event EventHandler OnEnemyEnteredGiveUpState;

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
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private LayerMask sightLayer;
    [SerializeField] private bool isAffectedByLight;

    private NavMeshAgent agent;
    private Transform targetTransform;
    private Action OnPlayerReached;

    private float currentAngerLevel = 0;
    [SerializeField] private float angerLevelMax = 10f;
    private bool isAngry = false;
    [SerializeField] private float angryIncreaseRatio = 0.4f;

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
        WaitingForTime,
        GiveUp
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
            case State.GiveUp:
                HandleGiveUpState();
                break;
        }

        if(targetTransform != null) {

            agent.SetDestination(targetTransform.position);
            agent.speed += Time.deltaTime * speedIncreaseRatio;
        }

        HandleSlowDown();

        HandleRotation();

        CheckTargetReached();

        

        agent.speed = Mathf.Clamp(agent.speed, enemyNormalSpeed, enemySprintSpeed);
        //Debug.Log($"Current Agent Speed: {agent.speed}"); // Debugging current speed
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

        if(targetTransform == null) {
            targetTransform = playerTransform;
        }
        
        if (targetTransform != null) {

            if (Vector3.Distance(transform.position, targetTransform.position) < attackRadius) {
                currentState = State.Attack;
            }

            if (Vector3.Distance(transform.position, targetTransform.position) < sightRadius) {
                isAngry = true;
            }
            else {
                currentAngerLevel = 0;
            }

            if (isAngry) {
                currentAngerLevel += Time.deltaTime * angryIncreaseRatio;
                if (currentAngerLevel > angerLevelMax) {
                    currentAngerLevel = 0;
                    isAngry = false;

                    targetTransform = null;
                    OnEnemyEnteredGiveUpState?.Invoke(this, EventArgs.Empty);

                    SetState(State.GiveUp);
                }
            }
        }


    }

    private void HandleRotation() {
        if(targetTransform == null) return;

        // Rotation logic
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void HandleSlowDown() {
        if (targetTransform == null) return;

        float distance = Vector3.Distance(transform.position, targetTransform.position);
        if (distance < slowDownDistanceThreshold) {
            float lerpedSpeed = Mathf.Lerp(0, enemySprintSpeed, distance / slowDownDistanceThreshold);
            agent.speed = lerpedSpeed;
        }
    }

    private void CheckTargetReached() {
        if (agent.remainingDistance < 0.1f && !agent.pathPending) {
            targetTransform = null;
            OnPlayerReached?.Invoke();
            agent.speed = enemyNormalSpeed;
        }
    }



    private void HandleAttackState() {
        // Implement attack logic
        Debug.Log("enemy attcked");
    }

    private void HandleScaredState() {
        agent.speed = enemySprintSpeed;
        //Debug.Log($"Speed in scared state: {agent.speed}"); // Debugging speed in scared state
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

    private void HandleGiveUpState() {
        Debug.Log("Enemy Entered GiveUpState Play Roar");
    }

    public void RunWay() { //referenced On Unity Event
        if (targetTransform == null) {
            Setarget(EnemySpawner.Instance.GetRandomHidePosition(), () => {
                gameObject.SetActive(false);
            });
        }
    }

    public void Setarget(Transform targetTransform, Action OnPlayerReached = null) {
        this.targetTransform = targetTransform;
        this.OnPlayerReached = OnPlayerReached;
        agent.speed = enemyNormalSpeed;
    }

    public void SetState(State state) {
        currentState = state;
    }

    public float GetCurrentSpeed() => agent.velocity.magnitude;
    public float GetMaxSpeed() => enemySprintSpeed;
}
