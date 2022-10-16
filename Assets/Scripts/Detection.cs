using System.Runtime.InteropServices;
using UnityEngine;

namespace MediaPipe.BlazePose {

partial class PoseDetector
{
    // Detection structure. The layout of this structure must be matched with
    // the one defined in Common.hlsl.
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Detection
    {
        // Bounding box
        public readonly Vector2 center;
        public readonly Vector2 extent;

        // Key points
        public readonly Vector2 key1;
        public readonly Vector2 key2;
        public readonly Vector2 key3;
        public readonly Vector2 key4;

        // Confidence score [0, 1]
        public readonly float score;

        // Padding
        public readonly float pad1, pad2, pad3;

        // sizeof(Detection)
        public const int Size = 16 * sizeof(float);
    };
}

} // namespace MediaPipe.BlazePose
