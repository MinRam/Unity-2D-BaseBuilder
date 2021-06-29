using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public float X {
        get{
            return Mathf.Lerp(
                WorldController.Instance.GetTilePositionAtWorldCoord(currTile).x ,
                WorldController.Instance.GetTilePositionAtWorldCoord(nextTile).x ,
                movementPercentage);
        }
    }
    public float Y {
        get{
            return Mathf.Lerp(
                WorldController.Instance.GetTilePositionAtWorldCoord(currTile).y ,
                WorldController.Instance.GetTilePositionAtWorldCoord(nextTile).y ,
                movementPercentage);
        }
    }

    public Tile currTile {get; protected set;}
    Tile destTile;            // If we aren't moving, then destTile = currTile.
    Tile nextTile;            // The next tile in the pathfinding sequence.
    Path_AStar pathAStar;


    // the distance that could be touch.
    float touchDistance;
    bool isTouchDistance(Tile target, Tile current) {
        return Math.Abs(target.X - current.X) + Math.Abs(target.Y - current.Y) <= touchDistance;
    }

    float movementPercentage; // Goes from 0 to 1 as we move from currTile to desTile.

    float speed = 4f;         // Tiles per second

    Action<Character> cbCharacterChanged;

    Job myJob;

    public Character(Tile tile) {
        // Debug.Log("Create character at x:" + tile.X + ",y:" + tile.Y);
        currTile = destTile = nextTile = tile;
        touchDistance = 2f;
    }

    void Update_DoJob(float deltaTime) {
        if (myJob == null) {
            myJob = currTile.World.jobQueue.Dequeue();
            if (myJob != null) {
                destTile = myJob.tile;
                myJob.RegisterJobCompleteCallback(OnJobEnded);
                myJob.RegisterJobCancelCallback(OnJobEnded);
            }
        }

        if (myJob != null && isTouchDistance(myJob.tile, currTile)) {
            myJob.DoWork(deltaTime);
        }
    }

    void Update_TrapAndMove2AvailableTile(float deltaTime) {
        if (currTile.movementCost <= 0) {
            Tile[] neighbours =  currTile.GetNeighbours(false);

            foreach(Tile neighbour in neighbours) {
                if (neighbour != null &&  neighbour.movementCost > 0) {
                    destTile = nextTile = neighbour;
                    return;
                }
            }

        }
    }

    void Update_HandleMovementForJob(float deltaTime) {
        if (currTile == destTile) return;

        if (nextTile == null || nextTile == currTile) {
            // Get the next tile from the pathfinder.
            if (pathAStar == null || pathAStar.Length() == 0) {
                // Generate a path to our destination.
                pathAStar = new Path_AStar(currTile.World, currTile, destTile, isTouchDistance);
                if (pathAStar.Length() == 0) {
                    // Debug.LogError("The Path_AStar returned no path to destination.");
                    // FIXME: Job should maybe be re-enqueue instead?
                    OnJobAbandon();
                    pathAStar = null;
                    return;
                }
            }

            // Grab the next waypoint from the pathing system.
            nextTile = pathAStar.Dequeue();
        }



        Vector3 curTileVec = WorldController.Instance.GetTilePositionAtWorldCoord(currTile);
        Vector3 desTileVec = WorldController.Instance.GetTilePositionAtWorldCoord(destTile);

        float distToTravel = Vector3.Distance(curTileVec, desTileVec);
        float distThisFrame = speed * deltaTime;

        float perThisFrame = distThisFrame / distToTravel;

        movementPercentage += perThisFrame;

        if (movementPercentage  >= 1) {

            // TOOD: Get the next tile from the pathfinding system.
            //       If there are no more tiles, then we have TRULY
            //       reached our destination.


            // we have reached our  destination
            currTile = nextTile;

            movementPercentage = 0;
            // FIXME? Do we actually want to retain any overshot movement;
        }

    }

    public void Update(float deltaTime) {
        Update_DoJob(deltaTime);

        Update_HandleMovementForJob(deltaTime);

        Update_TrapAndMove2AvailableTile(deltaTime);

        if (cbCharacterChanged != null) {
            cbCharacterChanged(this);
        }
    }

    public void SetDestination(Tile tile) {
        if (!currTile.IsNeighbour(tile, true)) {
            Debug.Log("Our destination tile isn't actually our neighbour");
        }

        destTile = tile;
    }

    public void RegisterOnChangedCallBack(Action<Character> callback) {
        cbCharacterChanged += callback;
    }

    public void UnRegisterOnChangedCallBack(Action<Character> callback) {
        cbCharacterChanged -= callback;
    }

    void OnJobAbandon() {
        nextTile = destTile = currTile;

        pathAStar = null;

        if (myJob != null) {
            myJob.UnRegisterJobCompleteCallback(OnJobEnded);
            myJob.UnRegisterJobCancelCallback(OnJobEnded);
        }

        currTile.World.jobQueue.Enqueue(myJob);

        myJob = null;
    }

    void OnJobEnded(Job j) {
        if (j != myJob) {
            Debug.Log("Character being told about job that isn't his. \n Job:" + j + "\n myJob:" + myJob);
            return;
        }

        myJob = null;
    }
}
