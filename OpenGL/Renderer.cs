using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

public class PyramidTruncadaWindow : GameWindow
{
    private double fps;
    private double frameTimeAccumulator = 0;
    private int frameCount = 0;

    private int shaderProgram;
    private int modelLoc, viewLoc, projLoc;

    private Camera camera;
    private float cameraSpeed = 60f;     // velocidad WASD
    private float mouseSensitivity = 0.2f;

    private bool firstMouseMove = true;
    private Vector2 lastMousePos;

    private List<Entity> entidades = new();
    private int modelSelected = 0;

    List<Keys> teclasNumericas = new()
    {
        Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5,
        Keys.D6, Keys.D7, Keys.D8, Keys.D9
    };

    public PyramidTruncadaWindow(GameWindowSettings gws, NativeWindowSettings nws)
        : base(gws, nws) { }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        camera = new Camera();

        // === SHADERS ===
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

        modelLoc = GL.GetUniformLocation(shaderProgram, "model");
        viewLoc = GL.GetUniformLocation(shaderProgram, "view");
        projLoc = GL.GetUniformLocation(shaderProgram, "projection");
    }

    public void SetEntity(Entity entity)
    {
        int vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        int positionVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, entity.vertices.Length * sizeof(float), entity.vertices, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(0);

        int colorVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, colorVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, entity.colores.Length * sizeof(float), entity.colores, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(1);

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

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        var keyboard = KeyboardState;
        var mouse = MouseState;

        float speed = cameraSpeed * (float)args.Time;

        // === Movimiento WASD ===
        if (keyboard.IsKeyDown(Keys.W))
            camera.Position += camera.Front * speed;
        if (keyboard.IsKeyDown(Keys.S))
            camera.Position -= camera.Front * speed;
        if (keyboard.IsKeyDown(Keys.A))
            camera.Position -= Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * speed;
        if (keyboard.IsKeyDown(Keys.D))
            camera.Position += Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * speed;
        if (keyboard.IsKeyDown(Keys.Space))
            camera.Position += camera.Up * speed;
        if (keyboard.IsKeyDown(Keys.LeftShift))
            camera.Position -= camera.Up * speed;

        // === Rotación de cámara con clic derecho ===
        if (mouse.IsButtonDown(MouseButton.Right))
        {
            if (firstMouseMove)
            {
                lastMousePos = mouse.Position;
                firstMouseMove = false;
            }

            Vector2 delta = mouse.Position - lastMousePos;
            lastMousePos = mouse.Position;

            camera.Yaw += delta.X * mouseSensitivity;
            camera.Pitch -= delta.Y * mouseSensitivity;
        }
        else
        {
            firstMouseMove = true;
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.UseProgram(shaderProgram);

        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix(Size.X / (float)Size.Y);

        GL.UniformMatrix4(viewLoc, false, ref view);
        GL.UniformMatrix4(projLoc, false, ref projection);

        foreach (var entidad in entidades)
        {
            GL.BindVertexArray(entidad.vao);
            Matrix4 model = entidad.transform;
            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.DrawElements(PrimitiveType.Triangles, entidad.indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        SwapBuffers();

        // FPS
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
        foreach (var e in entidades)
        {
            GL.DeleteBuffer(e.positionVbo);
            GL.DeleteBuffer(e.colorVbo);
            GL.DeleteBuffer(e.ebo);
            GL.DeleteVertexArray(e.vao);
        }
        GL.DeleteProgram(shaderProgram);
    }

    private void CheckShaderCompile(int shader)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0)
        {
            throw new Exception("Shader compile error: " + GL.GetShaderInfoLog(shader));
        }
    }

    private void CheckProgramLink(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0)
        {
            throw new Exception("Program link error: " + GL.GetProgramInfoLog(program));
        }
    }
}
