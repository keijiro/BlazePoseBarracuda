using UnityEngine;
using UnityEngine.UI;
using MediaPipe.BlazePose;

sealed class Test : MonoBehaviour
{
    [SerializeField] Texture2D _sourceTexture = null;
    [SerializeField] RawImage _uiImage = null;
    [SerializeField] GameObject _marker = null;
    [SerializeField] ResourceSet _resources = null;

    void AddMarker(Vector2 pos)
    {
        var marker = Instantiate(_marker, _uiImage.transform);
        var parent = _uiImage.GetComponent<RectTransform>();
        var xform = marker.GetComponent<RectTransform>();
        xform.anchoredPosition = pos * parent.rect.size;
    }

    void Start()
    {
        using var detector = new PoseDetector(_resources);
        detector.ProcessImage(_sourceTexture);

        foreach (var found in detector.Detections)
        {
            AddMarker(found.key1);
            AddMarker(found.key2);
            AddMarker(found.key3);
            AddMarker(found.key4);
        }

        _uiImage.texture = _sourceTexture;
    }
}
