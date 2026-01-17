using UnityEngine;

public class BreathingUITester : MonoBehaviour
{
    public HoldToBreathe holdToBreathe;
    
    void Update()
    {
        // Press Space to start breathing cycle
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Test: Starting breathing UI cycle");
            holdToBreathe.StartBreathingCycle();
        }
    }
}
