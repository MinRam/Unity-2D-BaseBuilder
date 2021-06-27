using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World {

    Tile[,] tiles;
    List<Character> characters;
    public Path_TileGraph tileGraph;

    Dictionary<string, Furniture> furniturePrototypes;

    Action<Character> cbCharacterCreated;
    Action<Furniture> cbFurnitureCreated;
    Action<Tile> cbTileChanged;

    public JobQueue jobQueue;

    public int Width { get; protected set;}

    public int Height { get; protected set;}

    public World(int width = 50, int height = 50) {
        jobQueue = new JobQueue();

        this.Width = width;
        this.Height = height;
        tiles = new Tile[width, height];


        for (int x = 0; x < width; ++x) {
            for (int y = 0; y < height; ++y) {
                tiles[x,y] = new Tile(this, x, y);
                tiles[x,y].RedgisterTileTypeChangedCallback(OnTileChanged);
            }
        }

        CreateFurniturePrototypes();

        characters = new List<Character>();
    }

    public void Update(float deltaTime) {
        foreach(Character c in characters) {
            c.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile t) {
        Character c = new Character(t);

        characters.Add(c);

        if (cbCharacterCreated != null) {
            cbCharacterCreated(c);
        }

        return c;
    }

    void CreateFurniturePrototypes() {
        if ( null == furniturePrototypes ) {
            furniturePrototypes = new Dictionary<string, Furniture>();
        }

        CreateFurniturePrototype("Wall",
                        Furniture.CreatePrototype("Wall",
                                                        0,   // Impassable
                                                        1,   // Width
                                                        1,   // Height
                                                        true // Links to neighbours and "sort of" becomes part of a large object.
                                                    ));
    }

    Furniture CreateFurniturePrototype(string objectName, Furniture installedObject) {
        // Debug.Log("Preload InstalledObject:" + objectName);
        furniturePrototypes.Add("Wall", installedObject);
        return installedObject;
    }

    public void PlaceFurniture(string objectType, Tile t) {
        // TOD: This function assumes 1x1 tiles -- change this later!

        if (!furniturePrototypes.ContainsKey(objectType)) {
            Debug.LogError("FurnituretPrototypes doesn't contain a proto for key: " + objectType);
            return;
        }

        Furniture obj = Furniture.PlaceInstance(furniturePrototypes[objectType], t);

        if (null == obj) return;

        if (cbFurnitureCreated != null) {
            cbFurnitureCreated(obj);
            InvalidateTileGraph(t);
        }
    }

    public void RegisterFurnitureCreated(Action<Furniture> callbackfunc) {
        this.cbFurnitureCreated += callbackfunc;
    }

    public void UnRegisterFurnitureCreated(Action<Furniture> callbackfunc) {
        this.cbFurnitureCreated -= callbackfunc;
    }

    public void RegisterCharacterCreated(Action<Character> callbackfunc) {
        this.cbCharacterCreated += callbackfunc;
    }

    public void UnRegisterCharacterCreated(Action<Character> callbackfunc) {
        this.cbCharacterCreated -= callbackfunc;
    }

    public void RegisterTileChanged(Action<Tile> callbackfunc) {
        this.cbTileChanged += callbackfunc;
    }

    public void UnRegisterTileChanged(Action<Tile> callbackfunc) {
        this.cbTileChanged -= callbackfunc;
    }

    void OnTileChanged(Tile t) {
        if (cbTileChanged == null) return;
        cbTileChanged(t);

        InvalidateTileGraph(t);
    }

    public void InvalidateTileGraph(Tile t) {
        if (tileGraph != null)
            tileGraph.Update_PathTileGraph(t);
    }

    public Tile GetTileAt(int x, int y) {
        if (x >= Width || x < 0 || y >= Height || y < 0) {
            // Debug.LogError("Tile(" + x + "," + y + ") is out of range.");
            return null;
        }

        return tiles[x, y];
    }

    public void RandomizeTiles()
    {
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                if (UnityEngine.Random.Range(0,2) == 0) {
                    tiles[x,y].Type = Tile.TileType.Empty;
                }
                else
                {
                    tiles[x,y].Type = Tile.TileType.Floor;
                }
            }
        }
    }

    public bool IsFurniturePlacementValid(string furnitrueType, Tile t) {
        return furniturePrototypes[furnitrueType].IsValidPosition(t);
    }

    public Furniture GetFurniturePrototype(string objectType) {
        if (!furniturePrototypes.ContainsKey(objectType)) {
            return null;
        }
        return furniturePrototypes[objectType];
    }
}
