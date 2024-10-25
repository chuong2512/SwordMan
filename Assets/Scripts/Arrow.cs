using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

	protected Vector3 _movement;

	public enum ArrowType
    {
		Line,
		Trajectory
    };

	public ArrowType currentType;

	/// the current direction of the object
	[Tooltip("the current direction of the object")]
	public Vector3 Direction = Vector3.left;

	/// the speed of the object (relative to the level's speed)
	[Tooltip("the speed of the object (relative to the level's speed)")]
	public float Speed = 200;

	/// the acceleration of the object over time. Starts accelerating on enable.
	[Tooltip("the acceleration of the object over time. Starts accelerating on enable.")]
	public float Acceleration = 0;

	protected BoxCollider2D _collider;

	protected RaycastHit2D _hit2D;

	/// the layermask to use when performing the security check
	[Tooltip("the layermask to use when performing the security check")]
	public LayerMask SpawnSecurityCheckLayerMask;

	protected DamageOnTouch _damageOnTouch;

	protected ShootAttack _weapon;

	protected GameObject _owner;

	/// should the projectile damage its owner ?
	[Tooltip("should the projectile damage its owner ?")]
	public bool DamageOwner = false;

	protected bool _spawnerIsFacingRight;

	/// if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.
	[Tooltip("if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.")]
	public bool DirectionCanBeChangedBySpawner = true;

	/// determines whether or not the projectile is facing right
	[Tooltip("determines whether or not the projectile is facing right")]
	public bool ProjectileIsFacingRight = true;

	/// if true, the projectile will rotate at initialization towards its rotation
	[Tooltip("if true, the projectile will rotate at initialization towards its rotation")]
	public bool FaceDirection = true;

	public SpriteRenderer _spriteRenderer;

	/// the flip factor to apply if and when the projectile is mirrored
	[Tooltip("the flip factor to apply if and when the projectile is mirrored")]
	public Vector3 FlipValue = new Vector3(-1, 1, 1);


	public Transform targetTrs;

	[Tooltip("Position we want to hit")]
	private Vector3 targetPos;

	[Tooltip("How high the arc should be, in units")]
	public float arcHeight = 1;


	Vector3 startPos;
	private Vector3 nextPos;

	// Start is called before the first frame update
	void Awake()
    {
		
		_collider = GetComponent<BoxCollider2D>();
		_damageOnTouch = GetComponent<DamageOnTouch>();

		startPos = transform.position;

		if (targetTrs != null)
			targetPos = targetTrs.position;

	}

    // Update is called once per frame
    void Update()
    {
        
    }

	/// <summary>
	/// On FixedUpdate(), we move the object based on the level's speed and the object's speed, and apply acceleration
	/// </summary>
	protected virtual void FixedUpdate()
	{
		Movement();
	}

	/// <summary>
	/// Handles the projectile's movement, every frame
	/// </summary>
	public virtual void Movement()
	{
		switch(currentType)
        {
			case ArrowType.Line:
				_movement = Direction * (Speed / 10) * Time.deltaTime;
				transform.Translate(_movement, Space.World);
				// We apply the acceleration to increase the speed
				Speed += Acceleration * Time.deltaTime;
				break;

			case ArrowType.Trajectory:
				// Compute the next position, with arc added in
				float x0 = startPos.x;
				float x1 = targetPos.x;
				float dist = x1 - x0;
				float nextX = Mathf.MoveTowards(transform.position.x, x1, Speed * Time.deltaTime);
				float baseY = Mathf.Lerp(startPos.y, targetPos.y, (nextX - x0) / dist);
				float arc = arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
				nextPos = new Vector3(nextX, baseY + arc, transform.position.z);

				// Rotate to face the next position, and then move there
				transform.rotation = LookAt2D(nextPos - transform.position);
				transform.position = nextPos;

				// Do something when we reach the target
				if (nextPos == targetPos) Arrived();
				break;
        }
		
	}

	void Arrived()
	{
		gameObject.SetActive(false);
	}

	protected Quaternion LookAt2D(Vector2 forward)
	{
		return Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
	}

	/// <summary>
	/// Performs a local check to see if the projectile is within a collider or not
	/// </summary>
	protected void CheckForCollider()
	{
		

		if (_collider == null)
		{
			return;
		}

		_hit2D = Physics2D.BoxCast(this.transform.position, _collider.bounds.size, this.transform.eulerAngles.z, Vector3.forward, 1f, SpawnSecurityCheckLayerMask);
		if (_hit2D)
		{
			gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// On enable, we reset the object's speed
	/// </summary>
	protected void OnEnable()
	{
	

		if (_damageOnTouch != null)
		{
			_damageOnTouch.OnKill += OnKill;
			_damageOnTouch.OnHit += OnHit;
			_damageOnTouch.OnHitDamageable += OnHitDamageable;
			_damageOnTouch.OnHitNonDamageable += OnHitNonDamageable;
		}
	}

	/// <summary>
	/// On disable, we unsubscribe from our delegates
	/// </summary>
	protected void OnDisable()
	{
		
		if (_damageOnTouch != null)
		{
			_damageOnTouch.OnKill -= OnKill;
			_damageOnTouch.OnHit -= OnHit;
			_damageOnTouch.OnHitDamageable -= OnHitDamageable;
			_damageOnTouch.OnHitNonDamageable -= OnHitNonDamageable;
		}
	}

	/// <summary>
	/// On hit, we trigger a hit on our owner weapon
	/// </summary>
	protected virtual void OnHit()
	{
	
	}

	/// <summary>
	/// On kill, we trigger a kill on our owner weapon
	/// </summary>
	protected virtual void OnKill()
	{
		
	}

	/// <summary>
	/// On hit damageable, we trigger a hit damageable on our owner weapon
	/// </summary>
	protected virtual void OnHitDamageable()
	{
		
	}

	/// <summary>
	/// On hit non damageable, we trigger a hit non damageable on our owner weapon
	/// </summary>
	protected virtual void OnHitNonDamageable()
	{
		
	}

	public virtual void SetWeapon(ShootAttack newWeapon)
	{
		_weapon = newWeapon;
	}

	public virtual void SetOwner(GameObject newOwner)
	{
		_owner = newOwner;
		DamageOnTouch damageOnTouch = this.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();
		if (damageOnTouch != null)
		{
			damageOnTouch.Owner = newOwner;
			if (!DamageOwner)
			{
				damageOnTouch.ClearIgnoreList();
				damageOnTouch.IgnoreGameObject(newOwner);
			}
		}
	}

	public void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight)
	{
		
		if(currentType == ArrowType.Line)
        {
			Direction = newDirection;
			Flip(spawnerIsFacingRight);
		}	
		
	}

	protected void Flip(bool isFacingRight)
	{
		
	 _spriteRenderer.flipX = isFacingRight;
		
		
	}
}
