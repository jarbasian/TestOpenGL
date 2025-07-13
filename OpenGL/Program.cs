using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;



// Escala que usamos para los objetos mostrados (Se podria hacer por objeto en vez de que fuera general.
public class Program
{

    public static float scale = 0.01f;

    static void Main()
    {
        // Directorios de los archivos .obj que importamos, se hace manual, molaria un selector sencillo que te permita abrir el archivo directamente.
        string carpetaModelos3d = "Modelos3d/";
        List<string> rutasObj = new()
    {
        "OjoPirojo/eyeball.obj",
        "Espada/model.obj",
        "Skull/Skull.obj",
        "Planta/Planta.obj"
    };
        List<Entity> entities = new();

        var nativeSettings = new NativeWindowSettings()
        {
            ClientSize = new Vector2i(2560, 1440),
            Title = "Pirámide truncada rotando con colores y EBO",
            Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible
        };

        using var window = new PyramidTruncadaWindow(GameWindowSettings.Default, nativeSettings);

        SimpleObjLoader loader = new SimpleObjLoader();
        //loader.Load(carpetaModelos3d + rutaOjoPirojo);
        var i = 1f;
        foreach (string objeto in rutasObj)
        {
            string ruta = carpetaModelos3d + objeto;
            loader.Load(ruta);
            // Vertices/Posicion de los puntos que forman los Triangulos.
            var vertices = loader.Vertices.ToArray();

            // Índices/Caras de los triangulos.
            var indices = loader.Indices.ToArray();
            // Colores.
            float[] colores = loader.colores;
            Entity entidad = new();
            entidad.vertices = vertices;
            entidad.indices = indices;
            entidad.colores = colores;
            entidad.scale = scale;

            // Aqui posicionariamos dariamos escala al modelo, en este transform.

            entidad.transform = Matrix4.CreateTranslation(i, 0, 0);
            i += 30;
            window.SetEntity(entidad);
        }


        window.Run();





        





    }

}
