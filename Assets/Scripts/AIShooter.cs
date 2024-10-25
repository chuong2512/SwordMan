using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIShooter : MonoBehaviour
{
    public enum EnemyType
    {
        Standing,
        Patrol
    };

    public EnemyType currentType;

    protected enum State
    {

        Idle,
        Run,
        Attack
    };

    protected State currentState;

    protected float sleepingTimer;

    public float idleDuration, attackDuration;

   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  

}
