using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class World : IXmlSerializable {

    Tile[,] tiles;
    public List<Character> characters;
    public List<Furniture> furnitures;
    public Path_TileGraph tileGraph;

    Dictionary<string, Furniture> furniturePrototypes;

    Action<Character> cbCharacterCreated;
    Action<Furniture> cbFurnitureCreated;
    Action<Tile> cbTileChanged;

    public JobQueue jobQueue;

    public int Width { get; protected set;}

    public int Height { get; protected set;}

    public World(int width, int height) {
        SetupWorld(width,height);

        CreateCharacter(GetTileAt(Width/2  , Height/2 ));
    }

    protected void SetupWorld(int width, int height) {
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
        furnitures = new List<Furniture>();
    }

    public void Update(float deltaTime) {
        foreach(Character c in characters) {
            c.Update(deltaTime);
        }

        foreach(Furniture f in furnitures) {
            f.Update(deltaTime);
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
                        new Furniture("Wall",
                                    0,   // Impassable
                                    1,   // Width
                                    1,   // Height
                                    true // Links to neighbours and "sort of" becomes part of a large object.
                        ));

        CreateFurniturePrototype("Door",
                        new Furniture("Door",
                                    1,   // Impassable
                                    1,   // Width
                                    1,   // Height
                                    false // Links to neighbours and "sort of" becomes part of a large object.
                        ));

        // What if the object behaviours were scriptale? and therefore were part of the text file
        // we are reading in now?
        furniturePrototypes["Wall"].getSpriteName = FurnitureActions.Default_GetSpriteName;
        furniturePrototypes["Door"].furnParams["openness"] = 0;
        furniturePrototypes["Door"].furnParams["is_opening"] = 0;
        furniturePrototypes["Door"].updateActions += FurnitureActions.Door_UpdateAction;
        furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;
        furniturePrototypes["Door"].getSpriteName = FurnitureActions.Door_GetSpriteName;
    }

    Furniture CreateFurniturePrototype(string objectName, Furniture installedObject) {
        // Debug.Log("Preload InstalledObject:" + objectName);
        furniturePrototypes.Add(objectName, installedObject);
        return installedObject;
    }

    public Furniture PlaceFurniture(string objectType, Tile t) {
        // TOD: This function assumes 1x1 tiles -- change this later!

        if (!furniturePrototypes.ContainsKey(objectType)) {
            Debug.LogError("FurnituretPrototypes doesn't contain a proto for key: " + objectType);
            return null;
        }

        Furniture obj = Furniture.PlaceInstance(furniturePrototypes[objectType], t);

        furnitures.Add(obj);

        if (null == obj) return null;

        if (cbFurnitureCreated != null) {
            cbFurnitureCreated(obj);
            InvalidateTileGraph(t);
        }

        return obj;
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
                    tiles[x,y].Type = TileType.Empty;
                }
                else
                {
                    tiles[x,y].Type = TileType.Floor;
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

    /////////////////////////////////////////
    ///
    ///    Saving & Loading
    ///
    /////////////////////////////////////////

    public World() {}

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        // save info here.
        Debug.Log("Save xml");

        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; ++x) {
            for (int y = 0; y < Height; ++y) {
                if (tiles[x,y].Type != TileType.Empty) {
                    writer.WriteStartElement("Tile");
                    tiles[x,y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Furnitures");
        foreach (Furniture furniture in furnitures) {
            writer.WriteStartElement("Furniture");
            furniture.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

         writer.WriteStartElement("Characters");
        foreach (Character character in characters) {
            writer.WriteStartElement("Character");
            character.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

    }

    public void ReadXml(XmlReader reader) {
        int width = int.Parse(reader.GetAttribute("Width"));
        int height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(width,height);

        while (reader.Read()) {
            switch (reader.Name) {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
                default:
                    Debug.Log("Unknow Name:" + reader.Name);
                    break;
            }
        }
        reader.MoveToElement();
    }

    void ReadXml_Tiles(XmlReader reader) {
        if ( reader.ReadToDescendant("Tile")) {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                tiles[x, y].ReadXml(reader);
            } while (reader.ReadToNextSibling("Tile"));
        }
    }

    void ReadXml_Furnitures(XmlReader reader) {
        if ( reader.ReadToDescendant("Furniture")) {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("objectType"), tiles[x,y]);

                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furniture"));
        }
    }

    void ReadXml_Characters(XmlReader reader) {
        if ( reader.ReadToDescendant("Character")) {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Character c = CreateCharacter(tiles[x, y]);

                c.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));
        }
    }
}
