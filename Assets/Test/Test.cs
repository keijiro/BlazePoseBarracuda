using UnityEngine;
using UnityEngine.UI;
using MediaPipe.BlazePose;
using System.Collections.Generic;

sealed class Test : MonoBehaviour
{
    [SerializeField] Texture _sourceTexture = null;
    [SerializeField] RawImage _uiImage = null;
    [SerializeField] GameObject _markerPrefab = null;
    [SerializeField] ResourceSet _resources = null;

    (Queue<GameObject> free, Queue<GameObject> used) _markers
      = (new Queue<GameObject>(), new Queue<GameObject>());

    PoseDetector _detector;

    void AddMarker(Vector2 pos)
    {
        var marker = _markers.free.Count > 0 ?
          _markers.free.Dequeue() :
          Instantiate(_markerPrefab, _uiImage.transform);

        _markers.used.Enqueue(marker);

        var parent = _uiImage.GetComponent<RectTransform>();
        var xform = marker.GetComponent<RectTransform>();
        xform.anchoredPosition = pos * parent.rect.size;
    }

    void Start()
    {
        _detector = new PoseDetector(_resources);
        _uiImage.texture = _sourceTexture;
    }

    void OnDestroy()
    {
        _detector?.Dispose();
        _detector = null;
    }

    void Update()
    {
        _detector.ProcessImage(_sourceTexture);

        while (_markers.used.Count > 0)
            _markers.free.Enqueue(_markers.used.Dequeue());

        foreach (var found in _detector.Detections)
        {
            AddMarker(found.key1);
            AddMarker(found.key2);
            AddMarker(found.key3);
            AddMarker(found.key4);
        }
    }
}
