﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static PyramidTruncadaWindow;


public class SimpleObjLoader
{
    public List<float> Vertices = new();
    public List<uint> Indices = new();
    public float[] colores;
    public List<coloresZonas> ZonasModelo = new();

    public void Load(string path)
    {
        Vertices.Clear();
        Indices.Clear();
        ZonasModelo.Clear();

        var indexList = new List<uint>();
        bool esZona = false;
        int parteZona = 0;
        string ultimaZonaSTR = "";
        coloresZonas zona;
        foreach (var line in File.ReadLines("../../../" + path))
        {
            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;
            switch (parts[0])
            {
                case "g":
                {
                    // Esta lógica es como la mayor basura que he hecho en 3 minutos para que me de el rango de las partes del modelo. Mejorable.
                    if (esZona)
                    {
                        // Esto de ZonasModelo es un delimitador de colores por zonas del modelo, sin mas.
                        ultimaZonaSTR = parts[1].Split(".")[0];
                        zona.inicioZona = parteZona * 3; // hay 3 vertices por linea
                        zona.finZona = Vertices.Count; // Hay 3 vertices por linea
                        zona.nombreZona = ultimaZonaSTR;
                        parteZona = Vertices.Count / 3;
                        ZonasModelo.Add(zona);
                    }
                    else
                    {
                        esZona = true;
                        parteZona = Vertices.Count / 3;
                    }
                    break;
                }
                case "v":
                {
                    if (parts.Length < 4) continue;
                    for(int i = 1; i < 4; i++)
                    {
                        Vertices.Add(float.Parse(parts[i], CultureInfo.InvariantCulture));
                    }
                    
                    break;
                }
                case "f":
                {
                    if (parts.Length < 4) continue;

                    // Extraemos todos los índices de la cara
                    List<uint> faceIndices = new List<uint>();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var vertexIndexStr = parts[i].Split('/')[0];
                        uint index = uint.Parse(vertexIndexStr) - 1;
                        faceIndices.Add(index);
                    }

                    // Triangulación tipo fan
                    // Aqui hace una vaina gangster.
                    for (int i = 1; i < faceIndices.Count - 1; i++)
                    {
                        Indices.Add(faceIndices[0]);
                        Indices.Add(faceIndices[i]);
                        Indices.Add(faceIndices[i + 1]);
                    }
                    break;
                }
                default: 
                { break; }
            }

        }

        zona.inicioZona = parteZona * 3; // hay 3 vertices por linea
        zona.finZona = Vertices.Count; // Hay 3 vertices por linea
        zona.nombreZona = ultimaZonaSTR;
        ZonasModelo.Add(zona);

        colores = new float[Vertices.ToArray().Length];
        DameColoresZonas(ref colores);
    }
    private void DameColoresZonas(ref float[] colores)
    {
        // Por cada zona, le damos un color a los vertices.
        Random rnd = new Random();
        foreach (var zona in ZonasModelo)
        {
            float colorZonaR = (float)rnd.NextDouble();
            float colorZonaG = (float)rnd.NextDouble();
            float colorZonaB = (float)rnd.NextDouble();
            int inicioZona = zona.inicioZona;
            int finZona = zona.finZona;
            string nomZona = zona.nombreZona;
            // Vuelve a hacer una vaina gangster.
            for (int i = inicioZona; i < finZona; i += 3)
            {
                colores[i] = colorZonaR;
                colores[i + 1] = colorZonaG;
                colores[i + 2] = colorZonaB;
            }
        }
    }
}
