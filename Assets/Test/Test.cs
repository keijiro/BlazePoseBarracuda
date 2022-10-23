using UnityEngine;
using MediaPipe.BlazePose;

sealed class Test : MonoBehaviour
{
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Texture2D _sourceTexture = null;

    void Start()
    {
        using var detector = new PoseDetector(_resources);
        detector.ProcessImage(_sourceTexture);

        foreach (var found in detector.Detections)
        {
            Debug.Log($"{found.center}, {found.extent}, {found.score}");
        }
    }
}
