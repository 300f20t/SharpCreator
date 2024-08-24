using SharpDX;
using SharpDX.Mathematics.Interop;

namespace SharpCreator
{
    public struct CustomVertex
    {
        public Vector3 Position;
        public RawColorBGRA Color;

        public CustomVertex(Vector3 position, RawColorBGRA color)
        {
            Position = position;
            Color = color;
        }

        public static int Stride => sizeof(float) * 3 + sizeof(int);
    }
}
