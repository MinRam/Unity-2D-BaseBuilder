using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;


public class BuildModeController : MonoBehaviour
{
    bool buildModeIsObject = false;
    TileType buildModeTile = TileType.Floor;
    string        buildModeInstalledObject;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetMode_BuildFloor(){
        buildModeIsObject = false;
        buildModeTile = TileType.Floor;
    }

    public void SetMode_Bullldoze() {
        buildModeIsObject = false;
        buildModeTile = TileType.Empty;
    }

    public void SetMode_BuildInstalledObject(string objectType) {
        // Wall is not a tile
        buildModeIsObject = true;
        this.buildModeInstalledObject = objectType;
    }

    public void DoPathFindingTest() {

    }

    public void DoBuild(Tile selectedTile) {
        if (buildModeIsObject) {

            // WorldController.Instance.World.PlaceFurniture(this.buildModeInstalledObject, selectedTile);
            string furnitrueType = buildModeInstalledObject;

            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitrueType,selectedTile) && selectedTile.pendingFurnitureJob == null) {
                Job j = new Job(selectedTile, furnitrueType,  (theJob) => {
                    Debug.Log("complete job, reflesh furniture");
                    WorldController.Instance.world.PlaceFurniture(furnitrueType, theJob.tile);

                    theJob.tile.pendingFurnitureJob = null;
                });

                // FIXME: I don't like having to manually and explicitly set
                // flag that preven conflicts. It's too easy to forget to set/clear them!
                selectedTile.pendingFurnitureJob = j;

                j.RegisterJobCancelCallback( (theJob) => {theJob.tile.pendingFurnitureJob = null;});

                WorldController.Instance.world.jobQueue.Enqueue(j);

            } else {
                Debug.Log("Invalid Furniture at x_" + selectedTile.X + ",y_" + selectedTile.Y);
            }

        } else {
            selectedTile.Type = buildModeTile;
        }
    }
}
