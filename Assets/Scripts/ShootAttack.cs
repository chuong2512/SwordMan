using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAttack : MonoBehaviour
{
    /// A MMFeedback to play when the weapon hits anything (damageable or not) 
    [Tooltip("A MMFeedback to play when the weapon hits anything (damageable or not)")]
    public MMFeedbacks WeaponOnHitFeedback;
    /// A MMFeedback to play when the weapon misses (what constitutes a miss is defined per Weapon subclass)
    [Tooltip("A MMFeedback to play when the weapon misses (what constitutes a miss is defined per Weapon subclass)")]
    public MMFeedbacks WeaponOnMissFeedback;
    /// A MMFeedback to play when the weapon hits a damageable
    [Tooltip("A MMFeedback to play when the weapon hits a damageable")]
    public MMFeedbacks WeaponOnHitDamageableFeedback;
    /// A MMFeedback to play when the weapon hits a non damageable object
    [Tooltip("A MMFeedback to play when the weapon hits a non damageable object")]
    public MMFeedbacks WeaponOnHitNonDamageableFeedback;
    /// A MMFeedback to play when the weapon kills something
    [Tooltip("A MMFeedback to play when the weapon kills something")]
    public MMFeedbacks WeaponOnKillFeedback;

    public float fireDelay, shootDuration, shootAnimDuration;

    protected float shootTimer;

    public bool canShoot;

    public MMObjectPooler ObjectPooler { get; set; }

    public Character Owner { get; protected set; }

    public bool Flipped;

    public float flipOffset;
  
    public Transform SpawnPosition;

    public Animator animator;

    public Transform targetThrow;

    private void Awake()
    {
       
       
    }

    public void Initialization()
    {
        if (GetComponent<MMSimpleObjectPooler>() != null)
        {
            ObjectPooler = GetComponent<MMSimpleObjectPooler>();
        }
        if (ObjectPooler == null)
        {
            Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
            return;
        }
        Owner = GetComponent<Character>();
        DetermineSpawnPosition();
        shootTimer = 0.0f;
        canShoot = false;
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (!canShoot)
            shootTimer += Time.deltaTime;

        if (shootTimer >= shootDuration)
        {
            canShoot = true;
        }

        Flipped = Owner.IsFacingRight;
    }


    public void ProcessShoot()
    {
        if (!canShoot)
            return;
        canShoot = false;
        shootTimer = 0.0f;
        StartToShoot();
    }

    void StartToShoot()
    {
        DetermineSpawnPosition();
        PlayShootAnimation();
        StartCoroutine(SpawnProjectileDelay());
        StartCoroutine(StopShoot());
    }

    IEnumerator StopShoot()
    {
        yield return new WaitForSeconds(shootAnimDuration);
        StopShootAnimation();
    }

    IEnumerator SpawnProjectileDelay()
    {
        yield return new WaitForSeconds(fireDelay);
        SpawnProjectile();
    }

    void PlayShootAnimation()
    {
        animator.SetBool("Attack", true);
    }

    void StopShootAnimation()
    {
        animator.SetBool("Attack", false);
    }

    public GameObject SpawnProjectile()
    {
       
        
        /// we get the next object in the pool and make sure it's not null
        GameObject nextGameObject = ObjectPooler.GetPooledGameObject();
       
        // mandatory checks
        if (nextGameObject == null) { return null; }
        if (nextGameObject.GetComponent<MMPoolableObject>() == null)
        {
            //throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
        }
        // we position the object
        nextGameObject.transform.position = SpawnPosition.position;
        // we set its direction

        Arrow projectile = nextGameObject.GetComponent<Arrow>();
        if (projectile != null)
        {
            projectile.SetWeapon(this);
            if (Owner != null)
            {
                projectile.SetOwner(Owner.gameObject);
                
                projectile.SetDirection(transform.right * (!Owner.IsFacingRight ? -1 : 1), transform.rotation, !Owner.IsFacingRight);
            }

            if (targetThrow != null)
                projectile.targetTrs = targetThrow;
        }
        // we activate the object
        nextGameObject.gameObject.SetActive(true);

      

        return (nextGameObject);
    }

    /// <summary>
    /// Determines the spawn position based on the spawn offset and whether or not the weapon is flipped
    /// </summary>
    public void DetermineSpawnPosition()
    {

        if (!Owner.IsFacingRight)
        
            SpawnPosition.localPosition = new Vector3(-flipOffset, SpawnPosition.localPosition.y, SpawnPosition.localPosition.z);
        
        else
            SpawnPosition.localPosition = new Vector3(flipOffset, SpawnPosition.localPosition.y, SpawnPosition.localPosition.z);
    }
}
