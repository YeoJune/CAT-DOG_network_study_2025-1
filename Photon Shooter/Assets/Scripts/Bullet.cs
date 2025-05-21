using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPun
{
    private Vector3 direction;
    private float speed;
    private Player owner;
    private float destroyTime = 3f;
    
    // 총알 초기화
    public void Initialize(Vector3 dir, float spd, Player bulletOwner)
    {
        direction = dir.normalized;
        speed = spd;
        owner = bulletOwner;
        
        // 일정 시간 후 파괴
        Destroy(gameObject, destroyTime);
    }
    
    private void Update()
    {
        // 총알 이동
        transform.position += direction * speed * Time.deltaTime;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // --- TODO ---
        
        // 적과 충돌 처리
        if (collision.CompareTag("Enemy"))
        {
            // MasterClient만 적 처리 (게임 로직 일관성 유지)
            if (PhotonNetwork.IsMasterClient)
            {
                // 적 사망 처리
                collision.GetComponent<Enemy>().Die();
                
                // 점수 증가 (총알 주인의 점수)
                ScoreManager.Instance.AddScore(owner);                
            }
            // 총알 파괴
            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
        // 벽과 충돌 처리
        else if (collision.CompareTag("Wall"))
        {
            // 총알 파괴
            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
        // ------
    }
}