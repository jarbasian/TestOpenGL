using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static PyramidTruncadaWindow;


public class SimpleObjLoader
{
    public List<float> Vertices = new();
    public List<uint> Indices = new();
    public coloresZonas[] ZonasModelo = new coloresZonas[20]; 

    public void Load(string path)
    {
        Vertices.Clear();
        Indices.Clear();

        var vertexList = new List<Vector3>();
        var indexList = new List<uint>();
        bool esZona = false;
        foreach (var line in File.ReadLines(path))
        {

            
            int parteZona = 0;
            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;
            if (parts[0] == "g")
            {
                if (esZona){
                    esZona = false;

                    ZonasModelo[ZonasModelo.Length].nombreZona = parts[1].Split(".")[0];
                    ZonasModelo[ZonasModelo.Length].inicioZona = parteZona;
                    ZonasModelo[ZonasModelo.Length].finZona = vertexList.Count;
                }
                else
                {
                    esZona = true;
                    parteZona = vertexList.Count;
                }
                
            }
            if (parts[0] == "v") // vértice
            {
                if (parts.Length < 4) continue;
                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                vertexList.Add(new Vector3(x, y, z));
            }
            else if (parts[0] == "f") // cara
            {
                if (parts.Length < 4) continue;

                // OBJ usa índices 1-based, lo corregimos a 0-based
                for (int i = 1; i <= 3; i++)
                {
                    // Los vértices pueden venir como "1", "1/1/1", etc. 
                    var vertexIndexStr = parts[i].Split('/')[0];
                    uint index = uint.Parse(vertexIndexStr) - 1;
                    indexList.Add(index);
                }

                // Si la cara tiene más de 3 vértices (como un quad), se puede triangulizar,
                // pero aquí solo consideramos triángulos
            }
        }

        // Ahora pasamos a arrays planos para OpenGL
        foreach (var v in vertexList)
        {
            Vertices.Add(v.X);
            Vertices.Add(v.Y);
            Vertices.Add(v.Z);
        }

        Indices.AddRange(indexList);
    }
}
