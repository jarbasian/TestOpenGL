using OpenTK.Mathematics;
using System;

public class Camera
{
    public Vector3 Position { get; set; } = new(0f, 0f, 5f);
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, Up));

    private float yaw = -90f;   // mirando hacia -Z
    private float pitch = 0f;
    private float fov = 65f;

    public float Yaw
    {
        get => yaw;
        set
        {
            yaw = value;
            UpdateVectors();
        }
    }

    public float Pitch
    {
        get => pitch;
        set
        {
            pitch = MathHelper.Clamp(value, -89f, 89f);
            UpdateVectors();
        }
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + Front, Up);
    }

    public Matrix4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspectRatio, 0.1f, 2000f);
    }

    private void UpdateVectors()
    {
        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
        Front = Vector3.Normalize(front);
    }
}
