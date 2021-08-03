using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public float atmosO2  = 0;
    public float atmosN   = 0;
    public float atmosCO2 = 0;

    List<Tile> tiles;

    public Room() {
        tiles = new List<Tile>();
    }

    public void AssignTIle( Tile t) {
        // This tile already in this room.
        if (tiles.Contains(t)) return;

        // Remove tile from old room first.
        if (t.room != null) {
            t.room.tiles.Remove(t);
        }

        t.room = this;
        tiles.Add(t);
    }

    public void UnAssignAllTile() {
        if (tiles.Count == 0) return;

        Room outsiedRoom = this.tiles[0].World.GetOutsideRoom();

        // The <span>AssignTile</span> method will remove tile from old room. so we 
        // don't need to clear the tiles parameter again.
        while (tiles.Count > 0) {
                outsiedRoom.AssignTIle(tiles[0]);
        }
    }

    public static void DoRoomFloorFill(Furniture sourceFurniture) {
        // <param>sourceFurniture</param> is the piece of furniture that may be
        // spliting two existing rooms, or may be the final
        // enclosing piece to form a new room.
        // Check the NEWS's neighbours of the furniture's tile
        // and do floor fill from them.

        World world = sourceFurniture.tile.World;

        Room oldRoom = sourceFurniture.tile.room;

        // try building a new rooom starting from the neighbours (8).
        foreach (Tile nT in sourceFurniture.tile.GetNeighbours(true)) {
            ActualFloodFill(nT, oldRoom);
        }

        sourceFurniture.tile.room = null;
        oldRoom.tiles.Remove(sourceFurniture.tile);

        /**
         *  If this piece of furniture was added to an existing room
         *  (which should be always be true assuming with consider "outside" to be a big room)
         *  delete that room and assign all tiles within to be "outside" for now.
         */

        // we know all tiles now point to another room so we can just force the old
        // root tiles list  to be blank
        oldRoom.tiles = new List<Tile>();

        if (oldRoom != world.GetOutsideRoom()) {
            if (oldRoom.tiles.Count > 0)
                Debug.LogError("'oldroom' still have some tiles assigned to it. This is  clearly wrong.");

            world.DeleteRoom(oldRoom);
        }
    }

    protected static void ActualFloodFill(Tile tile, Room oldRoom) {
        // we are trying to flood fill off the map. so just return
        // without doing anything.
        if (null == tile) return;

        // This tile is empty space and must remain part of the outside.
        if (tile.Type == TileType.Empty) return;

        // This tile was already assigned to another "new" room, which means
        // that the direction picked isn't isolated. So we can just return
        // without creating a new room.
        if (tile.room != oldRoom)  return;

        // This tile has a enclosure furniture(e.g. Wall, Door) in it, so clearly
        // we can't do a room here.
        if (tile.furniture != null && tile.furniture.roomEnclosure)  return;

        // If we get to this point, then we know that we need to create a new room.
        Room room = new Room();
        Queue<Tile> tiles2Check = new Queue<Tile>();
        tiles2Check.Enqueue(tile);

        while (tiles2Check.Count > 0) {
            Tile checkTile = tiles2Check.Dequeue();

            if (checkTile.room == oldRoom) {
                room.AssignTIle(checkTile);

                Tile[] ns = checkTile.GetNeighbours(true);

                foreach (Tile t2 in ns) {
                    // we have hit the open space (either by being the edge of the map or being an empty tile)
                    // so this "room“ we're building is acturally part of the Outside.
                    // thereforce, we can immediately end the floor fill (which otherwise would take ages.)
                    // And more importantly, we need to delete the new Room and re-assign all tiles to Outside.
                    if (t2 == null || t2.Type == TileType.Empty) {
                        room.UnAssignAllTile();
                        return;
                    }


                    // t2 is not null nor is it an empty tile. so just make sure it
                    // hasn't already been processed and isn't  a 'wall' type tile.
                    if ( t2.room == oldRoom && (t2.furniture == null || t2.furniture.roomEnclosure == false) ) {
                        tiles2Check.Enqueue(t2);
                    }
                }
            }
        }

        tile.World.AddRoom(room);

    }
}
