using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Transform target;

    private void Start()
    {
        target = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.rotation = target.rotation;
    }
}
