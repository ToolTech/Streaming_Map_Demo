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

using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public static class DebugUtils
    {
        public static RenderTexture LastCopiedBuffer { get; set; }

        public static ComputeShader CopyShader { get; set; }

        public static bool BufferToRenderTexture(RenderTexture rt, ComputeShader cs, ComputeBuffer buffer, Vector2Int dimensions, Vector2 fov)
        {
            int threads = 8;

            if (dimensions.x == 0 || dimensions.y == 0)
                return false; //TODO: Log message

            if (rt.width != dimensions.x || rt.height != dimensions.y)
                return false; //TODO: Log message

            if (buffer.stride != sizeof(int))
                return false; //TODO: Log message

            var kernel = cs.FindKernel("CSTransferToTexture");

            cs.SetVector("FOV", fov);
            cs.SetVector("Dimensions", (Vector2)dimensions);
            cs.SetTexture(kernel, "Output", rt);
            cs.SetBuffer(kernel, "Input", buffer);

            var groupx = Mathf.CeilToInt((float)dimensions.x / threads);
            var groupy = Mathf.CeilToInt((float)dimensions.y / threads);
            cs.Dispatch(kernel, groupx, groupy, 1);

            LastCopiedBuffer = rt;

            return true;
        }

        public static bool BufferToRenderTexture(ComputeBuffer buffer, Vector2Int dimensions, Vector2 fov)
        {
            if (CopyShader == null)
                return false;

            var rt = LastCopiedBuffer;

            if (LastCopiedBuffer != null)
            {
                rt = RenderTexture.GetTemporary(dimensions.x, dimensions.y); //TODO: RELEASE
                rt.enableRandomWrite = true;
            }
            rt = LastCopiedBuffer;

            return BufferToRenderTexture(rt, CopyShader, buffer, dimensions, fov);
        }
    }
}