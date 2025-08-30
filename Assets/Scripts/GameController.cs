using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    int progressAmount;
    public Slider progressSlider;
    public GameObject player;
    public GameObject LoadCanvas;
    public List<GameObject> levels;
    private int currentLevelIndex = 0;
    public GameObject level2IntroDialogue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        progressAmount = 0;
        progressSlider.value = 0;
        Apple.OnAppleCollect += IncreaseProgressAmount;
        HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
        LoadCanvas.SetActive(false);
        // if (levelIntroDialogue != null)
        // {
        //     levelIntroDialogue.gameObject.SetActive(true);
        //     levelIntroDialogue.StartDialogue();
        // }
        //Debug.Log("Start");
        }

    // Update is called once per frame
    void Update()
    {

    }

    void IncreaseProgressAmount(int amount)
    {
        progressAmount += amount;
        progressSlider.value = progressAmount;
        Debug.Log($"Added {amount}. Total now {progressAmount}. Max is {progressSlider.maxValue}");

        if (progressAmount >= 100)
        {
            // Level is complete
            LoadCanvas.SetActive(true);
            Debug.Log("Level Complete");
        }
    }

    void LoadNextLevel()
    {
        int nextLevelIndex = (currentLevelIndex == levels.Count - 1) ? 0 : currentLevelIndex + 1;
        LoadCanvas.SetActive(false);

        levels[currentLevelIndex].SetActive(false);
        levels[nextLevelIndex].SetActive(true);

        levels[nextLevelIndex].SetActive(true);

        player.transform.position = new Vector3(-9, -2, 0);

        currentLevelIndex = nextLevelIndex;
        progressAmount = 0;
        progressSlider.value = 0;

        if (nextLevelIndex == 1) // Level 2
        {
            level2IntroDialogue.SetActive(true); // Show Dialogue
            level2IntroDialogue.GetComponent<Dialogue>().StartDialogue(); // Start typing
        }
    }
}
