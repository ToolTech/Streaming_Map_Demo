/* 
 * Copyright (C) SAAB AB
 *
 * All rights, including the copyright, to the computer program(s) 
 * herein belong to Saab AB. The program(s) may be used and/or
 * copied only with the written permission of Saab AB, or in
 * accordance with the terms and conditions stipulated in the
 * agreement/contract under which the program(s) have been
 * supplied. 
 * 
 * Information Class:          COMPANY RESTRICTED
 * Defence Secrecy:            UNCLASSIFIED
 * Export Control:             NOT EXPORT CONTROLLED
 */

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