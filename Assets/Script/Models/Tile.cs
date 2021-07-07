using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public enum TileType {Empty, Floor};
public class Tile : IXmlSerializable{
    Action<Tile> cbTileTypeChanged;

    TileType _type = TileType.Empty;
    public TileType Type
    {
        get
        {
            return _type;
        }
        set
        {
            TileType oldType = _type;

            _type = value;

            if (cbTileTypeChanged != null && oldType != _type)
                cbTileTypeChanged(this);
        }
    }

    Inventory inventory;
    public Furniture furniture {get;protected set;}
    public Job pendingFurnitureJob;
    public World World {get; protected set;}
    public int X {get; protected set;}
    public int Y {get; protected set;}

    public float movementCost {
        get {
            if (Type == TileType.Empty) return 0;
            if (furniture == null) return 1;

            return 1 * furniture.movementCost;
        }
    }

    public Tile(World wd, int x, int y) {
        this.World = wd;
        this.X = x;
        this.Y = y;
    }

    public void RedgisterTileTypeChangedCallback(Action<Tile> callback) {
        cbTileTypeChanged += callback;
    }
    public void UnredgisterTileTypeChangedCallback(Action<Tile> callback) {
        cbTileTypeChanged -= callback;
    }

    public bool PlaceFurniture(Furniture obj) {
        if (null == obj && furniture != null) {
            // There will do something to uninstall installObject.
            furniture = null;
            return true;
        }

        if(furniture != null) {
			Debug.LogError("Trying to assign a furniture to a tile that already has one!");
			return false;
		}

        furniture = obj;
        return true;
    }

    public bool IsNeighbour(Tile tile, bool canDiag = false) {

        return
            Math.Abs(this.X - tile.X) + Math.Abs(this.Y - tile.Y) == 1 ||
            (canDiag && (Math.Abs(this.X - tile.X) == 1 && Mathf.Abs(this.Y - tile.Y) == 1));
    }

    public Tile[] GetNeighbours(bool canDiag = false) {
        Tile[] ns = new Tile[canDiag ? 8 : 4];

        ns[0] = World.GetTileAt(X, Y+1);
        ns[1] = World.GetTileAt(X+1, Y);
        ns[2] = World.GetTileAt(X, Y-1);
        ns[3] = World.GetTileAt(X-1, Y);

        if (canDiag) {
            ns[4] = World.GetTileAt(X+1, Y+1);
            ns[5] = World.GetTileAt(X+1, Y-1);
            ns[6] = World.GetTileAt(X-1, Y+1);
            ns[7] = World.GetTileAt(X-1, Y+1);
        }

        return ns;
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
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader) {
        // int x = int.Parse(reader.GetAttribute("X"));
        // int y = int.Parse(reader.GetAttribute("Y"));
        Type = (TileType) int.Parse(reader.GetAttribute("Type"));
    }
}
