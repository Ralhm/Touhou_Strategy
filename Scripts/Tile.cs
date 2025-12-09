using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class Tile : Area2D
{



    public float G = 0, H = 0;

    public float F = 0;


    //Let this represent how far the tile is from a specific tile, used for AI Decision making
    public int DistanceToTile;


    float NumTilesFromStart = 0;

    public Tile Previous;

    [Export]
    public bool IsOccupied;

    [Export]
    public bool IsBlocked;

    [Export]
    public Character CurrentCharacter;

    public TileData Data;

    [Export]
    public Vector2I Coordinate;


    [Export]
    AnimatedSprite2D Highlight;

    [Export]
    public Godot.Collections.Array<Tile> Children;



    public override void _Ready()
    {
        base._Ready();
        Highlight = GetNode<AnimatedSprite2D>("Sprite");

        MouseEntered += SetCursor;
        //MouseExited += HideTile;

        
        
    }


    public void SetNeighbours(Godot.Collections.Array<Vector2I> children)
    {
        for (int i = 0; i < children.Count; i++)
        {



            Tile ChildTile = MapManager.Instance.GetTile(children[i]);

            if (ChildTile != null)
            {
                //ChildTile.ShowTile();
                Children.Add(ChildTile);
            }




        }      

    }

    public Godot.Collections.Array<Tile> GetChildren()
    {

        if (Children.Count <= 0)
        {
            GD.Print("BAD SORT");
            SetNeighbours(MapManager.Instance.GetSurroundingCells(Coordinate));
        }

        
  

        return Children;
    }

    public void CalculateDistanceEuclidean(Tile Start, Tile End, Tile ChildTile)
    {
        //G = GetDistanceToNodeManhatten(Start, ChildTile);
        H = GetDistanceToNodeEucliden(End, ChildTile);
        F = G + H;
    }

    public void CalculateDistanceManhatten(Tile Start, Tile End, Tile ChildTile)
    {
        //G = GetDistanceToNodeManhatten(Start, ChildTile);
        H = GetDistanceToNodeManhatten(End, ChildTile);
        F = G + H;
    }


    public float GetDistanceToNodeEucliden(Tile Start, Tile ChildTile)
    {
        return Mathf.Sqrt(Mathf.Pow(Start.Coordinate.X - ChildTile.Coordinate.X, 2) + Mathf.Pow(Start.Coordinate.Y - ChildTile.Coordinate.Y, 2));
  
    }

    public int GetDistanceToNodeManhatten(Tile Start, Tile ChildTile)
    {     
        return Mathf.Abs(Start.Coordinate.X - ChildTile.Coordinate.X) + Mathf.Abs(Start.Coordinate.Y - ChildTile.Coordinate.Y);
    }

    public void SetTileData(TileData data)
    {
        Data = data;
    }



    public void SetCoordinate(Vector2I coord)
    {
        Coordinate = coord;
    }


    public bool GetIsOccupied()
    {
        return IsOccupied;
    }

    public void SetIsOccupied(Character character)
    {
        IsOccupied = true;
        CurrentCharacter = character;
    }

    public void ClearTile()
    {
        IsOccupied = false;
        CurrentCharacter = null;
    }


    public void SetCursor()
    {
        MapManager.Instance.SetCursor(this);        
    }

    public void ShowTile()
    {

        Modulate = new Color(1, 1, 1, 1);

    }

    public void HideTile()
    {
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);

    }

    public void ShowAttackTile()
    {
        Modulate = new Color(1, 0, 0, 1);


    }


    public int Partition(int Low, int High)
    {
        float Pivot = Children[High].F;
        int i = Low - 1;
        Tile temp;

        for (int j = Low; j <= High - 1; j++)
        {
            if (Children[j].F > Pivot)
            {
                i++;

                // swap
                temp = Children[i];
                Children[i] = Children[j];
                Children[j] = temp;

            }
        }


        temp = Children[i + 1];
        Children[i + 1] = Children[High];
        Children[High] = temp;

        return i + 1;
    }
    
    public void QuickSort(int Low, int High)
    {
        if (Low < High)
        {
            return;
        }

        int pi = Partition(Low, High);

        //GD.Print("QuickSorting...");
        QuickSort(Low, pi - 1);
        QuickSort(pi + 1, High);
    }
}
