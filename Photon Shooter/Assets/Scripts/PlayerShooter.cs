using UnityEngine;
using Photon.Pun;

public class PlayerShooter : MonoBehaviourPun
{
    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float bulletSpeed = 10f;
    
    private float nextFireTime = 0f;
    
    private void Update()
    {
        // --- TODO ---
        // 로컬 플레이어만 총알 발사 가능
        if (photonView.IsMine && Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            // 다음 발사 시간 설정
            nextFireTime = Time.time + fireRate;
            
            // RPC로 모든 클라이언트에 발사 정보 전달
            photonView.RPC("Fire", RpcTarget.All, firePoint.position, firePoint.right);
        }
        // ------
    }
    
    // --- TODO ---
    // 발사 RPC 메서드
    [PunRPC]
    private void Fire(Vector3 position, Vector3 direction, PhotonMessageInfo info)
    {
        // Resources 폴더에서 총알 프리팹 로드
        GameObject bulletObj = PhotonNetwork.Instantiate("Bullet", position, Quaternion.identity);
        
        // 총알 초기화
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(direction, bulletSpeed, photonView.Owner);
        }
        
        // 발사 효과음 재생 (이펙트는 예시로만 포함)
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
    // ------
}