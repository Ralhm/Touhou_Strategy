using Godot;
using System;

public partial class WaitButton : Button
{

    public override void _Ready()
    {
        base._Ready();

        Pressed += OnButtonPressed;
    }

    public void OnButtonPressed()
    {
        MapManager.Instance.WaitAction();
    }


}
