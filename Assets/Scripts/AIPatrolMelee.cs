using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPatrolMelee : MonoBehaviour
{
    [Header("Obstacle Detection")]
    /// If set to true, the agent will change direction when hitting a wall
    [Tooltip("If set to true, the agent will change direction when hitting a wall")]
    public bool ChangeDirectionOnWall = true;
    /// If set to true, the agent will try and avoid falling
    [Tooltip("If set to true, the agent will try and avoid falling")]
    public bool AvoidFalling = false;
    /// The offset the hole detection should take into account
    [Tooltip("The offset the hole detection should take into account")]
    public Vector3 HoleDetectionOffset = new Vector3(0, 0, 0);
    /// the length of the ray cast to detect holes
    [Tooltip("the length of the ray cast to detect holes")]
    public float HoleDetectionRaycastLength = 1f;

    [Header("Layermasks")]
    /// Whether to use a custom layermask, or simply use the platform mask defined at the character level
    [Tooltip("Whether to use a custom layermask, or simply use the platform mask defined at the character level")]
    public bool UseCustomLayermask = false;
    /// if using a custom layer mask, the list of layers considered as obstacles by this AI
    [Tooltip("if using a custom layer mask, the list of layers considered as obstacles by this AI")]
    [MMCondition("UseCustomLayermask", true)]
    public LayerMask ObstaclesLayermask = LayerManager.ObstaclesLayerMask;
    /// the length of the horizontal raycast we should use to detect obstacles that may cause a direction change
    [Tooltip("the length of the horizontal raycast we should use to detect obstacles that may cause a direction change")]
    [MMCondition("UseCustomLayermask", true)]
    public float ObstaclesDetectionRaycastLength = 0.5f;
    /// the origin of the raycast (if casting against the same layer this object is on, the origin should be outside its collider, typically in front of it)
    [Tooltip("the origin of the raycast (if casting against the same layer this object is on, the origin should be outside its collider, typically in front of it)")]
    [MMCondition("UseCustomLayermask", true)]
    public Vector2 ObstaclesDetectionRaycastOrigin = new Vector2(0.5f, 0f);

    [Header("Revive")]
    /// if this is true, the character will automatically return to its initial position on revive
    [Tooltip("if this is true, the character will automatically return to its initial position on revive")]
    public bool ResetPositionOnDeath = true;

    // private stuff
    protected CorgiController _controller;
    protected Character _character;
    protected Health _health;
    protected CharacterHorizontalMovement _characterHorizontalMovement;
    protected Vector2 _direction;
    protected Vector2 _startPosition;
    protected Vector2 _initialDirection;
    protected Vector3 _initialScale;
    protected float _distanceToTarget;
    protected Vector2 _raycastOrigin;
    protected RaycastHit2D _raycastHit2D;
    protected Vector2 _obstacleDirection;

    /// the radius to search our target in
    [Tooltip("the radius to search our target in")]
    public float Radius = 3f;
    /// the center of the search circle
    [Tooltip("the center of the search circle")]
    public Vector3 DetectionOriginOffset = new Vector3(0, 0, 0);
    /// the layer(s) to search our target on
    [Tooltip("the layer(s) to search our target on")]
    public LayerMask TargetLayer = LayerManager.PlayerLayerMask;

    protected Vector2 _facingDirection;
    protected Collider2D _detectionCollider = null;
    protected Color _gizmoColor = Color.yellow;
    protected bool _init = false;

    private enum STATE
    {
        NONE,
        PATROL,
        CHARGE,
        ATTACK
    }

    private STATE currentState;

    public enum EnemeyType
    {
        STAND,
        PATROL,
        MELEE,
        SHOOT
    }

    public enum DetectType
    {
        Circle, Ray
    };

    public DetectType currentDetectType;

    public AIDecision detection;


    public EnemeyType currentType;

    public Animator animator;

    //timing

    public float chargeDuration;

    public float attackDuration;

    protected float timer;

    public Transform target;

    public Transform ownerModel;

    public MeleeAttack meleeAttack;

    public ShootAttack shootAttack;

    public Character.FacingDirections initFace;


    private void Awake()
    {
        Initialization();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(initFace == Character.FacingDirections.Left)
         _character.Flip();
    }

    // Update is called once per frame
    void Update()
    {
        PerformAction();
       
    }

    public void PerformAction()
    {
       

        switch(currentState)
        {
            case STATE.PATROL:

                if(currentType != EnemeyType.STAND)
                {
                    if (!Decide())
                    {
                        Patrol();

                    }

                    else
                    {

                        Charge();
                    }
                }
               
                else
                {
                    Charge();
                }
                   

                break;

            case STATE.CHARGE:

                timer += Time.deltaTime;
                if(timer >= chargeDuration)
                {
                    timer = 0.0f;
                    StartAttack();
                }
                break;

            case STATE.ATTACK:

                timer += Time.deltaTime;
                if (timer >= attackDuration)
                {
                    timer = 0.0f;
                    StopAttack();
                }

                break;
        }
        
    }


    public void Initialization()
    {
        // we get the CorgiController2D component
        _controller = GetComponent<CorgiController>();
        _character = GetComponent<Character>();
        _characterHorizontalMovement = _character?.FindAbility<CharacterHorizontalMovement>();
        _health = _character.CharacterHealth;
        // initialize the start position
        _startPosition = transform.position;
        // initialize the direction
        _direction = _character.IsFacingRight ? Vector2.right : Vector2.left;

        _initialDirection = _direction;
        _initialScale = transform.localScale;

        _gizmoColor.a = 0.25f;
        _init = true;

        currentState = STATE.PATROL;
        if (meleeAttack)
            meleeAttack.Initialization();
        if (shootAttack)
            shootAttack.Initialization();
        detection.Initialization();
       
    }

    /// <summary>
    /// This method initiates all the required checks and moves the character
    /// </summary>
    protected void Patrol()
    {
        if (_character == null)
        {
            return;
        }
        if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
            || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
        {
            return;
        }
        // moves the agent in its current direction
        CheckForWalls();
        CheckForHoles();
        _characterHorizontalMovement.SetHorizontalMove(_direction.x);
        PlayRunAnimation();

    }

    protected void Charge()
    {
         currentState = STATE.CHARGE;
         timer = 0.0f;
         PlayIdleAnimation();

        if (currentType == EnemeyType.STAND)
            return;

        _characterHorizontalMovement.SetHorizontalMove(0);


        if((Mathf.Sign(ownerModel.localScale.x) > 0 && (transform.position.x > target.transform.position.x))
            || (Mathf.Sign(ownerModel.localScale.x) < 0 && (transform.position.x < target.transform.position.x)))
        {
            _character.Flip();
            ChangeDirection();
        }
    }

    protected void StartAttack()
    {
        if(currentType == EnemeyType.STAND)
        {
            //Debug.Log("START TO ATTACK");
            currentState = STATE.ATTACK;
            if (meleeAttack != null)
                meleeAttack.SwordAttack();
            if (shootAttack != null)
                shootAttack.ProcessShoot();
        }

        else
        {
            if (!Decide())
            {
                currentState = STATE.PATROL;
                Patrol();
            }
            else
            {
                currentState = STATE.ATTACK;
                if (currentType == EnemeyType.MELEE)
                {
                    if (meleeAttack.canMeleeAttack)
                        meleeAttack.SwordAttack();
                }

                else if (currentType == EnemeyType.SHOOT)
                {
                    if (shootAttack.canShoot)
                        shootAttack.ProcessShoot();

                }
                
            }

        }


    }

    protected void StopAttack()
    {
        if (currentType == EnemeyType.STAND)
            Charge();
        else
        {
            if (!Decide())
            {
                currentState = STATE.PATROL;
                Patrol();
            }
            else
            {
                Charge();
            }
        }
       
    }

    void PlayRunAnimation()
    {
        animator.SetBool("Walking", true);
        animator.SetBool("Idle", false);
        animator.SetBool("Attack", false);
       
    }

    void PlayIdleAnimation()
    {
        animator.SetBool("Walking", false);
        animator.SetBool("Idle", true);
        animator.SetBool("Attack", false);
       
    }

   

    protected virtual void CheckForWalls()
    {
        if (!ChangeDirectionOnWall)
        {
            return;
        }

        if (UseCustomLayermask)
        {
            if (DetectObstaclesCustomLayermask())
            {
                ChangeDirection();
            }
        }
        else
        {
            // if the agent is colliding with something, make it turn around
            if (DetectObstaclesRegularLayermask())
            {
                ChangeDirection();
            }
        }
    }

    /// <summary>
    /// Checks for holes 
    /// </summary>
    protected virtual void CheckForHoles()
    {
        // if we're not grounded or if we're not supposed to check for holes, we do nothing and exit
        if (!AvoidFalling || !_controller.State.IsGrounded)
        {
            return;
        }

        // we send a raycast at the extremity of the character in the direction it's facing, and modified by the offset you can set in the inspector.

        if (_character.IsFacingRight)
        {
            _raycastOrigin = transform.position + (_controller.Bounds.x / 2 + HoleDetectionOffset.x) * transform.right + HoleDetectionOffset.y * transform.up;
        }
        else
        {
            _raycastOrigin = transform.position - (_controller.Bounds.x / 2 + HoleDetectionOffset.x) * transform.right + HoleDetectionOffset.y * transform.up;
        }

        if (UseCustomLayermask)
        {
            _raycastHit2D = MMDebug.RayCast(_raycastOrigin, -transform.up, HoleDetectionRaycastLength, ObstaclesLayermask, Color.gray, true);
        }
        else
        {
            _raycastHit2D = MMDebug.RayCast(_raycastOrigin, -transform.up, HoleDetectionRaycastLength, _controller.PlatformMask | _controller.MovingPlatformMask | _controller.OneWayPlatformMask | _controller.MovingOneWayPlatformMask, Color.gray, true);
        }

        // if the raycast doesn't hit anything
        if (!_raycastHit2D)
        {
            // we change direction
            ChangeDirection();
        }
    }

    /// <summary>
    /// Changes the current movement direction
    /// </summary>
    protected virtual void ChangeDirection()
    {
        _direction = -_direction;
       
    }

    /// <summary>
    /// Returns true if an obstacle is colliding with this AI, using its controller layer masks
    /// </summary>
    /// <returns></returns>
    protected bool DetectObstaclesRegularLayermask()
    {
        return (_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight);
    }


    /// <summary>
    /// Returns true if an obstacle is in front of the character, using a custom layer mask
    /// </summary>
    /// <returns></returns>
    protected bool DetectObstaclesCustomLayermask()
    {
        if (_character.IsFacingRight)
        {
            _raycastOrigin = transform.position + (_controller.Bounds.x / 2 + ObstaclesDetectionRaycastOrigin.x) * transform.right + ObstaclesDetectionRaycastOrigin.y * transform.up;
            _obstacleDirection = Vector2.right;
        }
        else
        {
            _raycastOrigin = transform.position - (_controller.Bounds.x / 2 + ObstaclesDetectionRaycastOrigin.x) * transform.right + ObstaclesDetectionRaycastOrigin.y * transform.up;
            _obstacleDirection = Vector2.left;
        }

        _raycastHit2D = MMDebug.RayCast(_raycastOrigin, _obstacleDirection, ObstaclesDetectionRaycastLength, ObstaclesLayermask, Color.gray, true);

        return _raycastHit2D;
    }

    public bool Decide()
    {
        if (detection.Decide())
            target = detection.targetTransform;

        return detection.Decide();
    }

  
}
