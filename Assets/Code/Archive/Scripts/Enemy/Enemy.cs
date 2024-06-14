using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, ILightAffectable
{
    private StateMachine stateMachine;
    public Transform playerTransform = null;
    public float attackRadius = 1f;
    public float patrolRadius = 10f;
    public float sightRadius = 5f;

    private NavMeshAgent agent;

    [SerializeField] private bool isAffectedByLight;
    public bool IsAffectedByLight
    {
        get => isAffectedByLight;
        set
        {
            isAffectedByLight = value;
            //EnemyScared(isAffectedByLight);
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
        stateMachine.SetState(chasingState); // This One Called Enter State And Also Exit
    }

    protected void Update()
    {
        stateMachine?.Update();                // This One Called frame Per Second Keep Updating State MAchin's Upadete State
    }

    public void SetCaughtByLight(bool isCaught)
    {
        IsAffectedByLight = isCaught;
    }

    public bool IsCrawling() => agent.velocity.magnitude > 0;

    [Serializable]
    private class IdleState : IState
    {
        private Enemy enemy;

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

        public PatrolState(Enemy enemy)
        {
            this.enemy = enemy;
        }
        public void Enter()
        {
            Debug.Log("Enemy Entered Patrol");
            SetRandomDestination();
        }
        public void Update()
        {
            int colliderCount = Physics.OverlapSphereNonAlloc(enemy.transform.position, enemy.sightRadius, colliders);
            // Check if reached destination
            Debug.Log("Colliders Count On Patrol " + colliderCount);
            if (!enemy.agent.pathPending && enemy.agent.remainingDistance < 0.1f)
            {
                // If patrol duration reached, set a new destination
                SetRandomDestination();
            }

            if (colliderCount > 0 && colliders[0].gameObject.TryGetComponent(out Player player))
            {
                // Player Entered Radius
                enemy.stateMachine.SetState(new ChasingState(enemy));
            }
        }

        private void SetRandomDestination()
        {
            // Set a random destination within the specified radius
            Vector3 randomPosition = GetRandomNavMeshPointWithinRadius(enemy.transform.position, enemy.patrolRadius);
            enemy.agent.SetDestination(randomPosition);
        }

        private Vector3 GetRandomNavMeshPointWithinRadius(Vector3 center, float radius)
        {
            // Generate a random direction within a circle
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * radius;

            // Convert the 2D random direction to 3D
            Vector3 randomPosition = new Vector3(randomDirection.x, 0f, randomDirection.y) + center;

            // Sample the nearest point on the NavMesh
            NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, radius, NavMesh.AllAreas);

            return hit.position;
        }

        public void Exit()
        {

        }
    }

    [Serializable]
    private class ChasingState : IState
    {
        private Enemy enemy;
        private Transform target = null;

        private float timer = 0;
        private float timerMax = 0.5f;
        public ChasingState(Enemy enemy)
        {
            this.enemy = enemy;
            target = null;
        }
        public void Enter()
        {
            Debug.Log("Enemy Entered Chasing State");
        }         
        public void Update()
        {
            if (target == null)
            {
                target = enemy.playerTransform;
                enemy.agent.SetDestination(target.position);
            }

            timer += Time.deltaTime;
            if (timer > timerMax)
            {
                enemy.agent.SetDestination(target.position);
                timer = 0;
            }

            //Debug.Log($"Robo to body Dis {owner.agent.remainingDistance}");
            if (enemy.agent.remainingDistance < enemy.attackRadius)
            {
                Debug.Log("Reached Target need To Change attack State");
                //enemy.stateMachine.SetState(enemy.attackState);
            }

            float distance = Vector3.Distance(enemy.transform.position, target.position);
            if(distance >= enemy.sightRadius)
            {
                //enemy.agent.SetDestination(enemy.transform.position);
                //enemy.stateMachine.SetState(new PatrolState(enemy));
            }

        }

        public void Exit()
        {

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
            enemy.agent.speed = 8;

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
            if (enemy.agent.remainingDistance < 1f)
            {
                enemy.stateMachine.SetState(enemy.waitingForTimeState);
            }
        }

        public void Exit()
        {
            enemy.agent.speed = 3.5f;
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
