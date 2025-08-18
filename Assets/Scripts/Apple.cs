using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apple : MonoBehaviour, IItem
{
    public void Collect()
    {
        Destroy(gameObject);
    }
}
