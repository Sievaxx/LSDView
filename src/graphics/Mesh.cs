﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSDView.math;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace LSDView.graphics
{
    public class Mesh : IDisposable, IRenderable
    {
        private readonly VertexArray _verts;

        public Shader Shader { get; set; }
        public Transform Transform { get; set; }
        public List<Texture2D> Textures { get; set; }

        public Mesh(Vertex[] vertices, int[] indices, Shader shader)
        {
            _verts = new VertexArray(vertices, indices);
            Shader = shader;
            Transform = new Transform();
            Textures = new List<Texture2D>();
        }

        public void Render(Matrix4 modelView, Matrix4 projection)
        {
            _verts.Bind();
            BindTextures();
            Shader.Bind();
            Shader.Uniform("Projection", false, projection);
            Shader.Uniform("ModelView", false, modelView);
            GL.DrawElements(PrimitiveType.Triangles, _verts.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Shader.Unbind();
            UnbindTextures();
            _verts.Unbind();
        }

        public void Dispose()
        {
            _verts.Dispose();
            foreach (Texture2D tex in Textures)
            {
                tex.Dispose();
            }
        }

        private void BindTextures()
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                Textures[i].Bind();
            }
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        private void UnbindTextures()
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                Textures[i].Unbind();
            }
            GL.ActiveTexture(TextureUnit.Texture0);
        }
    }
}
