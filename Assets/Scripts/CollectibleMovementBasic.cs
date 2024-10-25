using System;
using UnityEngine;
using MoreMountains.CorgiEngine;

public class CollectibleMovementBasic : MonoBehaviour
{
	public float initialForceY;

	public float initialForceX;

	public float bounceForceY;

	public float bounceForceX;

	public float bounceReduce;

	private int _bounceCount;

	public int _maxBounceCount;

	private int _direction;

	private bool _isColliderEnabled;

	public LayerMask platformMask;

	public bool isGrounded;

	public CorgiController _controller;

	public float direction;

	public float halfSizeX;

	private BoxCollider2D _collider;

	public GameObject coinFx;

	public Vector3 rayPosition
	{
		get
		{
			return (direction != 1) ? (base.transform.position + Vector3.left * halfSizeX) : (base.transform.position + Vector3.right * halfSizeX);
		}
	}

	public void Start()
	{
		
		float num = UnityEngine.Random.Range(-this.initialForceX, this.initialForceX);
		float verticalVelocity = UnityEngine.Random.Range(this.initialForceY * 0.75f, this.initialForceY);
		direction = ((num <= 0f) ? (-1) : 1);
		_controller.SetVerticalForce(verticalVelocity);
		_controller.SetHorizontalForce(num);
		_collider = GetComponent<BoxCollider2D>();
	
	}

	private void Update()
	{
		isGrounded = _controller.State.IsGrounded;


		if (_controller.Speed.y <= 0f && !this._isColliderEnabled)
		{
			this._isColliderEnabled = true;
		}
		
		if (Physics2D.Raycast(this.rayPosition, Vector2.right * (float)direction, Time.deltaTime, this.platformMask))
		{
			
			direction = - direction;
			_controller.SetHorizontalForce(this.bounceForceX * (float)direction);
		}
		if (_controller.Speed.y > 0f && Physics2D.Raycast(base.transform.position, Vector2.up, 0.6f, this.platformMask))
		{
			_controller.SetVerticalForce(0f);
		}
		if (isGrounded && this._bounceCount == 0)
		{
			_controller.SetHorizontalForce(0);
			_controller.SetVerticalForce(this.bounceForceY);
			this._bounceCount++;
		}
		else if (isGrounded && this._bounceCount > 0 && this._bounceCount < this._maxBounceCount)
		{
			this.bounceForceY *= this.bounceReduce;
			_controller.SetVerticalForce(this.bounceForceY);
			this._bounceCount++;
		}
		else if (isGrounded && this._bounceCount >= this._maxBounceCount)
		{
			//UnityEngine.Object.Destroy(this);
			_controller.SetVerticalForce(0f);
			_controller.SetHorizontalForce(0);
		}
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.gameObject.tag == "Player" && _isColliderEnabled)
        {
			//Instantiate(coinFx, transform.position, Quaternion.identity);
			//Destroy(gameObject);
		}
			
    }
}
