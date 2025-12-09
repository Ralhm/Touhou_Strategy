using Godot;
using System;

public partial class WaitCommand : Command
{

    public Character StoredWaiter;


    public WaitCommand(Character waiter)
    {
        StoredWaiter = waiter;

    }


    public override void Undo()
    {
        base.Undo();
    }



}
