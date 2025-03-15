using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public enum enemyState
{
    IDLE, ALERT, PATROL, FURY, FOLLOW, DIE
}

public enum GameState
{
    GAMEPLAY, DIE
}

public class GameManager : MonoBehaviour
{

    public Transform player;
    public GameState gameState;


    [Header("Slime IA")]
    public float slimeIdleWaitTime;
    public Transform[] slimeWayPoints;
    public float slimeDistanceToAttack = 2.3f;
    public float slimeAlertTime = 3f;
    public float slimeAttackDelay = 1f;
    public float slimeLookAtSpeed = 1f;

    [Header("Main manager")]
    public PostProcessVolume postB;
    public ParticleSystem rainParticle;
    private ParticleSystem.EmissionModule rainModule;
    public int rainRateOverTime;
    public int rainIncremant;
    public float rainIncrementDelay;


    private void Start()
    {
        rainModule = rainParticle.emission;
    }

    public void OnOffRain(bool isRain)
    {
        StopCoroutine("RainManager");
        StopCoroutine("PostManager");
        StartCoroutine("RainManager", isRain);
        StartCoroutine("PostManager", isRain);

    }

    IEnumerator RainManager (bool isRain)
    {
        switch (isRain)
        {
            case true:
                for(float r = rainModule.rateOverTime.constant; r < rainRateOverTime; r+=rainIncremant)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }

                rainModule.rateOverTime = rainRateOverTime;
                break;

            case false:
                for (float r = rainModule.rateOverTime.constant; r > 0; r -= rainIncremant)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }
                rainModule.rateOverTime = 0;
                break;
        }
    }

    IEnumerator PostManager(bool isRain)
    {
        switch(isRain)
        {
            case true:
                
                for(float w = postB.weight; w < 1; w+=1 * Time.deltaTime)
                {
                    postB.weight = w;
                    yield return new WaitForEndOfFrame();
                }
                postB.weight = 1;
                break;
            case false:
                for (float w = postB.weight; w > 0; w -= 1 * Time.deltaTime)
                {
                    postB.weight = w;
                    yield return new WaitForEndOfFrame();
                }
                postB.weight = 0;
                break;
        }
    }

    public void ChangeGameState(GameState newState)
    {
        gameState = newState;
    }
}
