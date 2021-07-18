using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : IXmlSerializable
{
    public Dictionary<string, float> furnParams;
    public Action<Furniture, float> updateActions;

    public Func<Furniture, string> getSpriteName;

    public Func<Furniture, ENTERABILITY> IsEnterable;

    public void Update(float deltaTime) {
        if (updateActions != null) {
            updateActions(this, deltaTime);
        }
    }

    // This represents the BASE ile of the object , but in practice, large objects may
    // actually occupy multile tiles.
    public Tile tile {get; protected set;}

    public string objectType {get; protected set;}

    public Action<Furniture> cbOnChanged;

    Func<Tile,bool> funcPositionValidation;

    // This is a multipler. So a value of "2" here , means you move twice as slowly (i.e at half speed)
    // Tile types and other enviroment effects may be combine.
    // For example, a "rough"  tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
    // would have a total movement cost of (2 + 3 + 3 = 8), so you'd move through this tile at 1/8th normal speed.
    // SPECTIAL: if movementCost = 0, then this tile is impassible. (e.g. a wall).
    public float movementCost {get; protected set;}

    int width = 1;
    int height = 1;

    public bool linksToNeighbour {get; protected set;}

    protected Furniture() {
        furnParams = new Dictionary<string, float>();
    }

    public Furniture(Furniture other) {
        this.objectType = other.objectType;
        this.movementCost = other.movementCost;
        this.width = other.width;
        this.height = other.height;
        this.linksToNeighbour = other.linksToNeighbour;

        this.furnParams = new Dictionary<string, float>(other.furnParams);
        if (other.updateActions != null) {
            this.updateActions = (Action<Furniture,float>) other.updateActions.Clone();
        }

        this.IsEnterable = other.IsEnterable;
        this.getSpriteName = other.getSpriteName;
    }

    virtual public Furniture Clone() {
        return new Furniture(this);
    }

    public Furniture(string objectType, float movementCost, int width = 1, int height = 1, bool linksToNeighbour = false) {
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.width = width;
        this.height = height;
        this.linksToNeighbour = linksToNeighbour;
        this.funcPositionValidation = this.__IsValidPosition;
        this.furnParams = new Dictionary<string, float>();
    }

    public static Furniture PlaceInstance(Furniture proto, Tile tile) {
        if (!proto.funcPositionValidation(tile)) {
            return null;
        }

        Furniture obj = proto.Clone();

        obj.tile = tile;

        if ( tile.PlaceFurniture(obj) == false ) {
            Debug.Log("Failed to Place Furniture at x:" + obj.tile.X + ",y:" + obj.tile.Y);
            return null;
        }

        if (obj.linksToNeighbour) {
            // This type of furniture links itself to its nighbours.
            // so we should inform our neigbours that they have a new
            // buddy. Just trigger their onChangedCallBack.
            int x = tile.X;
            int y = tile.Y;
            Tile t;
            for (int index_x = x - 1; index_x <= x + 1; ++index_x) {
                for (int index_y = y - 1; index_y <= y+ 1; ++ index_y) {
                    t = tile.World.GetTileAt(index_x, index_y);
                    if (t != null && t != tile && t.furniture != null && t.furniture.objectType == proto.objectType && t.furniture.cbOnChanged != null) {
                        t.furniture.cbOnChanged(t.furniture);
                    }
                }
            }
        }

        return obj;
    }

    public int GetSpriteIndex(byte mask) {
        switch (mask)
            {
                case 0: return 0;
                case 1:
                case 4:
                case 16:
                case 64: return 1;
                case 5:
                case 20:
                case 80:
                case 65: return 2;
                case 7:
                case 28:
                case 112:
                case 193: return 3;
                case 17:
                case 68: return 4;
                case 21:
                case 84:
                case 81:
                case 69: return 5;
                case 23:
                case 92:
                case 113:
                case 197: return 6;
                case 29:
                case 116:
                case 209:
                case 71: return 7;
                case 31:
                case 124:
                case 241:
                case 199: return 8;
                case 85: return 9;
                case 87:
                case 93:
                case 117:
                case 213: return 10;
                case 95:
                case 125:
                case 245:
                case 215: return 11;
                case 119:
                case 221: return 12;
                case 127:
                case 253:
                case 247:
                case 223: return 13;
                case 255: return 14;
            }
            return -1;
    }

    public int GetTransform(byte mask) {
    switch (mask)
        {
            case 4:
            case 20:
            case 28:
            case 68:
            case 84:
            case 92:
            case 116:
            case 124:
            case 93:
            case 125:
            case 221:
            case 253:
                return 1;
            case 16:
            case 80:
            case 112:
            case 81:
            case 113:
            case 209:
            case 241:
            case 117:
            case 245:
            case 247:
                return 2;
            case 64:
            case 65:
            case 193:
            case 69:
            case 197:
            case 71:
            case 199:
            case 213:
            case 215:
            case 223:
                return 3;
            default:
                return 0;
        }
    }

    public void RegisterOnChangedCallBack(Action<Furniture> callbackFunc) {
        cbOnChanged += callbackFunc;
    }

    public void UnRegisterOnChangedCallBack(Action<Furniture> callbackFunc) {
        cbOnChanged -= callbackFunc;
    }

    public bool IsValidPosition(Tile t) {
        return funcPositionValidation(t);
    }

    public bool __IsValidPosition(Tile t) {
        // Make sure tile is Floor
        // Make sure tile doesn't already have furniture
        if (t.Type != TileType.Floor) {
            return false;
        }

        if (t.furniture != null) {
            return false;
        }

        return true;
    }

    public bool __IsValidPosition_Door(Tile t) {
        if (!funcPositionValidation(t)) {
            return false;
        }
        return true;
    }

    //////////////////////////////////
    ///
    ///   Saving & Loading
    ///
    /////////////////////////////////

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("objectType", objectType.ToString());
        // writer.WriteAttributeString("movementCost", movementCost.ToString());

        // add Furniture Parameters
        foreach (string key in furnParams.Keys) {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", key);
            writer.WriteAttributeString("value", furnParams[key].ToString());
            writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader reader) {
        // X, Y and objectType have already been set and we should already
        // be assigned to a tile . so just read extra data.
        // movementCost = int.Parse(reader.GetAttribute("movementCost"));

        if (reader.ReadToDescendant("Param")) {
            do {
                string key = reader.GetAttribute("name");
                float value = float.Parse( reader.GetAttribute("value"));
                furnParams[key] = value;
            } while ( reader.ReadToNextSibling("Param") );
        }
    }
}
