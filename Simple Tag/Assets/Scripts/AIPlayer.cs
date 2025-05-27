using UnityEngine;
using System.Linq;

public class AIPlayer : PlayerController
{
    [Header("AI Settings")]
    public float aiSpeedMultiplier = 0.8f;
    public float detectionRange = 8f;
    public float changeDirectionTime = 1.5f;
    public float boundaryAvoidDistance = 1.5f;
    public float reactionTime = 0.1f;  // AI 반응 시간
    
    [Header("AI Behavior")]
    public float aggressionLevel = 1.0f;  // 공격성 (1.0 = 보통, 0.5 = 소극적, 1.5 = 적극적)
    public float fearLevel = 1.0f;        // 도망 성향
    
    private Vector2 currentDirection;
    private float lastDirectionChange;
    private PlayerController targetPlayer;
    private float lastTargetUpdate;
    private float nextReactionTime;
    
    // AI 상태 추적
    private Vector3 lastKnownTaggerPosition;
    private float lastTaggerSeen;
    private bool hasRecentTagInfo = false;
    
    protected override void Start()
    {
        base.Start();
        
        // AI마다 약간 다른 특성 부여
        aggressionLevel = Random.Range(0.7f, 1.3f);
        fearLevel = Random.Range(0.8f, 1.2f);
        reactionTime = Random.Range(0.05f, 0.2f);
        
        nextReactionTime = Time.time + reactionTime;
    }
    
    protected override void Update()
    {
        if (Time.time >= nextReactionTime)
        {
            DoAIBehavior();
            nextReactionTime = Time.time + reactionTime;
        }
        
        EnforcePlayAreaBounds();
        UpdateVisuals();
    }
    
    private void DoAIBehavior()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.IsGameActive()) return;
        
        UpdateTarget();
        
        Vector2 moveDirection = CalculateMoveDirection();
        moveDirection = ApplyBoundaryAvoidance(moveDirection);
        
        // 움직임 적용
        if (GetComponent<Rigidbody2D>() != null)
        {
            float speedModifier = CurrentRole == PlayerRole.Tagger ? aggressionLevel : fearLevel;
            GetComponent<Rigidbody2D>().velocity = moveDirection * moveSpeed * aiSpeedMultiplier * speedModifier;
        }
    }
    
    private void UpdateTarget()
    {
        if (Time.time - lastTargetUpdate < 0.2f) return;  // 타겟 업데이트 빈도 제한
        
        var gameManager = GameManager.Instance;
        if (gameManager == null) return;
        
        if (CurrentRole == PlayerRole.Tagger)
        {
            // 태거: 가장 가까운 러너 추적
            targetPlayer = gameManager.GetClosestPlayer(transform.position, PlayerRole.Runner, this);
        }
        else
        {
            // 러너: 태거가 감지 범위 내에 있으면 도망
            var tagger = gameManager.GetPlayersWithRole(PlayerRole.Tagger).FirstOrDefault();
            
            if (tagger != null)
            {
                float distance = Vector3.Distance(transform.position, tagger.transform.position);
                
                if (distance <= detectionRange)
                {
                    targetPlayer = tagger;
                    lastKnownTaggerPosition = tagger.transform.position;
                    lastTaggerSeen = Time.time;
                    hasRecentTagInfo = true;
                }
                else if (hasRecentTagInfo && Time.time - lastTaggerSeen < 3f)
                {
                    // 최근에 태거를 봤으면 마지막 위치에서 도망
                    // targetPlayer는 null로 두고 lastKnownTaggerPosition 사용
                    targetPlayer = null;
                }
                else
                {
                    targetPlayer = null;
                    hasRecentTagInfo = false;
                }
            }
        }
        
        lastTargetUpdate = Time.time;
    }
    
    private Vector2 CalculateMoveDirection()
    {
        Vector2 moveDirection = Vector2.zero;
        
        if (CurrentRole == PlayerRole.Tagger)
        {
            // 태거 행동: 러너 추적
            if (targetPlayer != null)
            {
                moveDirection = (targetPlayer.transform.position - transform.position).normalized;
            }
            else
            {
                // 타겟이 없으면 랜덤 패트롤
                moveDirection = GetPatrolDirection();
            }
        }
        else
        {
            // 러너 행동: 태거로부터 도망
            if (targetPlayer != null)
            {
                // 태거가 시야에 있으면 직접 도망
                moveDirection = (transform.position - targetPlayer.transform.position).normalized;
            }
            else if (hasRecentTagInfo)
            {
                // 최근 태거 위치에서 도망
                moveDirection = (transform.position - lastKnownTaggerPosition).normalized;
            }
            else
            {
                // 안전하면 랜덤 이동
                moveDirection = GetPatrolDirection();
            }
        }
        
        return moveDirection;
    }
    
    private Vector2 GetPatrolDirection()
    {
        if (Time.time - lastDirectionChange >= changeDirectionTime)
        {
            currentDirection = Random.insideUnitCircle.normalized;
            lastDirectionChange = Time.time;
        }
        return currentDirection;
    }
    
    private Vector2 ApplyBoundaryAvoidance(Vector2 originalDirection)
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null) return originalDirection;
        
        Vector3 currentPos = transform.position;
        Vector2 avoidanceForce = Vector2.zero;
        
        // 경계와의 거리 체크
        float distanceToLeft = currentPos.x - gameManager.MinX;
        float distanceToRight = gameManager.MaxX - currentPos.x;
        float distanceToBottom = currentPos.y - gameManager.MinY;
        float distanceToTop = gameManager.MaxY - currentPos.y;
        
        // 경계 회피 힘 계산
        if (distanceToLeft < boundaryAvoidDistance)
        {
            avoidanceForce.x += (boundaryAvoidDistance - distanceToLeft) / boundaryAvoidDistance;
        }
        if (distanceToRight < boundaryAvoidDistance)
        {
            avoidanceForce.x -= (boundaryAvoidDistance - distanceToRight) / boundaryAvoidDistance;
        }
        if (distanceToBottom < boundaryAvoidDistance)
        {
            avoidanceForce.y += (boundaryAvoidDistance - distanceToBottom) / boundaryAvoidDistance;
        }
        if (distanceToTop < boundaryAvoidDistance)
        {
            avoidanceForce.y -= (boundaryAvoidDistance - distanceToTop) / boundaryAvoidDistance;
        }
        
        // 회피 힘이 너무 강하지 않도록 제한
        Vector2 finalDirection = (originalDirection + avoidanceForce * 2f);
        return finalDirection.magnitude > 0 ? finalDirection.normalized : originalDirection;
    }
    
    // 태그 이벤트 알림 (GameManager에서 호출)
    public void OnTagEvent(PlayerController tagger, PlayerController target)
    {
        // 태그 이벤트에 반응
        if (tagger != this && target != this)
        {
            // 다른 플레이어들의 태그를 관찰하고 반응
            if (CurrentRole == PlayerRole.Runner)
            {
                // 태거 위치 업데이트
                lastKnownTaggerPosition = tagger.transform.position;
                lastTaggerSeen = Time.time;
                hasRecentTagInfo = true;
                
                // 약간의 공포 반응 (속도 일시적 증가)
                StartCoroutine(TemporarySpeedBoost(1.2f, 1f));
            }
        }
    }
    
    private System.Collections.IEnumerator TemporarySpeedBoost(float multiplier, float duration)
    {
        float originalMultiplier = aiSpeedMultiplier;
        aiSpeedMultiplier *= multiplier;
        yield return new WaitForSeconds(duration);
        aiSpeedMultiplier = originalMultiplier;
    }
    
    protected override void HandleMovement()
    {
        // AI가 자체적으로 움직임을 처리하므로 부모 클래스의 입력 처리 무시
    }
}