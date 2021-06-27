using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;

public class TileSpriteController : MonoBehaviour
{
    public Sprite floorSprite;
    public Sprite emptySprite; 

    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    Dictionary<string, Sprite> installedObjectSpritesMap;

    World world {
        get { return WorldController.Instance.world;}
    }

    // Start is called before the first frame update
    void Start()
    {
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create GameObject
        for (int x = 0; x < world.Width; ++x)
        {
            for (int y = 0; y < world.Height; ++y)
            {
                Tile tile_data = world.GetTileAt(x, y);

                GameObject tile_go = new GameObject();
                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.position = WorldController.Instance.GetTilePositionAtWorldCoord(tile_data);
                tile_go.transform.SetParent(this.transform, true);

                tileGameObjectMap.Add(tile_data, tile_go);

                SpriteRenderer tile_sprite =  tile_go.AddComponent<SpriteRenderer>();
                tile_sprite.sprite = emptySprite;
            }
        }
        
        world.RegisterTileChanged(OnTileChanged);
    }

    void  DestroyAllTileGameObjects() {
        while (tileGameObjectMap.Count > 0) {
            Tile tile_data = tileGameObjectMap.Keys.First();
            GameObject tile_go = tileGameObjectMap[tile_data];

            tileGameObjectMap.Remove(tile_data);

            tile_data.UnredgisterTileTypeChangedCallback(OnTileChanged);

            Destroy( tile_go );
        }
    }

    void OnTileChanged(Tile tile_data) {
        if (!tileGameObjectMap.ContainsKey(tile_data)) {
            Debug.LogError("tile = GameObject  Map is nulll");
            return;
        }

        GameObject tile_go = tileGameObjectMap[tile_data];

        if (null == tile_go) {
            Debug.LogError("GameObject of tile_go is null");
            return;
        }


        if (tile_data.Type == Tile.TileType.Floor) {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        } else if (tile_data.Type == Tile.TileType.Empty) {
            tile_go.GetComponent<SpriteRenderer>().sprite = emptySprite;
        } else {
            Debug.LogError("OnTileTypeChanged - Unrecognized tile type:" + tile_data.Type);
        }

    }

}
