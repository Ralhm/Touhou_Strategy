using Godot;
using System;
using System.Diagnostics;

public partial class MoveCommand : Command
{
    public Character StoredCharacter;
    public Tile StoredEndTile;
    public Tile StoredStartTile;


    public MoveCommand(MoveCommand command)
    {
        StoredCharacter = command.StoredCharacter;
        StoredEndTile = command.StoredEndTile;
        StoredStartTile = command.StoredStartTile;
    }


    public MoveCommand(Character character, Tile EndTile, Tile StartTile)
    {
        StoredCharacter = character;
        StoredEndTile = EndTile;
        StoredStartTile = StartTile;
    }



    public override void Execute()
    {
        StoredCharacter.BeginMovement(StoredEndTile);
    }

    public override void Undo()
    {
        GD.Print("MOVE UNDO");
        StoredCharacter.UndoMovement(StoredStartTile, StoredEndTile);
    }
}
