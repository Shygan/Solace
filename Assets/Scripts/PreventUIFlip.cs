using UnityEngine;

public class PreventUIFlip : MonoBehaviour
{
    public Transform player;
    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale; // e.g. (0.01, 0.01, 1)
    }

    void LateUpdate()
    {
        if (player == null) return;

        float dir = Mathf.Sign(player.localScale.x);
        if (dir == 0) dir = 1f;

        // Make the child scale cancel out parent's flip so world scale stays positive
        Vector3 s = baseScale;
        s.x *= dir;          // same sign as player
        transform.localScale = s;
    }
}