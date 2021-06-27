using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance {get; protected set;}
    public World world {get; protected set;}


    // Start is called before the first frame update
    void OnEnable()
    {

        if (Instance != null)
            Debug.LogError("There should never be two the world controllers.");

        Instance = this;

        world = new World();

        Tile centerTile = world.GetTileAt(world.Width / 2, world.Height / 2);
        Vector3  centerPosition = GetTilePositionAtWorldCoord(centerTile);
        centerPosition.z = Camera.main.transform.position.z;
        Camera.main.transform.position = centerPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Add pause/unpause, speed controls, etc...
        world.Update(Time.deltaTime);
    }

    public Tile GetTileAtWorldCoord(Vector3 coord) {
        Vector3 worldPosition = this.transform.position;

        int x = Mathf.FloorToInt(coord.x - worldPosition.x);
        int y = Mathf.FloorToInt(coord.y - worldPosition.y);

        return world.GetTileAt(x, y);
    }

    public Vector3 GetTilePositionAtWorldCoord(Tile tile) {
        Vector3 worldPosition = this.transform.position;

        return new Vector3(worldPosition.x + tile.X, worldPosition.y + tile.Y, 0);
    }
}
