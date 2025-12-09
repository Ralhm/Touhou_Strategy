using Godot;
using System;
using static Godot.TextServer;

public partial class Cursor : Node2D
{

    private static Cursor _instance;

    public static Cursor Instance { get { return _instance; } }


    private Tile FocusedTile;

    public override void _Ready()
    {
        base._Ready();

        if (_instance != null && _instance != this)
        {
            QueueFree();
        }
        else
        {
            _instance = this;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input.IsActionJustPressed("LeftClick"))
        {

            MapManager.Instance.OnTileSelect(FocusedTile);

            //GD.Print("Left Clicked!");
        }

    }


    public void SetFocusedOnTile(Tile tile)
    {
        FocusedTile = tile;
        GlobalPosition = tile.GlobalPosition;

    }

    public Tile GetFocusedTile()
    {
        return FocusedTile;
    }

}
