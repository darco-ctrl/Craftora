using Godot;
using System;

public partial class PlayerInteractions : Node3D
{
    GameManager GM;

    [Export] private Area3D ItemCollision;
    [Export] private Node3D SelectionBox;
    [Export] private Camera3D Camera;

    private PhysicsRayQueryParameters3D _rayQuery;
    private PhysicsDirectSpaceState3D _spaceState;

    public override void _Ready()
    {
		GM = GetTree().Root.GetNode<GameManager>("GameManager");
        InitializeRay();
    }

    public override void _Process(double delta)
    {
        UpdateRayCast();
        RunRayCheck();
    }

    private void InitializeRay()
    {
        _spaceState = GetWorld3D().DirectSpaceState;
        _rayQuery = new PhysicsRayQueryParameters3D
        {
            CollisionMask = 1 // interact collision
        };
    }

    private void UpdateRayCast()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector3 from = Camera.ProjectRayOrigin(mousePos);
        Vector3 to = from + Camera.ProjectRayNormal(mousePos) * GM.PlayerRange;

        _rayQuery.From = from;
        _rayQuery.To = to;

        var raycastResult = _spaceState.IntersectRay(_rayQuery);

        if (raycastResult.Count > 0)
        {
			Node3D collider = raycastResult["collider"].As<Node3D>();
            if (collider != null && collider.IsInGroup("block"))
            {
                SelectionBox.Position = collider.Position;
            }
            else
            {
                Vector3 pos = (Vector3)raycastResult["position"];

                int y = pos.Y < 0 ? 0 : (int)pos.Y;

                ItemCollision.Position = pos;
                SelectionBox.Position = new Vector3(Grid(pos.X), y - 1, Grid(pos.Z));
                SelectionBox.Visible = true;
            }
        }
        else
        {
            SelectionBox.Visible = false;
        }
    }

    private void RunRayCheck()
    {
        if (_rayQuery != null)
        {
            var raycastResult = _spaceState.IntersectRay(_rayQuery);
            if (raycastResult.Count > 0)
            {
                var collider = raycastResult["collider"].As<Node3D>();
                if (collider != null && collider.IsInGroup("interactable"))
                {
                    if (Input.IsActionJustPressed("attack"))
                    {
                        collider.Call("Clicked");
                    }
                }
            }
        }
    }

    private int Grid(float num)
    {
        int _num;
        if (num < 0)
        {
            _num = (int)(num - 1);
        }
        else
        {
            _num = (int)(num);
        }

        return _num;
    }
}
