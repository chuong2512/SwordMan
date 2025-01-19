using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public float attackCooldown;

    public float attackDuration;

    public float hitDelay;

    public float hitDuration;

    public float attackTimer;

    [HideInInspector]
    public bool canMeleeAttack;

    public AnimationManager animManager;

    protected GameObject _damageArea;

    protected BoxCollider2D _boxCollider2D;

    protected Collider2D _damageAreaCollider;

    protected Vector3 _gizmoOffset;

    protected DamageOnTouch _damageOnTouch;

    public Transform meleePoint;

    public Transform characterModel;

    /// the layers that will be damaged by this object
    [Tooltip("the layers that will be damaged by this object")]
    public LayerMask TargetLayerMask;

    /// the size of the damage area
    [Tooltip("the size of the damage area")]
    public Vector2 AreaSize = new Vector2(1, 1);
    /// the offset to apply to the damage area (from the weapon's attachment position
    [Tooltip("the offset to apply to the damage area (from the weapon's attachment position")]
    public Vector2 AreaOffset = new Vector2(1, 0);
    /// The amount of health to remove from the player's health
    [Tooltip("The amount of health to remove from the player's health")]
    public int DamageCaused = 10;




    private void Awake()
    {
       

    }

    public void Initialization()
    {
        attackTimer = 0.0f;
        canMeleeAttack = false;
        CreateDamageArea();
        DisableDamageArea();
    }

    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        attackTimer += Time.deltaTime;

        FlipWeapon();

        if (attackTimer >= attackCooldown)
        {
            canMeleeAttack = true;

        }



    }

    void FlipWeapon()
    {

        meleePoint.transform.localScale = new Vector3(Mathf.Sign(characterModel.localScale.x), 1, 1);

    }

    public void SwordAttack()
    {
        attackTimer = 0.0f;
        canMeleeAttack = false;
        DisableDamageArea();
        WeaponUse();
        animManager.Slash2H();
        StopAttack();
    }

    void StopAttack()
    {
        StartCoroutine(StopAttackIE());
    }

    IEnumerator StopAttackIE()
    {
        yield return new WaitForSeconds(attackDuration);
        animManager.SetState(CharacterState.Idle);
    }

    protected void WeaponUse()
    {
        StartCoroutine(MeleeWeaponAttack());
    }

    protected virtual IEnumerator MeleeWeaponAttack()
    {

        yield return new WaitForSeconds(hitDelay);
        EnableDamageArea();
        yield return new WaitForSeconds(hitDuration);
        DisableDamageArea();

    }

    protected virtual void EnableDamageArea()
    {

        _damageAreaCollider.enabled = true;
    }

    void CreateDamageArea()
    {
        _damageArea = new GameObject();
        _damageArea.name = this.name + "DamageArea";
        _damageArea.transform.position = meleePoint.position;
        _damageArea.transform.rotation = meleePoint.rotation;
        _damageArea.transform.SetParent(meleePoint);

        _boxCollider2D = _damageArea.AddComponent<BoxCollider2D>();
        _boxCollider2D.offset = AreaOffset;
        _boxCollider2D.size = AreaSize;
        _damageAreaCollider = _boxCollider2D;

        _damageAreaCollider.isTrigger = true;

        Rigidbody2D rigidBody = _damageArea.AddComponent<Rigidbody2D>();
        rigidBody.isKinematic = true;

        _damageOnTouch = _damageArea.AddComponent<DamageOnTouch>();
        _damageOnTouch.TargetLayerMask = TargetLayerMask;
        _damageOnTouch.MinDamageCaused = DamageCaused;
    }

    protected void DisableDamageArea()
    {
        _damageAreaCollider.enabled = false;
    }

    /// <summary>
    /// Draws the melee weapon's range
    /// </summary>
    protected void DrawGizmos()
    {
        _gizmoOffset = AreaOffset;
        Gizmos.color = Color.red;
        MMDebug.DrawGizmoRectangle(this.transform.position + _gizmoOffset, AreaSize, Color.red);

    }

    /// <summary>
    /// Draws gizmos on selected if the app is not playing
    /// </summary>
    protected void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            DrawGizmos();
        }
    }

}
