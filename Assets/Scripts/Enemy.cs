using System;
using System.Collections;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Sprites")]
    public Image enemyImage;
    public SpriteAnimation idleAnimation;
    public SpriteAnimation walkAnimation;
    public SpriteAnimation attackAnimation;
    public SpriteAnimation flinchAnimation;
    public SpriteAnimation deathAnimation;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float chanceToIdleDefault = 0.8f;
    public float chanceToIdleChange = 0.1f;
    public float maxDistancePatrollingPoint = 0.1f;

    [Header("Detection")]
    public Transform eyes;
    public float detectionAngle = 90;
    public float detectionDistance = 10;

    [Header("Other Settings")]
    public NavMeshAgent navMeshAgent;
    public TextMeshProUGUI lifeText;
    public float attackRange = 1f;

    private int life;
    private EnemyStatus currentStatus;
    private bool playerDetected;
    private Coroutine animationCoroutine = null;
    private float chanceToIdle;
    private int lastPatrolPointIndex = -1;
    private int nextPatrolPointIndex;

    public enum EnemyStatus
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        Flinching,
        Dead
    }

    private void Start()
    {
        lifeText.text = life.ToString();
        lastPatrolPointIndex = GetCloserPatrolPointIndex();
        chanceToIdle = chanceToIdleDefault;
        SetStatus(EnemyStatus.Idle, true);
    }

    private int GetCloserPatrolPointIndex()
    {
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            Debug.LogError("Enemy " + name + " needs at least 2 patrol points");
            return -1;
        }

        int closestIndex = 0;
        float closestDistance = Mathf.Infinity;

        Vector3 currentPos = transform.position;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
            {
                Debug.LogError("Enemy " + name + " has null patrol points");
                return -1;
            }

            float distance = Utils.HorizontalDistance(currentPos, patrolPoints[i].position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void SetNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            Debug.LogError("Enemy " + name + " needs at least 2 patrol points");
        }
        int index = UnityEngine.Random.Range(0, patrolPoints.Length - 1);
        if (index >= lastPatrolPointIndex)
        {
            index++;
        }
        nextPatrolPointIndex = index;
    }

    public void SetStatus(EnemyStatus enemyStatus, bool force)
    {
        if (currentStatus != enemyStatus || force)
        {
            currentStatus = enemyStatus;
            switch (enemyStatus)
            {
                case EnemyStatus.Idle:
                    PlayAnimation(idleAnimation);
                    break;
                case EnemyStatus.Patrolling:
                    if (lastPatrolPointIndex >= 0)
                    {
                        SetNextPatrolPoint();
                        navMeshAgent.destination = patrolPoints[nextPatrolPointIndex].position;
                        PlayAnimation(walkAnimation);
                    }
                    break;
                case EnemyStatus.Chasing:
                    PlayAnimation(walkAnimation);
                    break;
                case EnemyStatus.Attacking:
                    PlayAnimation(attackAnimation);
                    break;
                case EnemyStatus.Flinching:
                    PlayAnimation(flinchAnimation);
                    break;
                case EnemyStatus.Dead:
                    PlayAnimation(deathAnimation);
                    break;
            }
        }
    }

    private void PlayAnimation(SpriteAnimation spriteAnimation)
    {
        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationEnumerator(spriteAnimation));
    }

    private IEnumerator AnimationEnumerator(SpriteAnimation spriteAnimation)
    {
        float timePerSprite = spriteAnimation.totalTime / spriteAnimation.sprites.Length;
        for (int i = 0; i < spriteAnimation.sprites.Length; i++)
        {
            enemyImage.sprite = spriteAnimation.sprites[i];
            yield return new WaitForSeconds(timePerSprite);
        }
        animationCoroutine = null;
    }

    private void Update()
    {
        if (!playerDetected)
        {
            if (CanSeePlayer())
            {
                playerDetected = true;
            }
            if (playerDetected)
            {
                if (PlayerIsInAttackRange())
                {
                    SetStatus(EnemyStatus.Attacking, true);
                }
                else
                {
                    SetStatus(EnemyStatus.Chasing, true);
                }
            }
            else
            {
                if (currentStatus == EnemyStatus.Patrolling)
                {
                    if (Utils.HorizontalDistance(transform.position, patrolPoints[nextPatrolPointIndex].transform.position) < maxDistancePatrollingPoint)
                    {
                        chanceToIdle = chanceToIdleDefault;
                        SetStatus(EnemyStatus.Idle, true);
                    }
                }
                else if (!EnemyLocked())
                {
                    chanceToIdle -= chanceToIdleChange;
                    if (UnityEngine.Random.value < chanceToIdle)
                    {
                        SetStatus(EnemyStatus.Idle, true);
                    }
                    else
                    {
                        SetStatus(EnemyStatus.Patrolling, true);
                    }
                }
            }
        }
        else if (!EnemyLocked())
        {
            if (PlayerIsInAttackRange())
            {
                SetStatus(EnemyStatus.Attacking, true);
            }
            else
            {
                SetStatus(EnemyStatus.Chasing, true);
            }
        }
    }

    private bool EnemyLocked()
    {
        return animationCoroutine != null;
    }

    public bool CanSeePlayer()
    {
        if (detectionDistance > 0 && detectionDistance < Utils.HorizontalDistance(eyes.position, PlayerController.GetInstance().transform.position))
        {
            return false;
        }

        Vector3 directionToPlayer = (PlayerController.GetInstance().transform.position - eyes.position);

        if (Vector3.Angle(transform.forward, directionToPlayer) > detectionAngle * 0.5f)
        {
            return false;
        }

        Ray ray = new Ray(eyes.position, directionToPlayer.normalized);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            if (hit.transform.GetComponentInParent<PlayerController>())
            {
                return true;
            }
        }

        return false;
    }

    private bool PlayerIsInAttackRange()
    {
        return Utils.HorizontalDistance(transform.position, PlayerController.GetInstance().transform.position) < attackRange;
    }

    public void Hit(Weapon weapon)
    {
        //Determine damage
        life -= 10;
        lifeText.text = life.ToString();
        if (life < 1)
        {
            SetStatus(EnemyStatus.Dead, true);
        }
        else
        {
            SetStatus(EnemyStatus.Flinching, true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(eyes.position, 0.1f);
    }
}
