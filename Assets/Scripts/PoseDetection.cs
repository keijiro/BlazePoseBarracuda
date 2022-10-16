using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace MediaPipe.BlazePose {

public sealed partial class PoseDetector : System.IDisposable
{
    #region Public accessors

    public ComputeBuffer DetectionBuffer
      => _post2Buffer;

    public IEnumerable<Detection> Detections
      => _post2ReadCache ?? UpdatePost2ReadCache();

    #endregion

    #region Public methods

    public PoseDetector(ResourceSet resources)
    {
        _resources = resources;
        AllocateObjects();
    }

    public void Dispose()
      => DeallocateObjects();

    public void ProcessImage(Texture image)
      => RunModel(image);

    #endregion

    #region Compile-time constants

    // Maximum number of detections. This value must be matched with
    // MAX_DETECTION in Common.hlsl.
    const int MaxDetection = 64;

    const int DetectionImageSize = 224;

    #endregion

    #region Private objects

    ResourceSet _resources;
    ComputeBuffer _preBuffer;
    ComputeBuffer _post1Buffer;
    ComputeBuffer _post2Buffer;
    ComputeBuffer _countBuffer;
    IWorker _worker;

    void AllocateObjects()
    {
        var model = ModelLoader.Load(_resources.detectionModel);

        _preBuffer = new ComputeBuffer
          (DetectionImageSize * DetectionImageSize * 3, sizeof(float));

        _post1Buffer = new ComputeBuffer
          (MaxDetection, Detection.Size, ComputeBufferType.Append);

        _post2Buffer = new ComputeBuffer
          (MaxDetection, Detection.Size, ComputeBufferType.Append);

        _countBuffer = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);

        _worker = model.CreateWorker();
    }

    void DeallocateObjects()
    {
        _preBuffer?.Dispose();
        _preBuffer = null;

        _post1Buffer?.Dispose();
        _post1Buffer = null;

        _post2Buffer?.Dispose();
        _post2Buffer = null;

        _countBuffer?.Dispose();
        _countBuffer = null;

        _worker?.Dispose();
        _worker = null;
    }

    #endregion

    #region Neural network inference function

    void RunModel(Texture source)
    {
        // Reset the compute buffer counters.
        _post1Buffer.SetCounterValue(0);
        _post2Buffer.SetCounterValue(0);

        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetInt("_Size", DetectionImageSize);
        pre.SetVector("_Range", new Vector2(-1, 1));
        pre.SetTexture(0, "_Texture", source);
        pre.SetBuffer(0, "_Tensor", _preBuffer);
        pre.Dispatch(0, DetectionImageSize / 8, DetectionImageSize / 8, 1);

        // Run the BlazePose model.
        using (var tensor = new Tensor(1, DetectionImageSize, DetectionImageSize, 3, _preBuffer))
            _worker.Execute(tensor);

        // Output tensors -> Temporary render textures
        var scoresRT = _worker.CopyOutputToTempRT("Identity_1",  1, 2254);
        var  boxesRT = _worker.CopyOutputToTempRT("Identity"  , 12, 2254);

        // 1st postprocess (bounding box aggregation)
        var post1 = _resources.postprocess1;

        post1.SetInt("_RowOffset", 0);
        post1.SetTexture(0, "_Scores", scoresRT);
        post1.SetTexture(0, "_Boxes", boxesRT);
        post1.SetBuffer(0, "_Output", _post1Buffer);
        post1.Dispatch(0, 1, 1, 1);

        post1.SetInt("_RowOffset", 1568);
        post1.SetTexture(1, "_Scores", scoresRT);
        post1.SetTexture(1, "_Boxes", boxesRT);
        post1.SetBuffer(1, "_Output", _post1Buffer);
        post1.Dispatch(1, 1, 1, 1);

        post1.SetInt("_RowOffset", 1960);
        post1.SetTexture(2, "_Scores", scoresRT);
        post1.SetTexture(2, "_Boxes", boxesRT);
        post1.SetBuffer(2, "_Output", _post1Buffer);
        post1.Dispatch(2, 1, 1, 1);

        post1.SetInt("_RowOffset", 2058);
        post1.SetTexture(2, "_Scores", scoresRT);
        post1.SetTexture(2, "_Boxes", boxesRT);
        post1.SetBuffer(2, "_Output", _post1Buffer);
        post1.Dispatch(2, 1, 1, 1);

        post1.SetInt("_RowOffset", 2156);
        post1.SetTexture(2, "_Scores", scoresRT);
        post1.SetTexture(2, "_Boxes", boxesRT);
        post1.SetBuffer(2, "_Output", _post1Buffer);
        post1.Dispatch(2, 1, 1, 1);

        // Release the temporary render textures.
        RenderTexture.ReleaseTemporary(scoresRT);
        RenderTexture.ReleaseTemporary(boxesRT);

        // Retrieve the bounding box count.
        ComputeBuffer.CopyCount(_post1Buffer, _countBuffer, 0);

        // 2nd postprocess (overlap removal)
        var post2 = _resources.postprocess2;
        post2.SetBuffer(0, "_Input", _post1Buffer);
        post2.SetBuffer(0, "_Count", _countBuffer);
        post2.SetBuffer(0, "_Output", _post2Buffer);
        post2.Dispatch(0, 1, 1, 1);

        // Retrieve the bounding box count after removal.
        ComputeBuffer.CopyCount(_post2Buffer, _countBuffer, 0);

        // Read cache invalidation
        _post2ReadCache = null;
    }

    #endregion

    #region GPU to CPU readback

    Detection[] _post2ReadCache;
    int[] _countReadCache = new int[1];

    Detection[] UpdatePost2ReadCache()
    {
        _countBuffer.GetData(_countReadCache, 0, 0, 1);
        var count = _countReadCache[0];

        _post2ReadCache = new Detection[count];
        _post2Buffer.GetData(_post2ReadCache, 0, 0, count);

        return _post2ReadCache;
    }

    #endregion
}

} // namespace MediaPipe.BlazePose
