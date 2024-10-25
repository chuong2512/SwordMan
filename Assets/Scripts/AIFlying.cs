using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class AIFlying : MonoBehaviour
{
    /// the minimum distance from the target this Character can reach.
    [Tooltip("the minimum distance from the target this Character can reach.")]
    public float MinimumDistance = 1f;

    protected CharacterFly _characterFly;


    public enum State
    {
        IDLE,
        FLYING_TOWARD,
        ATTACK
    }

    public State currentState;


    public Animator animator;

    public AIDecisionDetectTargetRadius decisionDetect;

    public Transform characterModel;

    public float attackCoolDown, attackDuration;

    protected float attackTimer;

    protected DamageOnTouch damageOnTouch;
    /// <summary>
    /// On init we grab our CharacterFly ability
    /// </summary>
    public void Initialization()
    {
        _characterFly = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterFly>();
        damageOnTouch = GetComponent<DamageOnTouch>();
        decisionDetect.Initialization();
        currentState = State.IDLE;
        IdleAnimation();
        attackTimer = 0.0f;
    }

    private void Awake()
    {
        Initialization();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(currentState)
        {
            case State.IDLE:
                IdleState();
                break;

            case State.FLYING_TOWARD:

                FlyingToward();
                FaceToTarget();

                break;

            case State.ATTACK:

                ProcessingAttack();
              
                break;
        }
    }

    protected void IdleAnimation()
    {
        animator.SetBool("Idle", true);
        animator.SetBool("Flying", false);
    }

    protected void FlyingAnimation()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Flying", true);
    }

    protected void AttackAnimation()
    {
        animator.SetBool("Attack", true);
       
    }

    protected void IdleState()
    {
        if (!decisionDetect.Decide())
            return;
        else
        {
            currentState = State.FLYING_TOWARD;
            FlyingAnimation();
        }
            
    }

    protected void FlyingToward()
    {
        if (decisionDetect.targetTransform == null)
            return;

        if (this.transform.position.x < decisionDetect.targetTransform.position.x)
        {
            _characterFly.SetHorizontalMove(1f);
        }
        else
        {
            _characterFly.SetHorizontalMove(-1f);
        }

        if (this.transform.position.y < decisionDetect.targetTransform.position.y)
        {
            _characterFly.SetVerticalMove(1f);
        }
        else
        {
            _characterFly.SetVerticalMove(-1f);
        }

        if (Mathf.Abs(this.transform.position.x - decisionDetect.targetTransform.position.x) < MinimumDistance)
        {
            _characterFly.SetHorizontalMove(0f);
        }

        if (Mathf.Abs(this.transform.position.y - decisionDetect.targetTransform.position.y) < MinimumDistance)
        {
            _characterFly.SetVerticalMove(0f);
        }

        if (Mathf.Abs(this.transform.position.x - decisionDetect.targetTransform.position.x) < MinimumDistance
            && Mathf.Abs(this.transform.position.y - decisionDetect.targetTransform.position.y) < MinimumDistance)
            currentState = State.ATTACK;
    
    }

    protected void FaceToTarget()
    {
        if (decisionDetect.targetTransform == null)
            return;

        if (this.transform.position.x < decisionDetect.targetTransform.position.x)
        {
            characterModel.localScale = new Vector3(1, 1, 1);
        }
        else
            characterModel.localScale = new Vector3(-1, 1, 1);
    }


    private void StartToAttack()
    {
        AttackAnimation();
    }

    IEnumerator StopAttack()
    {
        yield return new WaitForSeconds(attackDuration);
        animator.SetBool("Attack", false);
        animator.SetBool("Idle", true);
        if(decisionDetect.targetTransform != null)
        {
            damageOnTouch.ApplyDamage(decisionDetect.targetTransform.GetComponent<BoxCollider2D>());

        }
       
    }

    protected void ProcessingAttack()
    {

        if (Mathf.Abs(this.transform.position.x - decisionDetect.targetTransform.position.x) < MinimumDistance
   && Mathf.Abs(this.transform.position.y - decisionDetect.targetTransform.position.y) < MinimumDistance)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackCoolDown)
            {
                attackTimer = 0.0f;
                StartToAttack();
                StartCoroutine(StopAttack());
            }
        }

        else
            currentState = State.FLYING_TOWARD;
    }
   
}
