using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Apple : MonoBehaviour, IItem
{
    public static event Action<int> OnAppleCollect;
    public int worth = 5;

    private bool collected = false;
    private Collider2D _col;   // cache to disable instantly

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
    }

    public void Collect()
    {
        if (collected) return;          // guard: ignore second call
        collected = true;

        if (_col) _col.enabled = false;  // stop further trigger events this frame
        OnAppleCollect?.Invoke(worth);   // safe invoke (null-checked)
        Destroy(gameObject);
    }
}