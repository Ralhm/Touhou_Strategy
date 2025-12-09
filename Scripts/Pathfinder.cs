using Godot;
using System;
using System.Collections.Generic;

public partial class Pathfinder
{

    //Instead of running this twice, once with manhatten and once with euclidean
    //Try running manhatten recursively 
    public Godot.Collections.Array<Tile> FindPath(Tile start, Tile end, bool AllyStatus, bool UseManhatten = true, int iterations = 0, bool IgnoreBlockages = false)
    {
        Godot.Collections.Array<Tile> openList = new Godot.Collections.Array<Tile>();
        Godot.Collections.Array<Tile> closedList = new Godot.Collections.Array<Tile>();



        start.G = 0;

        if (UseManhatten)
        {
            start.CalculateDistanceManhatten(start, end, start);
        }
        else
        {
            start.CalculateDistanceEuclidean(start, end, start);
        }



        openList.Add(start);


        while (openList.Count > 0)
        {
            //GD.Print("Looping...");

            int LowestFIndex = FindLowestFIndex(openList);
            Tile CurrentTile = openList[LowestFIndex];
            //GD.Print("Current Tile: " + CurrentTile.Coordinate);
            if (CurrentTile == end)
            {
                Godot.Collections.Array<Tile> FinishedList = GetFinishedList(start, end);
                //Godot.Collections.Array<Tile> Path1 = FindPath(start.Children[0], end);
                iterations++;
                return FinishedList;

            }


            closedList.Add(CurrentTile);

            Godot.Collections.Array<Tile> ChildTiles = CurrentTile.GetChildren();

            CurrentTile.QuickSort(0, CurrentTile.Children.Count - 1);


            //GD.Print("Returned Children...");
            foreach (Tile child in ChildTiles)
            {

                if (child.IsBlocked)
                {
                    continue;
                }


                if (!IgnoreBlockages)
                {
                    if (child.IsOccupied)
                    {
                        //Allow characters of the same allignment to pass through each other, otherwise, block
                        if (child.CurrentCharacter.IsAlly() != AllyStatus)
                        {
                            continue;
                        }

                    }
                }


                child.G = CurrentTile.G + 1;

                if (UseManhatten)
                {
                    child.CalculateDistanceManhatten(start, end, child);
                }
                else
                {
                    child.CalculateDistanceEuclidean(start, end, child);
                }


                Tile EndTileRecord;
                if (closedList.Contains(child))
                {

                    int i;
                    for (i = 0; i < closedList.Count; i++)
                    {
                        if (closedList[i].Coordinate == child.Coordinate)
                        {
                            break;
                        }
                    }
                    EndTileRecord = closedList[i];

                    if (EndTileRecord.G <= child.G)
                    {
                        continue;
                    }
                    closedList.Remove(EndTileRecord);

                }

                if (openList.Contains(child))
                {
                    int i;
                    for (i = 0; i < openList.Count; i++)
                    {
                        if (openList[i].Coordinate == child.Coordinate)
                        {
                            break;
                        }
                    }
                    EndTileRecord = openList[i];
                    if (EndTileRecord.G <= child.G)
                    {
                        continue;
                    }

                }
                else
                {
                    child.Previous = CurrentTile;
                    openList.Add(child);
                }


            }
            openList.Remove(CurrentTile);
            closedList.Add(CurrentTile);

        }

        GD.Print("-------FAILED TO FIND MATCHING TILE------");


        return GetFinishedList(start, end);
    }






    private Godot.Collections.Array<Tile> GetFinishedList(Tile start, Tile end)
    {
        Godot.Collections.Array<Tile> finishedList = new Godot.Collections.Array<Tile>();

        Tile CurrentTile = end;

        

        while (CurrentTile != start)
        {
            //CurrentTile.ShowAttackTile();
            finishedList.Add(CurrentTile);
            CurrentTile = CurrentTile.Previous;
        }

        //CurrentTile.ShowAttackTile();
        finishedList.Reverse();
        //finishedList[0].ShowAttackTile();
        //GD.Print("Returning Finished List!");
        return finishedList;

    }





    int FindLowestFIndex(Godot.Collections.Array<Tile> tiles)
    {
        float LowestF = tiles[0].F;
        int index = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].F < LowestF)
            {
                LowestF = tiles[i].F;
                index = i;
            }
        }
        return index;
    }



}
