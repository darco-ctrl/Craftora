using Godot;
using System;

public partial class PlayerCamera : Node3D
{
    GameManager GM;

    [Export] public Node3D CameraGroundPivot;
    [Export] public Node3D CameraPivot;
    [Export] public Node3D Camera;

    [Export] public float Speed = 0.5f;
    [Export] public float Acceleration = 0.1f;
    [Export] public float CameraZoomInOutSpeed = 20.0f;
    [Export] public float MaxZoom = 3.0f;
    [Export] public float MazZoomOut = 10.0f;

    private Vector3 CameraVelocity = Vector3.Zero;

    public override void _Ready()
    {
        GM = GetTree().Root.GetNode<GameManager>("GameManager");
        GM.SetPlayer(this as PlayerCamera);
    }

    public override void _PhysicsProcess(double delta)
    {
        CameraRotationAndPosition((float)delta);
        CameraInputMovement((float)delta);
    }


    private void CameraRotationAndPosition(float delta)
    {
        CameraPivot.LookAt(CameraGroundPivot.Position);

        int InputDirection = 0;

        if (Input.IsActionJustPressed("zoom_in"))
        {
            InputDirection = 1;
        }
        else if (Input.IsActionJustPressed("zoom_out"))
        {
            InputDirection = -1;
        }

        Vector3 ForwardDirection = -CameraPivot.GlobalTransform.Basis.Z;
        Vector3 NewPosition = CameraPivot.Position;

        NewPosition += ForwardDirection * CameraZoomInOutSpeed * InputDirection * delta;

        CameraPivot.Position = NewPosition;
    }

    private void CameraInputMovement(float delta)
    {
        Vector3 InputDirection = Vector3.Zero;

        if (Input.IsActionPressed("move_forward")) { InputDirection.Z -= 1; }
        if (Input.IsActionPressed("move_backward")) { InputDirection.Z += 1; }
        if (Input.IsActionPressed("move_left")) { InputDirection.X -= 1; }
        if (Input.IsActionPressed("move_right")) { InputDirection.X += 1; }

        InputDirection = InputDirection.Normalized();

        float YRotation = CameraPivot.GlobalTransform.Basis.GetEuler().Y;
        Basis FlatRotation = new Basis(Vector3.Up, YRotation);

        Vector3 Direction = FlatRotation * InputDirection;

        if (InputDirection != Vector3.Zero)
        {
            CameraVelocity += Direction * Speed * delta;
        }
        else
        {
            CameraVelocity = CameraVelocity.Lerp(Vector3.Zero, 0.1f);
        }

        CameraVelocity = CameraVelocity.LimitLength(Speed);

        CameraGroundPivot.Position += CameraVelocity;
    }
}
