using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

public class SwordManController : MonoBehaviour
{
    public Character _character;

    public CorgiController _corgiController;

    public Animator animator;

    protected InputManager _inputManager;

    public MeleeAttack meleeAttack;


    // Start is called before the first frame update
    void Start()
    {
        _character = this.gameObject.GetComponentInParent<Character>();
        if (_character != null)
        {
            _inputManager = _character.LinkedInputManager;
        }

        if (meleeAttack)
            meleeAttack.Initialization();
    }

    // Update is called once per frame
    void Update()
    {

        UpdateAnimations();

        if ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonDown)
            && meleeAttack.canMeleeAttack)
        {
            meleeAttack.SwordAttack();
        }
    }


    void UpdateAnimations()
    {
        animator.SetFloat("ySpeed", _corgiController.Speed.y);
        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Idle)
        {
            animator.SetBool("Walking", false);
            animator.SetBool("Grounded", true);
            animator.SetBool("Idle", true);
        }

        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Walking)
        {
            animator.SetBool("Walking", true);
            animator.SetBool("Grounded", true);
            animator.SetBool("Idle", false);
        }

        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Jumping)
        {
            animator.SetBool("Walking", false);
            animator.SetBool("Idle", false);
            animator.SetBool("Grounded", false);
        }

        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Falling)
        {
            animator.SetBool("Walking", false);
            animator.SetBool("Idle", false);
            animator.SetBool("Grounded", false);
        }
    }
}
