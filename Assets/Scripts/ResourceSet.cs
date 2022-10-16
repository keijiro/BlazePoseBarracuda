using UnityEngine;
using Unity.Barracuda;

namespace MediaPipe.BlazePose {

// ScriptableObject class used to hold references to internal assets
[CreateAssetMenu(fileName = "BlazePose",
                 menuName = "ScriptableObjects/MediaPipe/BlazePose Resource Set")]
public sealed class ResourceSet : ScriptableObject
{
    public NNModel detectionModel;
    public NNModel landmarkModel;
    public ComputeShader preprocess;
    public ComputeShader postprocess1;
    public ComputeShader postprocess2;
}

} // namespace MediaPipe.BlazePose
