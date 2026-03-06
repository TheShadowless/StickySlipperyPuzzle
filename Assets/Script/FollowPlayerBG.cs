using UnityEngine;

public class FollowPlayerBG : MonoBehaviour
{
    public Transform player;
    public float offsetY = 0f;

    void LateUpdate()
    {
        if (player == null) return;

        transform.position = new Vector3(
            player.position.x,
            player.position.y + offsetY,
            transform.position.z
        );
    }
}