using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    public CharacterType characterType;
    public StateEnums currentState;
    public NavMeshAgent navMeshAgent;
    [HideInInspector] public Transform target;

    [Header("Striker")] public int currentBaseCount = -1;

    [Header("Pitcher")] public GameObject ball;
    public Transform ballSpawnPosition;

    [HideInInspector] public Vector3 guardStartPosition;
    public enum CharacterType
    {
        Striker,
        Catcher,
        Guard,
        Pitcher
    }

    public enum StateEnums
    {
        Idle,
        Run,
    }

    public enum RunTypes
    {
        RunToBase,
        RunToBall,
    }

    void Start()
    {
        if(characterType == CharacterType.Striker)
            MGameManager.Instance.strikers.Add(this);
        if (characterType == CharacterType.Guard)
            guardStartPosition = transform.position;
       
        if (characterType == CharacterType.Pitcher) 
            PitcherThrowBall();
    }

    // Update is called once per frame
    void Update()
    {
        
        StateHandler();
    }

    public void StateHandler()
    {
        switch (characterType)
        {
            case CharacterType.Striker:

                switch (currentState)
                {
                    case StateEnums.Run:
                        MoveToTarget(target, RunTypes.RunToBase);
                        break;
                }

                break;
            case CharacterType.Catcher:


                break;
            case CharacterType.Guard:

                switch (currentState)
                {
                    case StateEnums.Run:
                        Debug.Log("a");
                        MoveToTarget(target, RunTypes.RunToBall);
                        break;
                }

                break;
            case CharacterType.Pitcher:


                break;
        }
    }

    public void ChangeState(StateEnums targetState)
    {
        switch (characterType)
        {
            case CharacterType.Striker:


                break;
            case CharacterType.Catcher:


                break;
            case CharacterType.Guard:
                
                break;
            case CharacterType.Pitcher:


                break;
        }

        currentState = targetState;
    }

    private void MoveToTarget(Transform targetTransform, RunTypes runType)
    {
        if (target == null)
        {
            switch (characterType)
            {
                case CharacterType.Guard:
                    ChangeState(StateEnums.Idle);
                    break;
            }

            return;
        }
        navMeshAgent.SetDestination(targetTransform.position);


        switch (runType)
        {
            case RunTypes.RunToBase:
                Debug.Log(Vector3.Distance(transform.position, targetTransform.position));
                if (Vector3.Distance(transform.position, targetTransform.position) <= 0.6f)
                {  
                    ChangeState(StateEnums.Idle);
                    MGameManager.Instance.Score(MGameManager.Teams.Blue);
                     
                     
                }
               
                break;
            case RunTypes.RunToBall:
                if (Vector3.Distance(transform.position, targetTransform.position) < 0.7f)
                {
                    ChangeState(StateEnums.Idle);
                }
                break;
        }
    }

    #region Pitcher

    private bool CalculateHitChance()
    {
        int randomChance = Random.Range(0, 100);
        bool result = randomChance < 60;
        return result;
    }

    public void PitcherThrowBall()
    {   
        GameObject spawnedBall = Instantiate(ball, ballSpawnPosition.position, ballSpawnPosition.rotation);
        MGameManager.Instance.spawnedBall = spawnedBall;
        bool isHit = CalculateHitChance();
        Transform ballTarget = isHit ? MGameManager.Instance.ballMissTarget : MGameManager.Instance.currentStriker.transform;
        Debug.Log(isHit + " " + ballTarget.name);
        spawnedBall.transform.DOJump(ballTarget.position, 2f, 1, 0.8f).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (!isHit)
                MGameManager.Instance.currentStriker.HitBall(spawnedBall);
            else
                MGameManager.Instance.Score(MGameManager.Teams.Red);
        });
    }
    
    public void HitBall(GameObject spawnedBall)
    {
        Transform ballTarget = MGameManager.Instance.GetRandomBallHitTarget();
        spawnedBall.transform.DOJump(ballTarget.position, 3f, 1, 2.2f).SetEase(Ease.OutQuint);
        MGameManager.Instance.MoveStrikersNextBase();
        MGameManager.Instance.MoveGuards(spawnedBall);
    }

    #endregion
}