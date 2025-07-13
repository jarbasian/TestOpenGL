using OpenTK.Mathematics;
using System.Collections.Generic;

public struct Entity
{
    public float[] vertices;
    public uint[] indices;
    public float[] colores;
    public int vao;
    public int positionVbo;
    public int colorVbo;
    public int ebo;
    public int indexCount;
    public Matrix4 transform;
    public float scale;

}
