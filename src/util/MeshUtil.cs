﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Formats;
using libLSD.Types;
using LSDView.graphics;
using OpenTK;

namespace LSDView.util
{
    public static class MeshUtil
    {
        public static string TMDToOBJFile(TMD tmd)
        {
            int vertCount = (int)tmd.NumberOfVertices;
            int faceCount = 0;
            foreach (var tmdObj in tmd.ObjectTable)
            {
                faceCount += (int)tmdObj.NumPrimitives;
            }

            ObjBuilder obj = new ObjBuilder();
            WriteOBJHeader(obj, vertCount, faceCount);
            
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();

            foreach (var tmdObj in tmd.ObjectTable)
            {
                foreach (Vec3 vert in tmdObj.Vertices)
                {
                    positions.Add(new Vector3(vert.X, vert.Y, vert.Z));
                }

                foreach (var norm in tmdObj.Normals)
                {
                    normals.Add(new Vector3(norm.X, norm.Y, norm.Z));
                }
            }

            foreach (var pos in positions)
            {
                obj.Vertex(pos);
            }

            foreach (var norm in normals)
            {
                obj.Normal(norm);
            }

            foreach (var tmdObj in tmd.ObjectTable)
            {
                foreach (var prim in tmdObj.Primitives)
                {
                    if (prim.PacketData is ITexturedPrimitivePacket uvPacket)
                    {
                        for (int j = 0; j < prim.PacketData.Vertices.Length; j++)
                        {
                            int uvIndex = j * 2;
                            obj.UV(new Vector2(uvPacket.UVs[uvIndex] / 255f, uvPacket.UVs[uvIndex + 1] / 255f));
                        }
                    }
                }
            }

            ObjFaceBuilder faceBuilder = new ObjFaceBuilder();
            int indexBase = 0;
            for (int i = 0; i < tmd.Header.NumObjects; i++)
            {
                obj.Group($"Object {i}");

                int uvCount = 1;
                foreach (var prim in tmd.ObjectTable[i].Primitives)
                {
                    IPrimitivePacket primitivePacket = prim.PacketData;
                    ITexturedPrimitivePacket texPrimitivePacket = prim.PacketData as ITexturedPrimitivePacket;
                    ILitPrimitivePacket litPrimitivePacket = prim.PacketData as ILitPrimitivePacket;

                    int[] indices = new int[primitivePacket.Vertices.Length];
                    for (int vert = 0; vert < primitivePacket.Vertices.Length; vert++)
                    {
                        int v = indexBase + primitivePacket.Vertices[vert] + 1;
                        indices[vert] = v;
                        int? t = null;
                        int? n = null;

                        if (texPrimitivePacket != null)
                        {
                            t = uvCount++;
                        }

                        if (litPrimitivePacket != null)
                        {
                            n = litPrimitivePacket.Normals[vert] + 1;
                        }
                    }

                    bool isQuad = (prim.Options & TMDPrimitivePacket.OptionsFlags.Quad) != 0;
                    bool isDoubleSided = (prim.Flags & TMDPrimitivePacket.PrimitiveFlags.DoubleSided) != 0;

                    faceBuilder.Vertex(indices[1]);
                    faceBuilder.Vertex(indices[0]);
                    faceBuilder.Vertex(indices[2]);
                    obj.Face(faceBuilder.Build());
                    faceBuilder.Clear();

                    if (isQuad)
                    {
                        faceBuilder.Vertex(indices[1]);
                        faceBuilder.Vertex(indices[2]);
                        faceBuilder.Vertex(indices[3]);
                        obj.Face(faceBuilder.Build());
                        faceBuilder.Clear();
                    }

                    if (isDoubleSided)
                    {
                        obj.Comment("DOuble sided!");
                        faceBuilder.Vertex(indices[0]);
                        faceBuilder.Vertex(indices[1]);
                        faceBuilder.Vertex(indices[2]);
                        obj.Face(faceBuilder.Build());
                        faceBuilder.Clear();
                        
                        if (isQuad)
                        {
                            faceBuilder.Vertex(indices[2]);
                            faceBuilder.Vertex(indices[1]);
                            faceBuilder.Vertex(indices[3]);
                            obj.Face(faceBuilder.Build());
                            faceBuilder.Clear();
                        }
                    }
                    

                    /*
                    if (indices.Length == 3)
                    {
                        faceBuilder.Vertex(indices[0]);
                        faceBuilder.Vertex(indices[2]);
                        faceBuilder.Vertex(indices[1]);
                        obj.Face(faceBuilder.Build());
                        if ((prim.Flags & TMDPrimitivePacket.PrimitiveFlags.DoubleSided) != 0)
                        {
                            faceBuilder.Clear();
                            faceBuilder.Vertex(indices[0]);
                            faceBuilder.Vertex(indices[1]);
                            faceBuilder.Vertex(indices[2]);
                            obj.Face(faceBuilder.Build());
                        }
                    }
                    else if (indices.Length > 3)
                    {
                        faceBuilder.Vertex(indices[2]);
                        faceBuilder.Vertex(indices[3]);
                        faceBuilder.Vertex(indices[1]);
                        faceBuilder.Vertex(indices[0]);
                        obj.Face(faceBuilder.Build());
                        if ((prim.Flags & TMDPrimitivePacket.PrimitiveFlags.DoubleSided) != 0)
                        {
                            faceBuilder.Clear();
                            faceBuilder.Vertex(indices[0]);
                            faceBuilder.Vertex(indices[1]);
                            faceBuilder.Vertex(indices[3]);
                            faceBuilder.Vertex(indices[2]);
                            obj.Face(faceBuilder.Build());
                        }
                    }*/
                    faceBuilder.Clear();
                }
                indexBase += tmd.ObjectTable[i].Vertices.Length;
            }

            return obj.ToString();
        }

        public static string RenderableToObjFile(IRenderable mesh)
        {
            int vertCount = mesh.Verts.Vertices.Length;
            int triCount = mesh.Verts.Tris;

            ObjBuilder objString = new ObjBuilder();
            WriteOBJHeader(objString, vertCount, triCount);

            foreach (var vert in mesh.Verts.Vertices)
            {
                objString.Vertex(vert.Position);
            }

            foreach (var vert in mesh.Verts.Vertices)
            {
                objString.Normal(vert.Normal);
            }

            foreach (var vert in mesh.Verts.Vertices)
            {
                objString.UV(vert.TexCoord);
            }

            ObjFaceBuilder faceBuilder = new ObjFaceBuilder();
            for (int i = 0; i < mesh.Verts.Length; i+= 3)
            {
                int objIndex = mesh.Verts.Indices[i] + 1;
                faceBuilder.Vertex(objIndex, objIndex, objIndex);
                faceBuilder.Vertex(objIndex + 1, objIndex + 1, objIndex + 1);
                faceBuilder.Vertex(objIndex + 2, objIndex + 2, objIndex + 2);
                objString.Face(faceBuilder.Build());
                faceBuilder.Clear();
            }

            return objString.ToString();
        }

        public static string RenderableListToObjFile(List<IRenderable> meshes)
        {
            int vertCount = 0;
            int triCount = 0;
            foreach (var mesh in meshes)
            {
                vertCount += mesh.Verts.Vertices.Length;
                triCount += mesh.Verts.Tris;
            }

            ObjBuilder objString = new ObjBuilder();
            WriteOBJHeader(objString, vertCount, triCount);
            
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            foreach (var mesh in meshes)
            {
                foreach (var vert in mesh.Verts.Vertices)
                {
                    positions.Add(vert.Position);
                    normals.Add(vert.Normal);
                    uvs.Add(vert.TexCoord);
                }
            }

            foreach (var pos in positions)
            {
                objString.Vertex(pos);
            }

            foreach (var norm in normals)
            {
                objString.Normal(norm);
            }

            foreach (var uv in uvs)
            {
                objString.UV(uv);
            }

            int faceBase = 0;
            int meshNumber = 0;
            foreach (var mesh in meshes)
            {
                objString.Group($"Mesh {meshNumber}");

                ObjFaceBuilder faceBuilder = new ObjFaceBuilder();
                for (int i = 0; i < mesh.Verts.Length; i += 3)
                {
                    int objIndex1 = faceBase + mesh.Verts.Indices[i] + 1;
                    int objIndex2 = faceBase + mesh.Verts.Indices[i + 2] + 1;
                    int objIndex3 = faceBase + mesh.Verts.Indices[i + 1] + 1;
                    faceBuilder.Vertex(objIndex1, objIndex1, objIndex1);
                    faceBuilder.Vertex(objIndex2, objIndex2, objIndex2);
                    faceBuilder.Vertex(objIndex3, objIndex3, objIndex3);
                    objString.Face(faceBuilder.Build());
                    faceBuilder.Clear();
                }

                meshNumber++;
                faceBase += mesh.Verts.Vertices.Length;
            }

            return objString.ToString();
        }

        private static void WriteOBJHeader(ObjBuilder builder, int verts, int faces)
        {
            builder.Comment("Generated by LSDView, created by Figglewatts, 2018");
            builder.Comment("See www.lsdrevamped.net for more info");
            builder.Comment($"Vertices: {verts}");
            builder.Comment($"Faces: {faces}");
        }
    }
}