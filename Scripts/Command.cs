using System.Collections;
using System.Collections.Generic;
using Godot;

[System.Serializable]
public partial class Command
{


    public virtual void Execute()
    {

    }

    public virtual void Undo()
    {
        GD.Print("GENERIC UNDO");
    }


    public virtual void Redo()
    {

    }


}
