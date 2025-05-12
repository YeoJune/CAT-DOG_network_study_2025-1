using UnityEngine;
using Photon.Pun;
using System.Collections;

public class EnemySpawner : MonoBehaviourPun
{
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemies = 10;
    
    private void Start()
    {
        // --- TODO ---
        // MasterClient만 적 생성 담당 (게임 로직 일관성)
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnEnemies());
        }
        // ------
    }
    
    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // 현재 적의 수 확인
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            // 최대 적 수보다 적으면 새로 생성
            if (enemies.Length < maxEnemies)
            {
                // 랜덤 위치 생성 (스폰 반경 내에서)
                Vector2 spawnPosition = Random.insideUnitCircle * spawnRadius;
                
                // --- TODO ---
                // Photon을 통해 적 생성
                PhotonNetwork.Instantiate("Enemy", spawnPosition, Quaternion.identity);
                // ------
            }
            
            // 스폰 간격만큼 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}