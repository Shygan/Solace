using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If collision doesnt contain IItem interface, it will be null!
        IItem item = collision.GetComponent<IItem>();
        if (item != null)
        {
            item.Collect();
        }
    }
}
