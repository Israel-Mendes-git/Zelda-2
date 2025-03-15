using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SlimeIa : MonoBehaviour
{
    private GameManager _gm;

    private Animator anim;
    public int HP;
    private bool isDie;
    

    public enemyState state;

    private bool isWalk;
    private bool isAlert;
    private bool isPlayerVisible;
    private bool isAttack;

    

    private NavMeshAgent agent;
    private int idWayPoint;
    private Vector3 destination;


    void Start()
    {
        anim = GetComponent<Animator>();

        _gm = FindObjectOfType(typeof(GameManager)) as GameManager;
        agent = GetComponent<NavMeshAgent>();

        //ChangeState(state);
    }
    void Update()
    {
        StateManager();

        if(agent.desiredVelocity.magnitude >= 0.1f)
        {
            isWalk = true;
        }
        else
        {
            isWalk = false;
        } 
        anim.SetBool("isWalk", isWalk);
        anim.SetBool("isAlert", isAlert);
           
    }

    IEnumerator Died()
    {
        isDie = true;
        yield return new WaitForSeconds(2.3f);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_gm.gameState != GameState.GAMEPLAY) { return; }

        if(other.gameObject.tag == "Player")  
        {
            isPlayerVisible = true;
            if (state == enemyState.IDLE || state == enemyState.PATROL)
            {
                ChangeState(enemyState.ALERT);
            } else if (state == enemyState.FOLLOW)
            {
                StopCoroutine("Follow");
                ChangeState(enemyState.FOLLOW);
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if( other.gameObject.tag == "Player")
        {
            isPlayerVisible = false;
        }
    }


    #region MEUS MÉTODOS
    void GetHit(int amount)
    {
        HP -= amount;

        if (isDie == true)
        {
            return;
        }
        if (HP > 0)
        {
            ChangeState(enemyState.FURY);
            anim.SetTrigger("GetHit");

        }
        else
        {
            ChangeState(enemyState.DIE);
            anim.SetTrigger("Die");
            StartCoroutine("Died");
        }

    }

    void StateManager()
    {
        if(_gm.gameState == GameState.DIE && state == enemyState.FOLLOW || state == enemyState.FURY || state == enemyState.ALERT)
        {
            ChangeState(enemyState.IDLE);
        }

        switch (state)
        {
            case enemyState.ALERT:
                LookAt();
                break;

            case enemyState.PATROL:
                break;

            case enemyState.FURY:
                LookAt();
                destination = _gm.player.position;
                agent.destination = destination;

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!isAttack)  // Apenas ataca se não estiver em cooldown
                    {
                        Attack();
                    }
                }
                break;


            case enemyState.FOLLOW:
                LookAt();
                destination = _gm.player.position;
                agent.destination = destination;

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!isAttack)  // Evita múltiplos ataques seguidos sem cooldown
                    {
                        Attack();
                    }
                }
                break;
        }
    }


    void ChangeState(enemyState newState)
    {
        StopAllCoroutines();
        state = newState;
        isAlert = false;
        print(newState);
        switch (state)
        {
            case enemyState.IDLE:
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;

                StartCoroutine("IDLE");
                break;
            case enemyState.ALERT:
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;
                isAlert = true;

                StartCoroutine("ALERT");

                break;
            case enemyState.PATROL:
                agent.stoppingDistance = 0;
                idWayPoint = Random.Range(0, _gm.slimeWayPoints.Length);
                destination = _gm.slimeWayPoints[idWayPoint].position;
                agent.destination = destination;

                StartCoroutine("PATROL");
                break;

            case enemyState.FURY:
                agent.stoppingDistance = _gm.slimeDistanceToAttack;
                isAttack = false; // Permitir ataque
                StartCoroutine("FOLLOW"); // Faz ele seguir o jogador
                break;


            case enemyState.FOLLOW:
                isAttack = true;
                agent.stoppingDistance = _gm.slimeDistanceToAttack;

                StartCoroutine("FOLLOW");
                StartCoroutine("AttackCooldown");
                break;
        
            case enemyState.DIE:
                destination = transform.position;
                agent.destination = destination;
                break;

        }



    }
    

    IEnumerator IDLE()
    {
        yield return new WaitForSeconds(_gm.slimeIdleWaitTime);
        StayStill(50);

    }

    IEnumerator PATROL()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= 0 ) ;
        StayStill(30);

    }

    IEnumerator FOLLOW()
    {
        yield return new WaitUntil(() => !isPlayerVisible);
        print("perdi tu");

        yield return new WaitForSeconds(_gm.slimeAlertTime);
        StayStill(50); 

    }
    IEnumerator ALERT()
    {
        yield return new WaitForSeconds(_gm.slimeAlertTime);

        if(isPlayerVisible == true)
        {
            ChangeState(enemyState.FOLLOW);
        }
        else
        {
            StayStill(10);
        }
    }

    void StayStill(int yes)
    {
        if(Rand() <= yes)
        {
            ChangeState(enemyState.IDLE);
        }
        else
        {
            ChangeState(enemyState.PATROL);
        }


    }

    int Rand()
    {
        int rand = Random.Range(0, 100);
        return rand;
    }

    void Attack()
    {
        if (!isAttack && isPlayerVisible)
        {
            isAttack = true;
            anim.SetTrigger("Attack");
            StartCoroutine(AttackCooldown());
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_gm.slimeAttackDelay);
        isAttack = false; // Permite que o ataque seja realizado novamente
    }


    void AttackIsDone()
    {
        StartCoroutine("AttackCooldown");
    }

    void LookAt()
    { 
        Vector3 lookDirection = (_gm.player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _gm.slimeLookAtSpeed * Time.deltaTime);
    }
    #endregion

}
