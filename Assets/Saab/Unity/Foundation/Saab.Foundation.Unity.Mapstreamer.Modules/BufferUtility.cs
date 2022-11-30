using System;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public static class BufferUtility
    {
        static ComputeBuffer _indirectBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);

        public static int BufferSize(ComputeBuffer buffer)
        {
            ComputeBuffer.CopyCount(buffer, _indirectBuffer, 0);

            int[] array = new int[4];
            _indirectBuffer.GetData(array);

            return array[0];
        }

        public static Vector4[] CopyActualBuffer(ComputeBuffer buffer)
        {
            var size = BufferSize(buffer);

            var result = new Vector4[size];

            buffer.GetData(result);
            return result;
        }

        public static Vector4[] CopyFullBuffer(ComputeBuffer buffer)
        {
            var size = buffer.count;

            var result = new Vector4[size];

            buffer.GetData(result);
            return result;
        }

        public static void Dispose()
        {
            _indirectBuffer.Dispose();
        }
    }
}
