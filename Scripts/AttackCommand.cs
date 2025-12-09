using Godot;
using System;

public partial class AttackCommand : Command
{
    public Character StoredAttacker;
    public Character StoredTarget;

    //You might just need to store things like damage output for this to work in a more fleshed out manner


    public AttackCommand(Character Attacker, Character Target)
    {
        StoredAttacker = Attacker;
        StoredTarget = Target;
    }


    public AttackCommand(AttackCommand command)
    {
        StoredAttacker = command.StoredAttacker;
        StoredTarget = command.StoredTarget;
    }



    public override void Execute()
    {
        base.Execute();

        StoredAttacker.AttackTarget(StoredTarget);

    }

    public override void Undo()
    {
        base.Undo();

        StoredAttacker.UndoAttack(StoredAttacker, StoredTarget);
    }


}
