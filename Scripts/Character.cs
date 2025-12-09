using Godot;
using System.Collections;
using System.Collections.Generic;


public enum CurrentState
{
    NotActed,
    Moved,
    Moving,
    Selected,
    SelectingMovement,
    Attacking,
    Dead,
    Acted
}


enum Behavior
{
    Waiting,
    Aggressive,
    Cowardly

}


//Let Allies and Enemies inherit from the same class
//This way, we don't need to distinguish between the two
//And it means allegiances can be changed very easily if needed
//Or allow for autobattle


//Do Consider at least making allies and enemies their own classes
//So that they can overload a few functions and reduce the number of IsAlly Checks
public partial class Character : Node2D
{
    [Export]
    public CharacterData Data;

    [Export]
    public AnimatedSprite2D Sprite;


    [Export]
    public float Speed = 5.0f;

    [Export]
    public int Health;


    [Export]
    public Vector2I StartingCoord;

    [Export]
    public Tile CurrentTile;

    bool Waited = false;

    [Export]
    public ProgressBar HealthBar;


    [Export]
    public CurrentState State = CurrentState.NotActed;

    [Export]
    public Character Target;

    protected int DistanceToTravel = 0;
    protected int DistanceTraveled = 0;

    protected Pathfinder Pathfinder = new Pathfinder();
    protected Rangefinder RangeFinder = new Rangefinder();

    protected Godot.Collections.Array<Tile> Path;
    protected Godot.Collections.Array<Tile> PathEuc;

    [Export]
    public Godot.Collections.Array<Tile> MoveTilesInRange;

    [Export]
    public Godot.Collections.Array<Tile> AttackTilesInRange;

    [Export]
    public int AggroRange;

    Godot.Collections.Array<Character> TargetList = new Godot.Collections.Array<Character>();

    //Quick and dirty debug bool
    bool EnemySelected = false;

    public override void _Ready()
    {
        base._Ready();

        Sprite = GetNode<AnimatedSprite2D>("Sprite");
        HealthBar = GetNode<ProgressBar>("HealthBar");
        SetCurrentTile(MapManager.Instance.GetTile(StartingCoord));
        Position = CurrentTile.GlobalPosition;

        if (IsAlly())
        {
            MapManager.Instance.AddToAllies(this);
        }
        else
        {
            MapManager.Instance.AddToEnemies(this);
        }

        HealthBar.MaxValue = Data.MaxHealth;
        SetHealth(Data.MaxHealth);

    }

    public override void _Process(double delta)
    {
        base._Process(delta);



        if (State == CurrentState.Moving)
        {
            MoveAlongPath((float)delta);
        }

    }


    public void OnSelect()
    {

        if (IsAlly() && State == CurrentState.NotActed)
        {
            FindMovementTiles();
        }
        else if (!IsAlly())
        {
            if (!EnemySelected)
            {
                EnemySelected = true;
                DisplayMovementTiles();
            }
            else
            {
                EnemySelected = false;
                HideAllTiles();
            }

            //Show enemy stats or something
        }


    }

    public void DisplayMovementTiles()
    {
        MoveTilesInRange = RangeFinder.GetMovementTilesInRange(CurrentTile, this, Data.MoveRange);

        ShowAttackRangeTiles(Data.AttackRange);


        foreach (Tile tile in MoveTilesInRange)
        {
            tile.ShowTile();
        }

    }

    public void FindMovementTiles(bool DisplayArea = true)
    {
        State = CurrentState.SelectingMovement;
        MoveTilesInRange = RangeFinder.GetMovementTilesInRange(CurrentTile, this, Data.MoveRange);

        ShowAttackRangeTiles(Data.AttackRange);
        MapManager.Instance.SetGameState(GameState.SelectingMovement);

        if (!DisplayArea)
        {
            return;
        }

        foreach (Tile tile in MoveTilesInRange)
        {


            tile.ShowTile();
        }


        
    }

    public void OnDeSelect()
    {
        State = CurrentState.NotActed;
        HideAllTiles();
        MapManager.Instance.SetCurrentCharacter(null);
        MapManager.Instance.SetGameState(GameState.SelectingCharacter);
    }

    public void FindPath(Tile EndTile)
    {
        //Instead of running this twice, once with manhatten and once with euclidean
        //Try running manhatten recursively 
        //Or do a short loop, where you loop through the paths of the children of  the current tile and see which is shortest
        Path = Pathfinder.FindPath(CurrentTile, EndTile, IsAlly());
        PathEuc = Pathfinder.FindPath(CurrentTile, EndTile, IsAlly(), false);

        if (PathEuc.Count < Path.Count)
        {
            Path = PathEuc;
        }
    }




    public void BeginMovement(Tile EndTile)
    {
        GD.Print(Name + ": BEGINNING MOVEMENT");
        if (EndTile == null)
        {
            GD.Print("NULL TILE");
            return;
        }
        else if (EndTile.IsOccupied)
        {
            GD.Print("END TILE IS OCCUPPIED TILE");
            if (EndTile.CurrentCharacter == this)
            {
                MapManager.Instance.RegisterMoveCommand(this, CurrentTile, CurrentTile);
                HideAllTiles();
                FinishedMoving();
            }

            return;
        }

        FindPath(EndTile);




        //CurrentTile.ShowTile();
        //EndTile.ShowTile();
        CurrentTile.ClearTile();

        DistanceToTravel = Path.Count;
        State = CurrentState.Moving;

        HideAllTiles();


        MapManager.Instance.RegisterMoveCommand(this, EndTile, CurrentTile);
        SetCurrentTile(EndTile);

    }


    public virtual void MoveAlongPath(float dt)
    {
        float step = Speed * dt;

        Position = Position.Lerp(Path[0].GlobalPosition, step);


        if ((Path[0].GlobalPosition - GlobalPosition).Length() < 0.01f)
        {
            DistanceTraveled++;


            Path.RemoveAt(0);
        }

        if (DistanceTraveled == DistanceToTravel)
        {
            FinishedMoving();

        }

    }

    public void FinishedMoving()
    {
        DistanceTraveled = 0;
        State = CurrentState.Moved;

        if (IsAlly())
        {
            ShowAttackRangeTiles(Data.AttackRange, false);
            MapManager.Instance.ShowActionMenu();
        }
        else
        {
            GD.Print(Name + ": Beginning Attack!");
            BeginAttackTarget();
        }

 
    }


    public void SetHealth(int val)
    {
        Health += val;

        if (HealthBar != null)
        {
            HealthBar.Value = Health;
            
        }

        


        if (Health <= 0)
        {
            Die();
        }
    }

    public bool CanCounter(Character attacker)
    {
        AttackTilesInRange = RangeFinder.GetTilesInRange(CurrentTile, Data.AttackRange);


        if (AttackTilesInRange.Contains(attacker.CurrentTile))
        {
 
            return true;
        }
        return false;
    }

    public bool AttemptCounter(Character attacker)
    {
        if (Health <= 0)
        {
            GD.Print("-----I'M DEAD------");
            return false;
        }

        if (CanCounter(attacker))
        {
            BeginCounter(attacker);
            GD.Print("--------COUNTERED-------");
            return true;
        }


        GD.Print("FAILED TO COUNTER");

        return false;
    }

    public void BeginAttackTarget()
    {
        MapManager.Instance.RegisterAttackCommand(this, Target);
        Sprite.Play("AttackFront");
        Sprite.AnimationFinished += FinishAttack;
        State = CurrentState.Attacking;
    }


    public void AttackTarget(Character target)
    {
        target.SetHealth(-Data.AttackPower);

    }



    public void BeginCounter(Character target)
    {
        GD.Print("BEGINNING COUNTER");
        MapManager.Instance.RegisterAttackCommand(this, target);
        Target = target;
        Sprite.Play("AttackFront");
        Sprite.AnimationFinished += EndCounter;
        
    }

    public void EndCounter()
    {
        GD.Print("ENDING COUNTER");

        Sprite.AnimationFinished -= EndCounter;
        Sprite.Play("IdleFront");
        AttackTarget(Target);
        
        Target.FinishAction();
    }

    public void FinishAttack()
    {
        GD.Print(Name + ": Finished Attacking");
        Sprite.Play("IdleFront");
        Sprite.AnimationFinished -= FinishAttack;
        AttackTarget(Target);

        if (!Target.AttemptCounter(this))
        {
            GD.Print("Target did NOT Counter!");
            FinishAction();
        }
        else
        {
            GD.Print("Target Countered!");
        }

    }

    public void FinishAction()
    {

        if (State != CurrentState.Dead)
        {
            State = CurrentState.Acted;
        }

        
        

        if (IsAlly())
        {
            HideAllTiles();
            MapManager.Instance.OnAllyFinishedAction(this);
        }
        else
        {
            MapManager.Instance.OnEnemyFinishedAction(this);
        }
    }


    public int GetProjectedHealth(Character character)
    {
        return Health - character.Data.AttackPower;
    }


    public void Die()
    {
        State = CurrentState.Dead;
        CurrentTile.ClearTile();
        Hide();


        //QueueFree();
    }


    //SubtractMovement should be set to true if you want to shoow total attack range plus movement range
    //ONLY set this to false when a player has finished moving and you want to see their range from that position
    public void ShowAttackRangeTiles(int Range, bool SubtractMovement = true, bool DisplayTiles = true)
    {

        AttackTilesInRange.Clear();
        if (SubtractMovement)
        {

            foreach (Tile tile in MoveTilesInRange)
            {
                AttackTilesInRange.Add(tile);
            }

            AttackTilesInRange = RangeFinder.GetTilesInAttackRange(AttackTilesInRange, this, Range);

            if (!DisplayTiles)
            {
                return;
            }

            foreach (Tile tile in AttackTilesInRange)
            {
                if (!MoveTilesInRange.Contains(tile))
                {
                    if (!tile.IsBlocked)
                    {
                        tile.ShowAttackTile();
                    }
                }
            }
        }
        else
        {

            AttackTilesInRange.Add(CurrentTile);
            AttackTilesInRange = RangeFinder.GetTilesInAttackRange(AttackTilesInRange, this, Range);


            if (!DisplayTiles)
            {
                return;
            }
            foreach (Tile tile in AttackTilesInRange)
            {
                if (!tile.IsBlocked)
                {
                    tile.ShowAttackTile();
                }

                
                
            }
        }

    }

    public void HideAllTiles()
    {
        foreach (Tile tile in MoveTilesInRange)
        {
            tile.HideTile();
        }
        MoveTilesInRange.Clear();
        foreach (Tile tile in AttackTilesInRange)
        {
            tile.HideTile();
        }
        AttackTilesInRange.Clear();
    }


    public void SetCurrentTile(Tile tile)
    {
        //Position = tile.GlobalPosition;
        CurrentTile = tile;
        CurrentTile.SetIsOccupied(this);

    }

    public void SetTarget(Character target)
    {
        Target = target;
    }


    public virtual void UndoAction()
    {

    }

    public virtual void UndoAttack(Character Attacker, Character Target)
    {
        GD.Print("Attempting to Undo Movement!");
        if (State == CurrentState.Acted) //Undoing Action
        {
            SetState(CurrentState.NotActed);
        }

        Target.UndoDamage(Attacker.Data.AttackPower);
    }

    public void UndoDamage(int damage)
    {
        SetHealth(damage);
        if ((Health - damage) <= 0)
        {
            UndoDeath();
        }
    }

    public void UndoDeath()
    {
        State = CurrentState.NotActed;
        CurrentTile.SetIsOccupied(this);
        Show();
    }

    public virtual void UndoMovement(Tile StartTile, Tile EndTile)
    {
        GD.Print("Attempting to Undo Movement!");
        if (State == CurrentState.Acted) //Undoing Action
        {
            SetState(CurrentState.NotActed);
        }
        else if (State == CurrentState.Moved) // Undoing movement
        {
            GD.Print("Selecting Movement Undo!");
            State = CurrentState.SelectingMovement;
        }

        //HideAllTiles();


        EndTile.ClearTile();
        SetCurrentTile(StartTile);
        Position = CurrentTile.GlobalPosition;
        


    }


    public void Wait()
    {
        Waited = true;

        FinishAction();
    }

    public void UndoWaitCommand()
    {
        SetState(CurrentState.NotActed);
    }


    public bool IsAlly()
    {
        return Data.IsAlly;
    }

    public void SetState(CurrentState state)
    {
        State = state;
    }


    public bool IsTileInMoveRange(Tile tile)
    {
        return MoveTilesInRange.Contains(tile);
    }

    public bool IsTileInAttackRange(Tile tile)
    {
        return AttackTilesInRange.Contains(tile);
    }


    //Most likely unnecessary
    public bool IsTileInRange(Tile CurrentTile, Tile TargetTile, int range)
    {

        //return MoveTilesInRange.Contains(TargetTile);
        return (Pathfinder.FindPath(CurrentTile, TargetTile, IsAlly(), false).Count <= range);



    }

    public Godot.Collections.Array<Tile> GetPath(Tile CurrentTile, Tile TargetTile)
    {
        return Pathfinder.FindPath(CurrentTile, TargetTile, IsAlly(), false);
    }


    //Check if there are allies coming up on ur ass
    //If so start moving forward
    //Else, wait
    public void AggroCheck()
    {

    }

    public void SelectTarget()
    {
        TargetList.Clear();
        FindMovementTiles(false);
        ShowAttackRangeTiles(Data.AttackRange, true, false);
        int foundTargets = 0;


        /*
        foreach (Tile tile in AttackTilesInRange)
        {
            tile.ShowAttackTile();
        }

        foreach (Tile tile in MoveTilesInRange)
        {
            tile.ShowTile();
        }
        */

        GD.Print(Name + ": Selecting Target");

        foreach (Tile tile in AttackTilesInRange)
        {
            //Add Target if tile is occuppied and it's occuppant is an enemy
            //This will allow for decision making if we so choose
            if (tile.IsOccupied)
            {
                if (tile.CurrentCharacter.IsAlly() != IsAlly())
                {
                    TargetList.Add(tile.CurrentCharacter);
                    foundTargets++;
                }
            }



        }
        GD.Print(Name + ": End of For Loop: ");
        

        //Wait if there are no targets in range
        if (foundTargets == 0)
        {
            HideAllTiles();
            GD.Print("Waiting!");
            Wait();
        }

        Godot.Collections.Array<Tile> PotentialMoveToTiles = RangeFinder.GetTilesWithinTargetRange(TargetList[0].CurrentTile, this, Data.AttackRange);

        //ADD DECISION MAKING CODE HERE
        SetTarget(TargetList[0]);

        GD.Print(Name + "'s Current Target: " + TargetList[0].Name);

        //Could clean this up but it will do
        if (PotentialMoveToTiles.Count == 1)
        {
            if (PotentialMoveToTiles[0] == CurrentTile)
            {
                GD.Print(Name + ": I Must Attack from Where I am Standing");
                FinishedMoving();
            }
        }


        

        if (Data.AttackRange == 1)
        {
            if (PotentialMoveToTiles.Contains(CurrentTile))
            {
                FinishedMoving();
                return;
            }
        }

        GD.Print(Name + ": Attempting to access Move To Tiles");

        if (PotentialMoveToTiles.Count == 0)
        {
            //HideAllTiles();
            GD.Print(Name + ": NO MOVE TO TILES");
            Wait();
            return;
        }

        //MOVEMENT TILE DECISION CODE HERE
        //Foreach tile in PotentialMoveToTiles, calculate distance from target tile to PotentialMoveToTile
        //Make an alternate pathfinder that goes from target to tile and ignores everything, only good for getting distance

        int LowestDistIndex = 0;
        for (int i = 0; i < PotentialMoveToTiles.Count; i++)
        {
            Pathfinder.FindPath(Target.CurrentTile, PotentialMoveToTiles[i], IsAlly(), false, 0, true);

            if (PotentialMoveToTiles[i].G > PotentialMoveToTiles[LowestDistIndex].G)
            {
                LowestDistIndex = i;
            }
            //PotentialMoveToTiles[i].ShowAttackTile();

        }

        GD.Print(Name + ": Found Potential move To Tiles: " + PotentialMoveToTiles.Count);

        GD.Print("Targets Name: " + Target.Name);



        MapManager.Instance.RegisterMoveCommand(this, PotentialMoveToTiles[LowestDistIndex], this.CurrentTile);
        GD.Print(Name + ": Registered MoveCommand");

        BeginMovement(PotentialMoveToTiles[LowestDistIndex]);



    }





}
