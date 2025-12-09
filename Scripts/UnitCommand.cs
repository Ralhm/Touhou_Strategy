using Godot;
using System;
using System.Collections.Generic;



//Let this class represent aall actions done by a unit
[System.Serializable]
public partial class UnitCommand : Command
{

    List<Command> CommandList;

    public List<Command> GetCommands()
    {
        return CommandList;
    }

    public override void Undo()
    {
        GD.Print("Undoing CommandList Commands: " + CommandList.Count);

        for (int i = CommandList.Count - 1; i >= 0; i--)
        {
            CommandList[i].Undo();
        }
    }


    public UnitCommand()
    {
        CommandList = new List<Command>();

    }

    public UnitCommand(List<Command> commandList)
    {
        

    }

    public UnitCommand(UnitCommand NewCommand)
    {

        CommandList = new List<Command>(NewCommand.GetCommands());
        //Array.Copy(NewCommand.GetCommands(), CommandList, NewCommand.GetCommands().Count);


    }


    public void AddCommand(Command command)
    {

        CommandList.Add(command);
    }

    public void AddMoveCommand(Character character, Tile EndTile, Tile StartTile)
    {
        CommandList.Add(new MoveCommand(character, EndTile, StartTile));
    }

    public void AddWaitCommand(Character character)
    {
        CommandList.Add(new WaitCommand(character));
    }



    public void AddAttackCommand(Character Attacker, Character Target)
    {
        CommandList.Add(new AttackCommand(Attacker, Target));
    }


    public void AddMoveCommandCopy(MoveCommand command)
    {
        MoveCommand newCommand = new MoveCommand(command);
        CommandList.Add(newCommand);
    }


    public void AddAttackCommandCopy(AttackCommand command)
    {
        AttackCommand newCommand = new AttackCommand(command);
        CommandList.Add(command);
    }

    public void RemoveCommand(Command command)
    {
        CommandList.Remove(command);
    }

    public Command GetMostRecentCommand()
    {
        return CommandList[0];
    }

    public void ClearCommands()
    {
        CommandList.Clear();
    }


}
