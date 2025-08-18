using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    int progressAmount;
    public Slider progressSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        progressAmount = 0;
        progressSlider.value = 0;
        Apple.OnAppleCollect += IncreaseProgressAmount;
        Debug.Log("Start");
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
            Debug.Log("Level Complete");
        }
    }
}
