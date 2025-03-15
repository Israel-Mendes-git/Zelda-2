using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameManager _gm;
    private CharacterController controller;
    private Animator anim;

    [Header("Config Player")]
    public int HP;
    public float movementSpeed = 3f;
    private Vector3 direction;
    private bool isWalk;

    private float horizontal;
    private float vertical;

    [Header("Attack Config")]
    public ParticleSystem fxAttack;
    public Transform hitbox;
    [Range(0.2f, 1f)]
    public float hitRange = 0.5f;
    public LayerMask hitMask;
    private bool isAttack;
    public Collider[] hitInfo;
    public int amountDmg;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        _gm = FindObjectOfType(typeof(GameManager)) as GameManager;
    }

    // Update is called once per frame
    void Update()
    {
        if(_gm.gameState != GameState.GAMEPLAY) { return; }

        Inputs();

        MoveCharacter();

        UpdateAnimator();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "TakeDamage")
        {
            GetHit(1);
        }
    }

    #region MEUS MÉTODOS 

    void Inputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Fire1") && isAttack == false)
        {
            Attack();
        }
        
    }

    void Attack()
    {
        
        isAttack = true;
        anim.SetTrigger("Attack");
        fxAttack.Play();

        hitInfo = Physics.OverlapSphere(hitbox.position, hitRange, hitMask);

        foreach (Collider c in hitInfo)
        {
            c.gameObject.SendMessage("GetHit", amountDmg, SendMessageOptions.DontRequireReceiver);
        }


    }

    void MoveCharacter()
    {
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }
        controller.Move(direction * movementSpeed * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        anim.SetBool("isWalk", isWalk);
    }


    void AttackIsDone()
    {
        isAttack = false;
    }

    void GetHit(int amount)
    {
        HP -= amount;
        if(HP > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            _gm.ChangeGameState(GameState.DIE);
            anim.SetTrigger("Die");
        }
    }
    #endregion


    private void OnDrawGizmosSelected()
    {
        if (hitbox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitbox.position, hitRange);
        }
    }

}
