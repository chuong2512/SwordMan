using System.Collections;
using System.Collections.Generic;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using Character = MoreMountains.CorgiEngine.Character;

public class SwordManController : MonoBehaviour
{
    public Character _character;

    public CorgiController _corgiController;

    public AnimationManager animManager;

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
        /*animator.SetFloat("ySpeed", _corgiController.Speed.y);*/
        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Idle)
        {
            /*animator.SetBool("Walking", false);
            animator.SetBool("Grounded", true);
            animator.SetBool("Idle", true);*/
            animManager.SetState(CharacterState.Idle);
        }

        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Walking)
        {
            /*animator.SetBool("Walking", true);
            animator.SetBool("Grounded", true);
            animator.SetBool("Idle", false);*/
            animManager.SetState(CharacterState.Walk);
        }

        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Jumping)
        {
            /*animator.SetBool("Walking", false);
            animator.SetBool("Idle", false);
            animator.SetBool("Grounded", false);*/
            animManager.SetState(CharacterState.Jump);
        }

        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Falling)
        {
            /*
            animator.SetBool("Walking", false);
            animator.SetBool("Idle", false);
            animator.SetBool("Grounded", false);
            */
            
            animManager.SetState(CharacterState.Ready);
        }
    }
}
