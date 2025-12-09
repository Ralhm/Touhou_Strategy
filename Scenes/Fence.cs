using Godot;
using System;

public partial class Fence : Node2D
{

    [Export]
    public Vector2I StartingCoord;


    public override void _Ready()
    {
        base._Ready();

        Tile tile = MapManager.Instance.GetTile(StartingCoord);
        tile.IsBlocked = true;
        Position = tile.GlobalPosition;

    }

}
