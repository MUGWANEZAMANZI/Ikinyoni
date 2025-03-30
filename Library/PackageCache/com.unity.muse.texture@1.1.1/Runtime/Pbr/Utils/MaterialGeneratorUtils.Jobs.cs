using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Unity.Muse.Texture
{
    internal static partial class MaterialGeneratorUtils
    {
        struct ConvertTextToDxtnmJob : IJobParallelFor
        {
            public NativeArray<Color32> IncomingPixels;
            public NativeArray<Color32> OutputPixels;

            public void Execute(int index)
            {
                //Incoming: ARGB32
                //Output: RGBA32
                //Swizzle ensues.
                var inputPixel = IncomingPixels[index];

                var outputPixel = inputPixel;
                outputPixel.r = 255;
                outputPixel.g = inputPixel.b;
                outputPixel.b = 255;
                outputPixel.a = inputPixel.g;

                OutputPixels[index] = outputPixel;
            }
        }
#if USING_HDRP
        struct CombineMaterialMaskMap : IJobParallelFor
        {
            public NativeArray<Color32> MetallicColors;
            [ReadOnly]
            public NativeArray<Color32> RoughnessColors;

            public void Execute(int index)
            {
                //Both native arrays are actually in ARGB32 so swizzle appropriately
                var col = new Color32((byte)(255 - RoughnessColors[index].a), MetallicColors[index].g, 255, 0);
                MetallicColors[index] = col;
            }
        }
#endif

        struct CombineMaterialRoughnessMap : IJobParallelFor
        {
            public NativeArray<Color32> MetallicColors;
            [ReadOnly]
            public NativeArray<Color32> RoughnessColors;

            public void Execute(int index)
            {
                //Both native arrays are actually in ARGB32 so swizzle appropriately
                var metallicPixel = MetallicColors[index];
                metallicPixel.r = (byte)(byte.MaxValue - RoughnessColors[index].a);

                MetallicColors[index] = metallicPixel;
            }
        }
    }
}
