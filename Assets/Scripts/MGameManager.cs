using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MGameManager : MonoBehaviour
{
    public static MGameManager Instance;
    public GameStates currentGameState;
    public GameObject strikerPrefab;
    public Character currentStriker;
    public List<Character> strikers;
    public List<Character> guards;
    public Character pitcher;
    public Transform strikerPoint;
    public Transform ballMissTarget;
    public List<Transform> ballHitTargets;
    public GameObject spawnedBall;
    public List<Transform> bases;
    public List<int> scores;
    public List<TextMeshProUGUI> scoreTexts;
    public List<TextMeshProUGUI> winTexts;
    public TextMeshProUGUI spaceTexts;
    public Image waitScreen;

    public enum GameStates
    {
        Play,
        Pause,
        Finished,
    }

    public enum Teams
    {
        Blue,
        Red,
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        SpawnStriker();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentGameState == GameStates.Finished)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public void Score(Teams team)
    {
        if (currentGameState != GameStates.Play) return;

        currentGameState = GameStates.Pause;

        scores[(int)team]++;
        scoreTexts[(int)team].text = scores[(int)team].ToString();
        if (scores[(int)team] >= 3)
        {
            waitScreen.DOFade(1, 0.5f).OnComplete(() =>
            {
                winTexts[(int)team].DOFade(1, 0.2f);
                spaceTexts.DOFade(1, 0.2f);
                currentGameState = GameStates.Finished;
            });
            return;
        }

        int step = 0;
        waitScreen.DOFade(1, 0.5f).SetLoops(2, LoopType.Yoyo).OnStepComplete(() =>
        {
            if (step == 0)
            {
                step++;
                
                Destroy(spawnedBall);

                if (team == Teams.Blue)
                    SpawnStriker();

                ResetGuards();
            }
        }).OnComplete(() =>
        {
            currentGameState = GameStates.Play; 
            pitcher.PitcherThrowBall();
        });
    }

    
    public void SpawnStriker()
    {
        currentStriker = Instantiate(strikerPrefab, strikerPoint.position, strikerPoint.rotation)
            .GetComponent<Character>();
    }


    public Transform GetRandomBallHitTarget()
    {
        int randomIndex = Random.Range(0, ballHitTargets.Count);
        return ballHitTargets[randomIndex];
    }

    public void MoveStrikersNextBase()
    {
        for (int i = 0; i < strikers.Count; i++)
        {
            strikers[i].currentBaseCount++;
            strikers[i].target = strikers[i].currentBaseCount <= bases.Count
                ? bases[strikers[i].currentBaseCount]
                : strikerPoint;
            strikers[i].ChangeState(Character.StateEnums.Run);
        }
    }

    public Transform GetTargetBaseOfFirstStriker()
    {
        return bases[strikers[strikers.Count - 1].currentBaseCount];
    }

    public void MoveGuards(GameObject ball)
    {
        for (int i = 0; i < guards.Count; i++)
        {
            guards[i].target = ball.transform;
            guards[i].navMeshAgent.enabled = true;
            guards[i].ChangeState(Character.StateEnums.Run);
        }
    }

    public void ResetGuards()
    {
        for (int i = 0; i < guards.Count; i++)
        {
            guards[i].transform.position = guards[i].guardStartPosition;
            guards[i].target = null;
            guards[i].navMeshAgent.enabled = false;
            guards[i].ChangeState(Character.StateEnums.Idle);
        }
    }
}