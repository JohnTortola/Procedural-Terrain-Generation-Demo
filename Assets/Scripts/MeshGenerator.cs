using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
        public static MeshData GenerateTerrainMesh(float[,] heightmap, float heightMultiplier, AnimationCurve HeightCurve, int lod)
    {
        AnimationCurve meshHeightCurve = new AnimationCurve(HeightCurve.keys);
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);
        float topRightX = (width - 1) / 2f;
        float topRightZ = (height - 1) / 2f;

        int lodIncrement = (lod == 0)?1:lod * 2;    //o LOD 0 deverá ser 1 para manter o detalhe e impedir a divisão por 0
        int verticesPerLine = (width - 1) / lodIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;
        for(int y = 0; y < height; y += lodIncrement)
        {
            for(int x = 0; x < width; x += lodIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(topRightX - x, meshHeightCurve.Evaluate(heightmap[x, y]) * heightMultiplier, topRightZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if(x < width -1 && y < height - 1)  //as beiras da mesh já são usadas em triângulos pelo ponto anterior
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);    //i             i+1...
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);                 //i+width       i+width+1...
                }
                vertexIndex++;
            }
        }

        return meshData;
    }
}
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    int triangleIndex;
    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        uvs = new Vector2[meshWidth * meshHeight];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex+2] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex] = c;

        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
