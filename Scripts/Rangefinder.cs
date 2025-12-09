using Godot;
using System;
using System.Collections.Generic;

public partial class Rangefinder
{


    //Find ALL tiles surrounding a target, no other checks
    public Godot.Collections.Array<Tile> GetTilesInRange(Tile currentTile, int range)
    {
        var inRangeTiles = new Godot.Collections.Array<Tile>();
        int stepCount = 0;

        inRangeTiles.Add(currentTile);

        var tileForPreviousStep = new Godot.Collections.Array<Tile>();
        tileForPreviousStep.Add(currentTile);

        while (stepCount < range)
        {
            var surroundingTiles = new Godot.Collections.Array<Tile>();

            foreach (var tile in tileForPreviousStep)
            {
                surroundingTiles.AddRange(tile.GetChildren());
            }

            foreach (Tile tile in surroundingTiles)
            {
                if (!inRangeTiles.Contains(tile))
                {
                    inRangeTiles.Add(tile);
                }

                if (!tileForPreviousStep.Contains(tile))
                {
                    tileForPreviousStep.Add(tile);
                }
            }

            stepCount++;

        }

        return inRangeTiles;

    }


    //Get tiles within range of a target, accounts for movement range of user
    public Godot.Collections.Array<Tile> GetTilesWithinTargetRange(Tile currentTile, Character character, int AttackRange)
    {
        var inRangeTiles = new Godot.Collections.Array<Tile>();
        int stepCount = 0;

        //inRangeTiles.Add(currentTile);

        var tileForPreviousStep = new Godot.Collections.Array<Tile>();
        tileForPreviousStep.Add(currentTile);

        while (stepCount < AttackRange)
        {
            var surroundingTiles = new Godot.Collections.Array<Tile>();

            foreach (var tile in tileForPreviousStep)
            {
                surroundingTiles.AddRange(tile.GetChildren());
            }

            foreach (Tile tile in surroundingTiles)
            {
 


                if (!inRangeTiles.Contains(tile))
                {
                    if (character.IsTileInMoveRange(tile))
                    {
                        //Add tile if it is occuppied by the searching character
                        if (tile.IsOccupied && tile.CurrentCharacter == character)
                        {
                            inRangeTiles.Add(tile);
                        }

                        if (!tile.IsOccupied && !tile.IsBlocked)
                        {
                            inRangeTiles.Add(tile);
                        }
                        
                    }

                    
                }

                if (!tileForPreviousStep.Contains(tile))
                {
                    tileForPreviousStep.Add(tile);
                }
            }

            stepCount++;

        }

        return inRangeTiles;

    }




    //Find tiles that character can move to from starting point, accounts for blockages
    public Godot.Collections.Array<Tile> GetMovementTilesInRange(Tile currentTile, Character character, int range)
    {
        var inRangeTiles = new Godot.Collections.Array<Tile>();
        int stepCount = 0;

        inRangeTiles.Add(currentTile);

        var tileForPreviousStep = new Godot.Collections.Array<Tile>();
        tileForPreviousStep.Add(currentTile);

        while (stepCount < range)
        {
            var surroundingTiles = new Godot.Collections.Array<Tile>();

            foreach (var tile in tileForPreviousStep)
            {
                surroundingTiles.AddRange(tile.GetChildren());
            }

            foreach (Tile tile in surroundingTiles)
            {

                if (tile.IsBlocked)
                {
                    continue;
                }

                //Continue if blocked by enemy
                if (tile.IsOccupied && tile.CurrentCharacter.IsAlly() != character.IsAlly())
                {
                    continue;
                } 


                if (!inRangeTiles.Contains(tile))
                {

                    if (!tile.IsOccupied)
                    {
                        inRangeTiles.Add(tile);
                    }

                        

                }

                if (!tileForPreviousStep.Contains(tile))
                {

                    tileForPreviousStep.Add(tile);

                }
            }

            stepCount++;

        }




        return inRangeTiles;

    }

    //Find tiles that user can target, accounts for movement range
    public Godot.Collections.Array<Tile> GetTilesInAttackRange(Godot.Collections.Array<Tile> AttackTiles, Character character, int range)
    {
        
        int stepCount = 0;

        //THIS IS RESULTING IN BLOCKED TILES BEING ADDED
        var tileForPreviousStep = AttackTiles;

  
        while (stepCount < range)
        {
            var surroundingTiles = new Godot.Collections.Array<Tile>();

            foreach (var tile in tileForPreviousStep)
            {
                surroundingTiles.AddRange(tile.GetChildren());
            }

            foreach (Tile tile in surroundingTiles)
            {



                if (!AttackTiles.Contains(tile))
                {
                    AttackTiles.Add(tile);

      

                }

                if (!tileForPreviousStep.Contains(tile))
                {

                    tileForPreviousStep.Add(tile);


                }

            }

            stepCount++;

        }



        return AttackTiles;

    }


}
