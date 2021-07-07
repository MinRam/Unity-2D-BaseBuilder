using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;

public class FurnitureSpriteController : MonoBehaviour
{
    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    Dictionary<string, Sprite> furnitureSpriteMap;

    World world {
        get { return WorldController.Instance.world;}
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSprite();

        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        world.RegisterFurnitureCreated(OnFurnitureCreated);

        foreach (Furniture furniture in world.furnitures) {
            OnFurnitureCreated(furniture);
        }
    }

    void LoadSprite() {
        furnitureSpriteMap = new Dictionary<string, Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furnitures/");
        foreach( Sprite s in sprites) {
            furnitureSpriteMap[s.name] = s;
        }
    }

    public void OnFurnitureCreated( Furniture obj) {
        // FIXME: Does not consider multi-tile objects nor rotated objects.

        // Create a GameObject linked to this date
        GameObject obj_go = new GameObject();

        obj_go.name = obj.objectType + "_" + obj.tile.X  + "_" + obj.tile.Y;
        obj_go.transform.position = WorldController.Instance.GetTilePositionAtWorldCoord(obj.tile);
        obj_go.transform.SetParent(this.transform, true);

        furnitureGameObjectMap.Add(obj, obj_go);

        SpriteRenderer obj_sprite =  obj_go.AddComponent<SpriteRenderer>();

        // FIXME: we assume that the object must be a wall, so use
        // the hardcoded reference to the wall sprite.
        obj_sprite.sprite = GetSpriteForFurniture(obj);
        obj_sprite.sortingLayerName = "Furniture";

        obj.RegisterOnChangedCallBack(OnFurnitureChanged);
    }

    void OnFurnitureChanged(Furniture furn) {

        if (!furnitureGameObjectMap.ContainsKey(furn)) {
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];

        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
    }

    public Sprite GetSpriteForFurniture(Furniture furn) {
        if (furn.linksToNeighbour == false) {
            return furnitureSpriteMap[furn.objectType + "_"];
        }

        StringBuilder spriteName = new StringBuilder(furn.objectType);
        spriteName.Append("_");

        Func<int,int, Furniture, bool> TileValue = (int index_x,int index_y, Furniture func) => {
            Tile t = world.GetTileAt(index_x, index_y);
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

        string finalSpriteName = spriteName.ToString();
        // Debug.Log("Sprite Nam:" + finalSpriteName);

        if (!furnitureSpriteMap.ContainsKey(finalSpriteName)) {
            Debug.LogError("Sprite Name doesn't exist:" + finalSpriteName);
            return furnitureSpriteMap[furn.objectType];
        } else {
            return furnitureSpriteMap[finalSpriteName];
        }
    }

    public Sprite GetSpriteForFurniture(string objectType) {
        if (furnitureSpriteMap.ContainsKey(objectType))
            return furnitureSpriteMap[objectType];

        if (furnitureSpriteMap.ContainsKey(objectType + "_0_0"))
            return furnitureSpriteMap[objectType + "_0_0"];

        return null;
    }

}
