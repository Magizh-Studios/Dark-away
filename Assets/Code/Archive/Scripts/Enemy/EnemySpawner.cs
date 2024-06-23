using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private Transform enemyPrefab;
    [SerializeField] private Transform entrancePosTransform;

    private void Awake() {
        Instance = this;
    }

    public void SpawnEnemyGenric(Vector3 spawnPos) {
        Enemy enemy =  Instantiate(enemyPrefab,spawnPos,Quaternion.identity).GetComponent<Enemy>();
    }

    public void SpawnEnemyAtEntrance() {
        Enemy enemy = Instantiate(enemyPrefab, entrancePosTransform.position, Quaternion.identity).GetComponent<Enemy>();
        enemy.SetState(Enemy.State.Chasing);
    }

    public Transform GetRandomHidePosition() {
        return entrancePosTransform; // For Test
    }
}
