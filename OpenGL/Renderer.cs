using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

public class PyramidTruncadaWindow : GameWindow
{
    // Variables para FPS.
    private double fps;
    private double frameTimeAccumulator = 0;
    private int frameCount = 0;

    private int shaderProgram;
    private float rotation = 0;
    private float rotationCamaraX = 0;
    private float rotationCamaraY = 0;
    private float rotationCamaraZ = 0;

    private Matrix4 rotacionCamara;

    // Constructor que debemos montar para la Clase.
    public PyramidTruncadaWindow(GameWindowSettings gws, NativeWindowSettings nws)
    : base(gws, nws) { }

    private List<Entity> entidades = new();

    protected override void OnLoad()
    {
        base.OnLoad();

        rotacionCamara = Matrix4.CreateTranslation(0, 0, 0);
        // Setup OpenGL
        // Darle color al fondo.
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        // Tema de Z-Buffering creo, si hay algo mas cerca que un objeto lejano, el lejano no se pinta.
        GL.Enable(EnableCap.DepthTest);

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
    
    public void SetEntity(Entity entity)
    {
        // Crear VAO
        int vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        // Crear VBO posiciones
        int positionVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, entity.vertices.Length * sizeof(float), entity.vertices, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(0);

        // Crear VBO colores
        int colorVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, colorVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, entity.colores.Length * sizeof(float), entity.colores, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        // Crear EBO índices
        int ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, entity.indices.Length * sizeof(uint), entity.indices, BufferUsageHint.StaticDraw);

        entity.vao = vao;
        entity.positionVbo = positionVBO;
        entity.colorVbo = colorVBO;
        entity.ebo = ebo;
        entity.indexCount = entity.indices.Length;
        entidades.Add(entity);
    }

    // Aquí puedes hacer toda la lógica de entrada, física, etc.
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // Delta del scroll desde el último frame
        var scrollDelta = MouseState.ScrollDelta;
        var mouseDelta = MouseState.Delta;


        rotationCamaraZ += scrollDelta.Y;

        var isLeftClickPressed = MouseState.IsButtonDown(MouseButton.Left);
        var isRightClickPressed = MouseState.IsButtonDown(MouseButton.Right);

        if (isLeftClickPressed)
        {
            Console.WriteLine(mouseDelta);
            rotationCamaraX += mouseDelta.X /50;
            rotationCamaraY -= mouseDelta.Y /50;
        }
        if (isRightClickPressed)
        {
            rotacionCamara *= Matrix4.CreateRotationX(mouseDelta.Y /50);
            rotacionCamara *= Matrix4.CreateRotationY(mouseDelta.X /50);
        }


    }


    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        // La rotacion se basa en el tiempo transcurrido, pero luego usamos el Seno del valor para que se mantenga estable.
        rotation *= (float)args.Time;

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.UseProgram(shaderProgram);

        // Matrices
        // Añadimos la rotacion a la vista (camara), para que de vueltitas alrededor de los objetos.
        Matrix4 view = Matrix4.CreateTranslation(rotationCamaraX, rotationCamaraY, -5f + rotationCamaraZ);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);

        int viewLoc = GL.GetUniformLocation(shaderProgram, "view");
        int projLoc = GL.GetUniformLocation(shaderProgram, "projection");

        GL.UniformMatrix4(viewLoc, false, ref view);
        GL.UniformMatrix4(projLoc, false, ref projection);
        
        foreach (Entity entidad in entidades)
        {
            GL.BindVertexArray(entidad.vao);
            // Añadimos la rotacion a la rotacion del objeto
            Matrix4 model = entidad.transform * Matrix4.CreateScale(entidad.scale) * rotacionCamara;
            int modelLoc = GL.GetUniformLocation(shaderProgram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.BindVertexArray(entidad.vao);
            GL.DrawElements(PrimitiveType.Triangles, entidad.indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        SwapBuffers();

        // Contador de FPS en el Título.
        frameCount++;
        frameTimeAccumulator += args.Time;
        if (frameTimeAccumulator >= 1.0)
        {
            fps = frameCount / frameTimeAccumulator;
            Title = $"Pirámide truncada - FPS: {fps:F2}";
            frameCount = 0;
            frameTimeAccumulator = 0;
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        foreach (Entity entidad in entidades)
        {
            GL.DeleteBuffer(entidad.positionVbo);
            GL.DeleteBuffer(entidad.colorVbo);
            GL.DeleteBuffer(entidad.ebo);
            GL.DeleteVertexArray(entidad.vao);
        }

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
}
