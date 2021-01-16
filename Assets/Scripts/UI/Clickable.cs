using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Clickable : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] bool canBlink = false;
    [SerializeField] float timeBlink;
    [SerializeField] TMP_Text textToBlink;
    [SerializeField] bool isButton = true;
    [SerializeField] int sceneToLoad;

    private void Update()
    {
        if (canBlink)
        {
            StartCoroutine(BlinkRoutine());
        }

        
        if (!isButton && Input.GetKeyDown(KeyCode.Space))
        {
            LoadScene(sceneToLoad);
        }
    }


    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    private IEnumerator BlinkRoutine()
    {
        textToBlink.enabled = false;
        canBlink = false;
        yield return new WaitForSeconds(timeBlink);
        textToBlink.enabled = true;
        yield return new WaitForSeconds(timeBlink);
        canBlink = true;
    }
}
