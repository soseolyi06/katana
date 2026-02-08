using UnityEngine;

public class CameraShakeTrigger : MonoBehaviour
{
    public float duration = 0.12f;
    public float strength = 0.15f;

    void OnEnable()
    {
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(duration, strength);
    }
}
