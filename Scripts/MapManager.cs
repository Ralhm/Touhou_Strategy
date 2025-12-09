using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using static Godot.TextEdit;

public enum GameState
{
    SelectingCharacter,
    SelectingMovement,
    SelectingAction,
    SelectingTarget,
    ConfirmingTarget,
    Cutscene
}


public partial class MapManager : TileMapLayer
{

    private static MapManager _instance;

    public static MapManager Instance { get { return _instance; } }

    [Export]
    public PackedScene TileRef = GD.Load<PackedScene>("res://Scenes/Tile.tscn");

    [Export]
    public Control ActionMenu;

    public bool AllyPhase = true;

    [Export]
    public GameState State;

    public List<UnitCommand> CommandList = new List<UnitCommand>();

    [Export]
    public CharacterPreview RightPreview;

    [Export]
    public CharacterPreview LeftPreview;


    public UnitCommand CurrentUnitCommand = new UnitCommand();

    public Character CurrentlySelectedCharacter;

    Godot.Collections.Array<Character> AllyList = new Godot.Collections.Array<Character>();
    Godot.Collections.Array<Character> EnemyList = new Godot.Collections.Array<Character>();
    Godot.Collections.Array<Character> AllEntities = new Godot.Collections.Array<Character>();

    public Godot.Collections.Dictionary<Vector2I, Tile> Map = new Godot.Collections.Dictionary<Vector2I, Tile>();

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
        ActionMenu.Hide();
        State = GameState.SelectingCharacter;
        //GD.Print("Cells Num: " + Cells.Count);

        Godot.Collections.Array<Vector2I>Cells = GetUsedCells();
  
        for (int i = 0; i < Cells.Count; i++)
        {
            
            Tile inst = TileRef.Instantiate() as Tile;   
            
            inst.SetCoordinate(Cells[i]);
            inst.GlobalPosition = ToGlobal(MapToLocal(Cells[i]));
            inst.SetTileData(GetCellTileData(Cells[i]));
            inst.HideTile();


            AddChild(inst);

            Map.Add(Cells[i], inst);
        }


        RightPreview.Hide();
        LeftPreview.Hide();

        SetChildren();

        
        
    }


    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("RightClick")) {
            Undo();
        }

        if (Input.IsActionJustPressed("LeftClick"))
        {
            if (State == GameState.ConfirmingTarget)
            {
                CurrentlySelectedCharacter.BeginAttackTarget();
                HidePreview();

            }
        }


    }

    public void Undo()
    {
        if (State == GameState.SelectingTarget)
        {

            ShowActionMenu();
        }
        else if (State == GameState.SelectingMovement)
        {
            CurrentlySelectedCharacter.OnDeSelect();
        }
        else if (State == GameState.SelectingCharacter)
        {
            UndoAction();

            //UndoLoop(CommandList.Count);

        }
        else if (State == GameState.SelectingAction)
        {
            GD.Print("Undoing Movement: " + CurrentUnitCommand.GetCommands().Count);


            ActionMenu.Hide();
            CurrentlySelectedCharacter.HideAllTiles();
            CurrentUnitCommand.GetCommands()[0].Undo();
            CurrentUnitCommand.ClearCommands();
            CurrentlySelectedCharacter.FindMovementTiles();
                      
        }
        else if (State == GameState.ConfirmingTarget)
        {
            SetGameState(GameState.SelectingTarget);
            HidePreview();
        }
    }


    public void UndoAction()
    {
        GD.Print("Undoing Action: " + CommandList.Count);
        CommandList[CommandList.Count - 1].Undo();
        CommandList[CommandList.Count - 1].ClearCommands();
        CommandList.Remove(CommandList[CommandList.Count - 1]);
    }

    public void UndoLoop(int UndoCount)
    {
        
        for (int i = 0; i < UndoCount; i++)
        {
            UndoAction();
        }
    }


    public void SetChildren()
    {

        foreach (KeyValuePair<Vector2I, Tile> tile in Map)
        {
            tile.Value.SetNeighbours(GetSurroundingCells(tile.Value.Coordinate));
        }

    }

    public Tile GetTile(Vector2I coord)
    {

        if (Map.ContainsKey(coord))
        {
            return Map[coord];
        }

        return null;

        
    }

    public Godot.Collections.Array<Vector2I> GetAdjacentCoords(Vector2I coord)
    {
        return GetSurroundingCells(coord);
    }


    //I suppose I could increment an int here and check,
    //but I believe this method will prevent bugs and edge cases
    public void OnAllyFinishedAction(Character character)
    {
        ActionMenu.Hide();
        SetGameState(GameState.SelectingCharacter);

        CommandList.Add(new UnitCommand(CurrentUnitCommand));
        CurrentUnitCommand.ClearCommands();

        foreach (Character ally in AllyList)
        {
            if (ally.State == CurrentState.NotActed)
            {
                //Break in the event we find an ally that hasn't acted
                
                return;
            }
        }
        BeginEnemyPhase();
    }

    public void RegisterMoveCommand(Character character, Tile EndTile, Tile StartTile)
    {
        CurrentUnitCommand.AddMoveCommand(character, EndTile, StartTile);
    }

    public void RegisterWaitCommand(Character character, Tile EndTile, Tile StartTile)
    {
        CurrentUnitCommand.AddWaitCommand(character);
    }

    public void RegisterAttackCommand(Character Attacker, Character Target)
    {
        CurrentUnitCommand.AddAttackCommand(Attacker, Target);
    }

    public void OnEnemyFinishedAction(Character character)
    {

        CommandList.Add(new UnitCommand(CurrentUnitCommand));
        CurrentUnitCommand.ClearCommands();


        GD.Print("Previous Enemy Action: " + character.Name);


        if (!SelectEnemyToActNext())
        {
            BeginAllyPhase();
        }
        

        
    }

    //Retursn true if we find an enemy to act, false if all living enemies have acted
    public bool SelectEnemyToActNext()
    {
        foreach (Character enemy in EnemyList)
        {
            if (enemy.State == CurrentState.Dead)
            {
                continue;
            }

            if (enemy.State == CurrentState.NotActed)
            {
                //Break in the event we find an enemy that hasn't acted
                enemy.SelectTarget();
                return true;
            }
        }

        return false;
    }


    public void OnTileSelect(Tile tile)
    {

        if (InAllyPhase())
        {
            //
            if (State == GameState.SelectingCharacter && tile.IsOccupied)
            {
                //Leave this specific call as ambiguous, let the character decide what to do with this function if it's an ally or enemy
                SetCurrentCharacter(tile.CurrentCharacter);
                CurrentlySelectedCharacter.OnSelect();
            }
            else if (State == GameState.SelectingMovement) //Only allow movement to 
            {
                //GD.Print("Beginning Movement!");
                CurrentlySelectedCharacter.BeginMovement(tile);
                
            }
            else if (State == GameState.SelectingTarget)
            {
                if (tile.IsOccupied)
                {
                    if (tile.CurrentCharacter.IsAlly() != CurrentlySelectedCharacter.IsAlly())
                    {
                        CurrentlySelectedCharacter.SetTarget(tile.CurrentCharacter);
                        State = GameState.ConfirmingTarget;
                        ShowPreview();
                        
                    }
                }
            }

        }


    }

    public void ShowPreview()
    {
        RightPreview.Show();
        RightPreview.Enter(CurrentlySelectedCharacter, CurrentlySelectedCharacter.Target);

        LeftPreview.Show();
        LeftPreview.Enter(CurrentlySelectedCharacter.Target, CurrentlySelectedCharacter);
    }

    public void HidePreview()
    {
        RightPreview.BeginEaseOut();

        LeftPreview.BeginEaseOut();
    }

    public void ConfirmAttack()
    {
        CurrentlySelectedCharacter.BeginAttackTarget();
    }




    public void BeginEnemyPhase()
    {
        GD.Print("----Beginning Enemy Phase----");

        foreach (Character enemy in EnemyList)
        {
            if (enemy.State != CurrentState.Dead)
            {
                enemy.SetState(CurrentState.NotActed);
            }


        }


        AllyPhase = false;

        SelectEnemyToActNext();
    }

    public void BeginAllyPhase()
    {
        AllyPhase = true;

        foreach(Character ally in AllyList)
        {
            if (ally.State != CurrentState.Dead)
            {
                ally.SetState(CurrentState.NotActed);
            }

            
        }

        SetGameState(GameState.SelectingCharacter);
    }

    public void AddToAllies(Character ally)
    {
        AllyList.Add(ally);
    }


    public void RemoveFromAllies(Character ally)
    {
        AllyList.Remove(ally);
    }

    public void AddToEnemies(Character enemy)
    {
        EnemyList.Add(enemy);
    }


    public void RemoveFromEnemies(Character enemy)
    {
        EnemyList.Remove(enemy);
    }


    public GameState GetGameState()
    {
        return State;
    }

    public void EnterSelectTargetMode()
    {
        SetGameState(GameState.SelectingTarget);
        ActionMenu.Hide();

    }

    public void ShowActionMenu()
    {
        SetGameState(GameState.SelectingAction);
        ActionMenu.Show();

    }


    public void WaitAction()
    {
        //WaitCommand wait


        CurrentlySelectedCharacter.Wait();
        
    }


    public void SetGameState(GameState NewState)
    {
        State = NewState;
    }

    public bool InAllyPhase()
    {
        return AllyPhase;
    }


    public void SetCurrentCharacter(Character character)
    {
        CurrentlySelectedCharacter = character;
    }

    public void SetCursor(Tile tile)
    {
        if (InAllyPhase())
        {
            if (State == GameState.SelectingCharacter)
            {
                Cursor.Instance.SetFocusedOnTile(tile);
            }
            else if (State == GameState.SelectingMovement)
            {
                GD.Print("Selecting Movement!");
                if (CurrentlySelectedCharacter.IsTileInMoveRange(tile))
                {
                    Cursor.Instance.SetFocusedOnTile(tile);
                }
            }
            else if (State == GameState.SelectingTarget)
            {
                GD.Print("Selecting Target!");
                if (CurrentlySelectedCharacter.IsTileInAttackRange(tile))
                {
                    Cursor.Instance.SetFocusedOnTile(tile);
                }
            }
        }
    }
}
