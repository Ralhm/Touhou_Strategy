using Godot;
using System;

public partial class AttackButton : Button
{

    public override void _Ready()
    {
        base._Ready();

        Pressed += OnButtonPressed;
    }

    public void OnButtonPressed()
    {
        MapManager.Instance.EnterSelectTargetMode();
    }

}
