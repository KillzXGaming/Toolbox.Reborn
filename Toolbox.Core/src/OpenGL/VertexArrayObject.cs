using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Toolbox.Core.OpenGL
{
    public struct VertexArrayObject
    {
        private int vaoID;

        private readonly int buffer;
        private readonly int? indexBuffer;
        private readonly Dictionary<int, VertexAttribute> attributes;

        public VertexArrayObject(int buffer, int? indexBuffer = null)
        {
            vaoID = -1;
            this.buffer = buffer;
            this.indexBuffer = indexBuffer;
            attributes = new Dictionary<int, VertexAttribute>();
        }

        public void AddAttribute(int index, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
        {
            attributes[index] = new VertexAttribute(size, type, normalized, stride, offset);
        }

        public void Initialize()
        {
            if (vaoID != -1)
                return;

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);

            foreach (KeyValuePair<int, VertexAttribute> a in attributes)
            {
                GL.EnableVertexAttribArray(a.Key);
                if (a.Value.type == VertexAttribPointerType.Int)
                    GL.VertexAttribIPointer(a.Key, a.Value.size, VertexAttribIntegerType.Int, a.Value.stride, new System.IntPtr(a.Value.offset));
                else
                    GL.VertexAttribPointer(a.Key, a.Value.size, a.Value.type, a.Value.normalized, a.Value.stride, a.Value.offset);
            }
            vaoID = vao;
        }

        public void Enable()
        {
            GL.BindVertexArray(vaoID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);

            foreach (KeyValuePair<int, VertexAttribute> a in attributes)
            {
                GL.EnableVertexAttribArray(a.Key);
                if (a.Value.type == VertexAttribPointerType.Int)
                    GL.VertexAttribIPointer(a.Key, a.Value.size, VertexAttribIntegerType.Int, a.Value.stride, new System.IntPtr(a.Value.offset));
                else
                    GL.VertexAttribPointer(a.Key, a.Value.size, a.Value.type, a.Value.normalized, a.Value.stride, a.Value.offset);
            }
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        }

        public void Use()
        {
            GL.BindVertexArray(vaoID);

            if (indexBuffer.HasValue)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer.Value);
        }

        private struct VertexAttribute
        {
            public int size;
            public VertexAttribPointerType type;
            public bool normalized;
            public int stride;
            public int offset;
            public VertexAttribute(int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
            {
                this.size = size;
                this.type = type;
                this.normalized = normalized;
                this.stride = stride;
                this.offset = offset;
            }
        }
    }
}