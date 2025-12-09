using Godot;
using System;

[GlobalClass]
public partial class CharacterData : Resource
{

    [Export]
    public int MaxHealth;

    [Export]
    public bool IsAlly;

    [Export]
    public String Name;

    [Export]
    public int AttackRange;

    [Export]
    public int MoveRange;

    [Export]
    public int AttackPower;

    public int GetMaxHealth()
    {
        return MaxHealth;
    }


}
