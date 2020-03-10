using UnityEngine;

namespace Saab.Unity.Core
{
    namespace ComputeExtension
    {
        public static class ComputeExtension
        {
            private static int[] args = new int[4];
            private static float[] floatMatrix = new float[16];

            public static int GetCounterValue(this ComputeBuffer buffer, ComputeBuffer argBuffer)
            {
                args[0] = 0;
                args[1] = 1;
                args[2] = 0;
                args[3] = 0;
                argBuffer.SetData(args);
                ComputeBuffer.CopyCount(buffer, argBuffer, 0);
                argBuffer.GetData(args);
                return args[0];
            }

            public static void SetMatrix(this ComputeShader computeShader, string name, Matrix4x4 matrix)
            {
                floatMatrix[0] = matrix.m00;
                floatMatrix[1] = matrix.m10;
                floatMatrix[2] = matrix.m20;
                floatMatrix[3] = matrix.m30;
                floatMatrix[4] = matrix.m01;
                floatMatrix[5] = matrix.m11;
                floatMatrix[6] = matrix.m21;
                floatMatrix[7] = matrix.m31;
                floatMatrix[8] = matrix.m02;
                floatMatrix[9] = matrix.m12;
                floatMatrix[10] = matrix.m22;
                floatMatrix[11] = matrix.m32;
                floatMatrix[12] = matrix.m03;
                floatMatrix[13] = matrix.m13;
                floatMatrix[14] = matrix.m23;
                floatMatrix[15] = matrix.m33;
                computeShader.SetFloats(name, floatMatrix);
            }

            public static void SetMatrix(this ComputeShader computeShader, int id, Matrix4x4 matrix)
            {
                floatMatrix[0] = matrix.m00;
                floatMatrix[1] = matrix.m10;
                floatMatrix[2] = matrix.m20;
                floatMatrix[3] = matrix.m30;
                floatMatrix[4] = matrix.m01;
                floatMatrix[5] = matrix.m11;
                floatMatrix[6] = matrix.m21;
                floatMatrix[7] = matrix.m31;
                floatMatrix[8] = matrix.m02;
                floatMatrix[9] = matrix.m12;
                floatMatrix[10] = matrix.m22;
                floatMatrix[11] = matrix.m32;
                floatMatrix[12] = matrix.m03;
                floatMatrix[13] = matrix.m13;
                floatMatrix[14] = matrix.m23;
                floatMatrix[15] = matrix.m33;
                computeShader.SetFloats(id, floatMatrix);
            }

            public static T[] GetData<T>(this ComputeBuffer computeBuffer, int count)
            {
                T[] output = new T[count];
                computeBuffer.GetData(output);
                return output;
            }

            public static void SafeRelease(this ComputeBuffer computeBuffer)
            {
                if (computeBuffer != null)
                {
                    computeBuffer.Release();
                }
            }
        }

        public class ComputeKernel
        {
            private string kernelName;
            public string Name
            {
                get
                {
                    return kernelName;
                }
            }

            private int kernelIndex;
            public int Index
            {
                get
                {
                    return kernelIndex;
                }
            }

            private ComputeShader kernelShader;
            public ComputeShader Shader
            {
                get
                {
                    return kernelShader;
                }
            }

            public ComputeKernel(string kernelName, ComputeShader computeShader)
            {
                this.kernelShader = computeShader;
                this.kernelName = kernelName;
                this.kernelIndex = computeShader.FindKernel(Name);
            }

            public void SetBuffer(string name, ComputeBuffer buffer)
            {
                Shader.SetBuffer(kernelIndex, name, buffer);
            }

            public void SetBuffer(int id, ComputeBuffer buffer)
            {
                Shader.SetBuffer(kernelIndex, id, buffer);
            }

            public void SetTexture(string name, Texture texture)
            {
                Shader.SetTexture(kernelIndex, name, texture);
            }

            public void SetTexture(int id, Texture texture)
            {
                Shader.SetTexture(kernelIndex, id, texture);
            }

            public void SetTextureFromGlobal(int id, int idGlobal)
            {
                Shader.SetTextureFromGlobal(kernelIndex, id, idGlobal);
            }

            public void SetTextureFromGlobal(string name, string nameGlobal)
            {
                Shader.SetTextureFromGlobal(kernelIndex, name, nameGlobal);
            }

            public void Dispatch(int threadGroupsX, int threadGroupsY, int threadGroupsZ)
            {
                Shader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, threadGroupsZ);
            }

            public void Dispatch1D(int invocations)
            {
                uint x, y, z;

                GetThreadGroupSizes(out x, out y, out z);
                x = (uint)Mathf.CeilToInt(invocations / (float)x);

#if UNITY_EDITOR
                if (y != 1 || z != 1)
                {
                    Debug.LogWarning("Dispatch1D() should be used only with 1D thread groupes.");
                }
#endif

                Shader.Dispatch(kernelIndex, (int)x, 1, 1);
            }

            public void DispatchIndirect(ComputeBuffer argsBuffer, uint argsOffset = 0)
            {
                Shader.DispatchIndirect(kernelIndex, argsBuffer, argsOffset);
            }

            public void GetThreadGroupSizes(out uint x, out uint y, out uint z)
            {
                Shader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            }
        }
    }
}
