using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;


public struct coloresZonas
{
    public string nombreZona;
    public int inicioZona;
    public int finZona;
}


class PyramidTruncadaWindow : GameWindow
{


    private int vao;
    private int positionVBO;
    private int colorVBO;
    private int ebo;

    private int shaderProgram;
    private float rotation;

    public PyramidTruncadaWindow(GameWindowSettings gws, NativeWindowSettings nws)
    : base(gws, nws) { }

    private uint[] indices;

    private float scale = 0.01f;

    public List<coloresZonas> zonas = new();


    protected override void OnLoad()
    {
        base.OnLoad();

        string carpetaModelos3d = "Modelos3d/";
        string rutaOjoPirojo =  "OjoPirojo/eyeball.obj";
        string rutaEspada = "Espada/model.obj";


        SimpleObjLoader loader = new SimpleObjLoader();
        //loader.Load(carpetaModelos3d + rutaOjoPirojo);
        loader.Load(carpetaModelos3d + rutaEspada);


        // Posiciones
        var positions = loader.Vertices.ToArray();

        // Índices
        indices = loader.Indices.ToArray();

        zonas = loader.ZonasModelo;

        // Colores aleatorios
        float[] colors = new float[positions.Length];
        DameColoresZonas(ref colors, positions);


        // Después subes buffers con positions, colors, indices igual que antes


        // Setup OpenGL
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        // Crear VAO
        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        
        // Como queremos ver los triangulos, si rellenados, que caras etc.
        GL.PolygonMode(TriangleFace.Front, PolygonMode.Fill);

        // Crear VBO posiciones
        positionVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, positions.Length * sizeof(float), positions, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(0);

        // Crear VBO colores
        colorVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, colorVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        // Crear EBO índices
        ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        // Compilar shaders y linkear programa
        string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aColor;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            out vec3 vertexColor;

            void main()
            {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
                vertexColor = aColor;
            }
        ";

        string fragmentShaderSource = @"
            #version 330 core
            in vec3 vertexColor;
            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(vertexColor, 1.0);
            }
        ";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);
        CheckShaderCompile(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);
        CheckShaderCompile(fragmentShader);

        shaderProgram = GL.CreateProgram();
        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);
        GL.LinkProgram(shaderProgram);
        CheckProgramLink(shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }
    private void DameColoresZonas(ref float[] colores, float[] vertices)
    {
        Random rnd = new Random();
        foreach (var zona in zonas)
        {
            float colorZonaR = (float)rnd.NextDouble();
            float colorZonaG = (float)rnd.NextDouble();
            float colorZonaB = (float)rnd.NextDouble();
            int inicioZona = zona.inicioZona;
            int finZona = zona.finZona;
            string nomZona = zona.nombreZona;
            for (int i = inicioZona; i < finZona ; i += 3)
            {
                colores[i] = colorZonaR;
                colores[i + 1] = colorZonaG;
                colores[i + 2] = colorZonaB;
            }
        }
    }
    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        rotation += (float)args.Time;

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.UseProgram(shaderProgram);

        // Matrices
        Matrix4 model = Matrix4.CreateScale(scale) * Matrix4.CreateRotationY(rotation) * Matrix4.CreateRotationX(rotation * 0.5f);
        Matrix4 view = Matrix4.CreateTranslation((float)Math.Sin(0 + rotation), (float)Math.Sin(0 + rotation), -4f + (float)Math.Sin(0 + rotation));
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);

        int modelLoc = GL.GetUniformLocation(shaderProgram, "model");
        int viewLoc = GL.GetUniformLocation(shaderProgram, "view");
        int projLoc = GL.GetUniformLocation(shaderProgram, "projection");

        GL.UniformMatrix4(modelLoc, false, ref model);
        GL.UniformMatrix4(viewLoc, false, ref view);
        GL.UniformMatrix4(projLoc, false, ref projection);

        GL.BindVertexArray(vao);
        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.DeleteBuffer(positionVBO);
        GL.DeleteBuffer(colorVBO);
        GL.DeleteBuffer(ebo);
        GL.DeleteVertexArray(vao);
        GL.DeleteProgram(shaderProgram);
    }

    private void CheckShaderCompile(int shader)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0)
        {
            string info = GL.GetShaderInfoLog(shader);
            throw new Exception("Shader compilation failed: " + info);
        }
    }

    private void CheckProgramLink(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0)
        {
            string info = GL.GetProgramInfoLog(program);
            throw new Exception("Program linking failed: " + info);
        }
    }

    static void Main()
    {
        var nativeSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600),
            Title = "Pirámide truncada rotando con colores y EBO",
            Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible
        };

        using var window = new PyramidTruncadaWindow(GameWindowSettings.Default, nativeSettings);
        window.Run();
    }
}
