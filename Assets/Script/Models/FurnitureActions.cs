using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureActions
{
    public static void Door_UpdateAction(Furniture furniture, float deltaTime) {
        if (furniture.furnParams["is_opening"] >= 1) {
            furniture.furnParams["openness"] += deltaTime;
            if (furniture.furnParams["openness"] >= 1)
                furniture.furnParams["is_opening"] = 0;
        }
        else {
            furniture.furnParams["openness"] -= deltaTime;
        }

        furniture.furnParams["openness"] = Mathf.Clamp01(furniture.furnParams["openness"]);

        if (furniture.cbOnChanged != null)
            furniture.cbOnChanged(furniture);
    }

    public static ENTERABILITY Door_IsEnterable(Furniture furniture) {
        furniture.furnParams["is_opening"] = 1;

        if (furniture.furnParams["openness"] >= 1) {
            return ENTERABILITY.Yes;
        }

        return ENTERABILITY.Soon;
    }

    public static string Default_GetSpriteName(Furniture furn) {
        if (furn.linksToNeighbour == false) {
            return furn.objectType + "_0_0";
        }

        StringBuilder spriteName = new StringBuilder(furn.objectType);
        spriteName.Append("_");

        Func<int,int, Furniture, bool> TileValue = (int index_x,int index_y, Furniture func) => {
            Tile t = WorldController.Instance.world.GetTileAt(index_x, index_y);
            return t != null && t.furniture != null && t.furniture.objectType.Equals(func.objectType);
        };

        int x = furn.tile.X;
        int y = furn.tile.Y;

        int mask =  TileValue(x    , y + 1, furn) ? 1 : 0;
            mask += TileValue(x + 1, y + 1, furn) ? 2 : 0;
            mask += TileValue(x + 1, y    , furn) ? 4 : 0;
            mask += TileValue(x + 1, y - 1, furn) ? 8 : 0;
            mask += TileValue(x    , y - 1, furn) ? 16 : 0;
            mask += TileValue(x - 1, y - 1, furn) ? 32 : 0;
            mask += TileValue(x - 1, y    , furn) ? 64 : 0;
            mask += TileValue(x - 1, y + 1, furn) ? 128 : 0;

        byte original = (byte) mask;
        if ((original | 254) < 255) {mask = mask & 125;}
        if ((original | 251) < 255) {mask = mask & 245;}
        if ((original | 239) < 255) {mask = mask & 215;}
        if ((original | 191) < 255) {mask = mask & 95;}

        int index = furn.GetSpriteIndex((byte) mask);
        if (index >= 0 ) {
            spriteName.Append(index);
            spriteName.Append("_");
            spriteName.Append(furn.GetTransform((byte)mask).ToString());
        }

        return spriteName.ToString();
    }
    public static string Door_GetSpriteName(Furniture furn) {
        float openness = furn.furnParams["openness"];

        if (openness < 0.1f) return "Door_0_0";
        if (openness < 0.45f) return "Door_0_1";
        if (openness < 0.65f) return "Door_0_2";
        if (openness < 0.9f) return "Door_0_3";
        return "Door_0_4";
    }
}
