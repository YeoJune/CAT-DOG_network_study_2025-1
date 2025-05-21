using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerShooter : MonoBehaviourPun
{
    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float bulletSpeed = 10f;
    
    private float nextFireTime = 0f;
    
    private void Update()
    {
        // 로컬 플레이어만 총알 발사 처리
        if (photonView.IsMine && Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            // 다음 발사 시간 설정
            nextFireTime = Time.time + fireRate;
            
            // 직접 PhotonNetwork.Instantiate 호출 (이미 네트워크 동기화됨)
            GameObject bulletObj = PhotonNetwork.Instantiate("Bullet", firePoint.position, Quaternion.identity);
            
            // 생성된 총알의 PhotonView ID 가져오기
            int bulletViewID = bulletObj.GetComponent<PhotonView>().ViewID;
            
            // RPC로 모든 클라이언트에 초기화 정보 전달
            photonView.RPC("InitializeBulletRPC", RpcTarget.All, bulletViewID, firePoint.right, photonView.Owner);
        }
    }
    
    [PunRPC]
    private void InitializeBulletRPC(int bulletViewID, Vector3 direction, Player player)
    {
        // ViewID로 총알 PhotonView 찾기
        PhotonView bulletView = PhotonView.Find(bulletViewID);
        
        if (bulletView != null)
        {
            // 총알 초기화
            Bullet bullet = bulletView.GetComponent<Bullet>();
            if (bullet != null)
            {
                // 총알 초기화 (각 클라이언트에서 동일하게 동작하도록)
                bullet.Initialize(direction, bulletSpeed, player);
            }
        }
    }
}