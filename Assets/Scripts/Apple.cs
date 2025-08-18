using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Apple : MonoBehaviour, IItem
{
    public static event Action<int> OnAppleCollect;
    public int worth = 5;
    public void Collect()
    {
        OnAppleCollect.Invoke(worth);
        Destroy(gameObject);
    }
}
