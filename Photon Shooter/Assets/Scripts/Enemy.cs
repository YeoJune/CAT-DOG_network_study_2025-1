using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviourPun
{
    [SerializeField] private float moveSpeed = 3f;
    
    private Transform targetPlayer;
    
    private void Start()
    {
        // MasterClient가 아니면 리턴 (AI는 MasterClient만 실행)
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        // 가장 가까운 플레이어를 타겟으로 설정
        FindClosestPlayer();
    }
    
    private void Update()
    {
        // MasterClient가 아니면 리턴
        if (!PhotonNetwork.IsMasterClient)
            return;
            
        // 타겟 플레이어 쪽으로 이동
        if (targetPlayer != null)
        {
            // 플레이어 방향으로 이동
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // 플레이어 방향으로 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // 타겟이 없으면 새 타겟 찾기
            FindClosestPlayer();
        }
    }
    
    // 가장 가까운 플레이어 찾기
    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        float closestDistance = Mathf.Infinity;
        Transform closest = null;
        
        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player.transform;
            }
        }
        
        targetPlayer = closest;
    }
    
    // 적 사망 처리
    public void Die()
    {
        // 파티클 효과나 사운드는 여기서 실행
        
        // --- TODO ---
        // 네트워크를 통해 모든 클라이언트에서 적 제거
        PhotonNetwork.Destroy(gameObject);
        // ------
    }
}