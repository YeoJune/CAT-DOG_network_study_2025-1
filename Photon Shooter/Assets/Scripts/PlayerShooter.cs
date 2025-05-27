using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PlayerShooter : MonoBehaviourPun
{
    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float bulletSpeed = 10f;
    
    private float nextFireTime = 0f;

    private Queue<float> recentFireTimes = new Queue<float>();
    private int maxFiresPerSecond = 8;  // 초당 최대 8발
    
    private void Update()
    {
        // 로컬 플레이어만 총알 발사 처리
        if (photonView.IsMine && Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            // 🛡️ 발사 빈도 검증
            if (!ValidateFireRate())
            {
                Debug.LogWarning("🚨 Fire rate limit exceeded");
                return;  // 발사 차단
            }
            nextFireTime = Time.time + fireRate;
            // 기존 총알 생성 코드...
            GameObject bulletObj = PhotonNetwork.Instantiate("Bullet", firePoint.position, Quaternion.identity);
            int bulletViewID = bulletObj.GetComponent<PhotonView>().ViewID;
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

    // 🎯 발사 빈도 검증 메서드
    private bool ValidateFireRate()
    {
        float currentTime = Time.time;
        // 현재 발사 시간 기록
        recentFireTimes.Enqueue(currentTime);
        // 1초 이전 기록들 제거
        while (recentFireTimes.Count > 0 && currentTime - recentFireTimes.Peek() > 1f)
        {
            recentFireTimes.Dequeue();
        }
        // 초당 발사 횟수 제한 확인
        return recentFireTimes.Count <= maxFiresPerSecond;
    }

}