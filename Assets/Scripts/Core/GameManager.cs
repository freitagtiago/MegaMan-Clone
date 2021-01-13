using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Megaman Components")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject player;
    [SerializeField] Mover mover;
    [SerializeField] Shooter shooter;
    [SerializeField] Health health;
    [SerializeField] TMP_Text readyLettering;
    [SerializeField] TMP_Text uiPoints;

    [Header("Control Variables")]
    [SerializeField] float initializationTime = 2f;
    [SerializeField] Vector3 spawnPos;
    [SerializeField] bool isPlayerAlive;
    [SerializeField] int totalPoints = 0;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip levelMusic;
    [SerializeField] AudioClip bossMusic;
    [SerializeField] AudioClip victoryMusic;

    [Header("Game Over")]
    [SerializeField] int sceneToLoadWhenGameOver;


    private void Awake()
    {
        SpawnPlayer();
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        audioSource.Play();
        StartCoroutine(Initialize());
    }
    // Update is called once per frame
    void Update()
    {
        IsPlayerAlive();
        if (!isPlayerAlive)
        {
            StartCoroutine(GameOver());
        }
    }

    private void SpawnPlayer()
    {
        player = Instantiate(playerPrefab);
        
        mover = player.GetComponent<Mover>();
        mover.Spawn(spawnPos);
        shooter = player.GetComponent<Shooter>();
        health = player.GetComponent<Health>();
    }

    private void IsPlayerAlive()
    {
        isPlayerAlive = (health.GetCurrentHealth() > 0);
    }

    private IEnumerator Initialize()
    {
        uiPoints.gameObject.SetActive(false);
        readyLettering.gameObject.SetActive(true);
        yield return new WaitForSeconds(initializationTime);
        readyLettering.gameObject.SetActive(false);
        uiPoints.gameObject.SetActive(true);
        mover.SetCanMove(true);
        shooter.SetCanShoot(true);
    }

    private IEnumerator GameOver()
    {
        mover.SetCanMove(false);
        shooter.SetCanShoot(false);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneToLoadWhenGameOver);
    }

    public bool GetIsPlayerAlive()
    {
        return isPlayerAlive;
    }

    public void AddPoints(int value)
    {
        totalPoints += value;
        uiPoints.text = totalPoints.ToString();
    }

    public void PlayBossMusic()
    {
        audioSource.clip = bossMusic;
        audioSource.Play();
    }

    public void PlayVictoryMusic()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(victoryMusic);
    }
}
