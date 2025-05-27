using UnityEngine;
using Photon.Pun;
using System.Collections;
using Photon.Realtime;

// 상속을 MonoBehaviourPunCallbacks으로 변경
public class EnemySpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemies = 10;

    private Coroutine spawnCoroutine;
    
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            spawnCoroutine = StartCoroutine(SpawnEnemies());
        }
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
                
                // Photon을 통해 적 생성
                PhotonNetwork.Instantiate("Enemy", spawnPosition, Quaternion.identity);
            }
            
            // 스폰 간격만큼 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    // 🎯 핵심: MasterClient 변경 콜백 추가
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 새로운 MasterClient가 되었을 때
            if (spawnCoroutine == null)
            {
                spawnCoroutine = StartCoroutine(SpawnEnemies());
            }
        }
        else
        {
            // MasterClient가 아니면 코루틴 정리
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
    }
}