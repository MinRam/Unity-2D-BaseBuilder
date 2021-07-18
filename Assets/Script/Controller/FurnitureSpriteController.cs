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

        // FIXME: this hardcode is not ideal!
        if (obj.objectType == "Door") {
            Tile nothTile = world.GetTileAt(obj.tile.X, obj.tile.Y - 1);

            if (nothTile != null && nothTile.furniture != null && nothTile.furniture.objectType == "Wall") {
                obj_go.transform.rotation = Quaternion.Euler( 0, 0, 90);
                obj_go.transform.Translate(1f, 0, 0, Space.World);
            }
        }

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
        if (furn.getSpriteName != null) {
            string spriteName = furn.getSpriteName(furn);
            if (furnitureSpriteMap.ContainsKey(spriteName))
                return furnitureSpriteMap[spriteName];
        }
        return furnitureSpriteMap[furn.objectType + "_0_0"];
    }

    public Sprite GetSpriteForFurniture(string objectType) {
        if (furnitureSpriteMap.ContainsKey(objectType))
            return furnitureSpriteMap[objectType];

        if (furnitureSpriteMap.ContainsKey(objectType + "_0_0"))
            return furnitureSpriteMap[objectType + "_0_0"];

        return null;
    }

}
