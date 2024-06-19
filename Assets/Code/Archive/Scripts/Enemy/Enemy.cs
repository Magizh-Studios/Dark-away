using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : MonoBehaviour, ILightAffectable
{
    public static Enemy Instance { get; private set; }


    private StateMachine stateMachine;

    public CircleManager TEST_CircleManager;

    public Transform playerTransform = null;
    public float attackRadius = 1f;
    public float patrolRadius = 10f;
    public float sightRadius = 5f;
    public float fovThreshold = 2f;

    public float enemyNormalSpeed = 5f;
    public float enemySprintSpeed = 10f;
    public float speedIncreaseRatio = 0.8f;
    public float slowDownDistanceThreshold = 5;

    public LayerMask sightLayer;

    private NavMeshAgent agent;
    private Vector3 target;
    private Action OnPlayerReached;

    [SerializeField] private bool isAffectedByLight;
    public bool IsAffectedByLight
    {
        get => isAffectedByLight;
        set
        {
            isAffectedByLight = value;
            EnemyScared(isAffectedByLight);
        }
    }

    private IdleState idleState;
    private ChasingState chasingState;
    private PatrolState patrolState;
    private AttackState attackState;
    private ScaredState scaredState;
    private WaitingForTimeState waitingForTimeState;

    private void EnemyScared(bool enemyScared)
    {
        if(enemyScared)
        {
            stateMachine?.SetState(scaredState);
        }
    }

    private void Awake()
    {
        Instance = this;
        agent = GetComponent<NavMeshAgent>();

        idleState = new IdleState(this);
        chasingState = new ChasingState(this);
        patrolState = new PatrolState(this);
        attackState = new AttackState(this,playerTransform);
        scaredState = new ScaredState(this);
        waitingForTimeState = new WaitingForTimeState(this);

    }
    private void Start()
    {
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine();
        stateMachine?.SetState(patrolState); // This One Called Enter State And Also Exit
    }

    protected void Update()
    {
        stateMachine?.Update();                // This One Called frame Per Second Keep Updating State MAchin's Upadete State

        if(target != Vector3.zero) {
            agent.SetDestination(target);
            agent.speed += Time.deltaTime * speedIncreaseRatio;

            float distance = Vector3.Distance(transform.position, target);
            if (distance < slowDownDistanceThreshold) {
                // Calculate the speed based on the distance to the target
                float lerpedSpeed = Mathf.Lerp(0, enemySprintSpeed, distance / slowDownDistanceThreshold);
                agent.speed = lerpedSpeed;
            }

            if (agent.remainingDistance < 0.1f && !agent.pathPending) {
                target = Vector3.zero;
                OnPlayerReached?.Invoke();
                agent.speed = enemyNormalSpeed;
            }
        }

        agent.speed = Mathf.Clamp(agent.speed, enemyNormalSpeed, enemySprintSpeed);
    }

    public bool IsCrawling() => agent.velocity.magnitude > 0;

    public float GetCurrentSpeed() => agent.velocity.magnitude;
    public float GetSprintSpeed() => enemySprintSpeed;

    public void SetDestination(Vector3 target,Action OnPlayerReached = null) {
        this.target = target;
        this.OnPlayerReached = OnPlayerReached;
    }

    [Serializable]
    private class IdleState : IState
    {
        private Enemy enemy;
        protected Collider[] colliders = new Collider[5];

        internal IdleState(Enemy enemy)
        {
            this.enemy = enemy;
        }

        public void Enter()
        {
            Debug.Log("Enemy Entered Idle State");
        }

        public void Update()
        {
            int colliderCount = Physics.OverlapSphereNonAlloc(enemy.transform.position, enemy.sightRadius, colliders,enemy.sightLayer);

            if (colliderCount > 0) {

                for (int i = 0; i < colliderCount; i++)
                {
                    if(colliders[i].gameObject.TryGetComponent(out Player _)) {
                        // Player Entered Radius
                        enemy.stateMachine.SetState(enemy.chasingState);
                    }
                }
                
            }
        }

        public void Exit()
        {

        }
    }


    [Serializable]
    private class PatrolState : IState
    {    
        private Enemy enemy;
        protected Collider[] colliders = new Collider[5];
        private int colliderCount = 0;
        public PatrolState(Enemy enemy)
        {
            this.enemy = enemy;
        }
        public void Enter() {
            Debug.Log("Enemy Entered Patrol");

            MoveToNextFarthestPoint();
        }

        private void MoveToNextFarthestPoint() {
            PathNode farthestPathNode = enemy.TEST_CircleManager.GetFarthestPathNodeNotInspected(enemy.transform.position);

            if (farthestPathNode != null) {
                enemy.SetDestination(farthestPathNode.transform.position, () =>
                {
                    farthestPathNode.InspectedArea = true;
                    // Move to the next farthest point after reaching the current one
                    MoveToNextFarthestPoint();
                });
            }
            else {
                //want to reset
                Debug.Log("No more uninspected farthest points available.");
            }
        }

        public void Update()
        {
            colliderCount = Physics.OverlapSphereNonAlloc(enemy.transform.position, enemy.sightRadius, colliders);
            //Debug.Log("Colliders Count On Patrol " + colliderCount);
            if (colliderCount > 0) {

                HandlePlayerDetection();

                HandlePathNodeDEtection();
            }
        }

        private void HandlePlayerDetection() {

            for (int i = 0; i < colliderCount; i++) {
                if (colliders[i].gameObject.TryGetComponent(out Player _)) {
                    // Player Entered Radius
                    enemy.stateMachine.SetState(enemy.chasingState);
                }
            }
        }

        private void HandlePathNodeDEtection() {

            foreach (var nodeCollider in colliders) {
                if (nodeCollider.TryGetComponent(out PathNode pathNode) && !pathNode.InspectedArea) {
                    Vector3 directionToNode = (pathNode.transform.position - enemy.transform.position).normalized;
                    float angleToNode = Vector3.Angle(enemy.transform.forward, directionToNode);

                    if (angleToNode < enemy.sightRadius / enemy.fovThreshold) // assuming sightRadius is also used as FOV angle here
                    {
                        pathNode.InspectedArea = true;
                    }
                }
            }
        }

        public void Exit()
        {

        }
    }

    [Serializable]
    private class ChasingState : IState {
        private Enemy enemy;
        private Transform target = null;

        private float timer = 0;
        private float timerMax = 0.5f;

        public ChasingState(Enemy enemy) {
            this.enemy = enemy;
            target = null;
        }
        public void Enter() {
            Debug.Log("Enemy Entered Chasing State");
        }
        public void Update() {
            if (target == null) {
                target = enemy.playerTransform;
                enemy.agent.SetDestination(target.position);
            }

            timer += Time.deltaTime;
            if (timer > timerMax) {
                enemy.agent.SetDestination(target.position);
                timer = 0;
            }

            float distance = Vector3.Distance(enemy.transform.position, target.position);
            if (distance < enemy.slowDownDistanceThreshold) {
                // Calculate the speed based on the distance to the target
                float lerpedSpeed = Mathf.Lerp(0, enemy.enemySprintSpeed, distance / enemy.slowDownDistanceThreshold);
                enemy.agent.speed = lerpedSpeed;
            }

            if (enemy.agent.remainingDistance < enemy.attackRadius) {
                Debug.Log("Reached Target need To Change attack State");
                //enemy.stateMachine.SetState(enemy.attackState);
            }

            if (distance >= enemy.sightRadius) {
                enemy.agent.SetDestination(enemy.transform.position);
                enemy.stateMachine.SetState(enemy.idleState);
            }
        }

        public void Exit() {
        }
    }

    [Serializable]
    private class AttackState : IState
    {
        private Enemy enemy;
        private Transform target;

        private float timer = 0;
        private float timerMax = 1.5f;

        public AttackState(Enemy enemy, Transform target)
        {
            this.enemy = enemy;
            this.target = target;
        }

        public void Enter()
        {
            Debug.Log("Enemy Attack State");
        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer > timerMax)
            {
                if (Vector3.Distance(enemy.transform.position, target.position) < enemy.attackRadius)
                {
                    // Attack Logic
                    Debug.Log("Attack");
                }
                else
                {
                    enemy.stateMachine.SetState(enemy.chasingState);
                }

                timer = 0;
            }

        }

        public void Exit()
        {
            // You can add exit logic if needed
        }
    }


    [Serializable]
    private class ScaredState : IState
    {
        private Enemy enemy;
        public ScaredState(Enemy enemy)
        {
            this.enemy = enemy;
        }

        public void Enter()
        {
            Debug.Log("Enemy Scared State");
            enemy.agent.speed = enemy.enemySprintSpeed;
            //enemy.agent.SetDestination(GetValidHidePointPosition());
        }

        //private Vector3 GetValidHidePointPosition()
        //{
            //HidePoint hidePoint = HidePointManager.Instance.GetNearHidePointOutPlayerFov();
            
            //NavMesh.SamplePosition(hidePoint.transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas);

            //return hit.position;
        //}

        public void Update()
        {
            //if (enemy.agent.remainingDistance < 1f)
            //{
            //    enemy.stateMachine.SetState(enemy.waitingForTimeState);
            //}

            if(!enemy.isAffectedByLight) {
                enemy.stateMachine.SetState(enemy.idleState);
            }
        }

        public void Exit()
        {
            enemy.agent.speed = enemy.enemyNormalSpeed;
        }
    }


    [Serializable]
    private class WaitingForTimeState : IState
    {
        private const float chaseTorchRadius = 2f;
        private Enemy enemy;
        private float playerTorchRadius;
        public WaitingForTimeState(Enemy enemy)
        {
            this.enemy = enemy;
        }

        public void Enter()
        {
            Debug.Log("Enemy WaitingForTimeState State");
        }

        public void Update()
        {
            playerTorchRadius = Torch.Instance.FOVCollider.Length;

            if(playerTorchRadius <= chaseTorchRadius)
            {
                enemy.stateMachine.SetState(enemy.chasingState);
            }
        }

        public void Exit()
        {
            
        }
    }
}
