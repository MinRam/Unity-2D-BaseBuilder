using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance {get; protected set;}
    public World world {get; protected set;}

    static bool loadWorld = false;

    // Start is called before the first frame update
    void OnEnable()
    {

        if (Instance != null)
            Debug.LogError("There should never be two the world controllers.");

        Instance = this;

        if (loadWorld) {
            loadWorld = false;
            CreateWorldFromSavedFile();
        }
        else
            CreateEmptyWorld();
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

    public void NewWorld() {
        Debug.Log("New World");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveWorld() {
        Debug.Log("Save World");

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, world);
        writer.Close();

        // Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());
    }

    public void LoadWorld() {
        Debug.Log("Load World");
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CreateEmptyWorld() {
        world = new World(100, 100);

        Tile centerTile = world.GetTileAt(world.Width / 2, world.Height / 2);
        Vector3  centerPosition = GetTilePositionAtWorldCoord(centerTile);
        centerPosition.z = Camera.main.transform.position.z;
        Camera.main.transform.position = centerPosition;
    }

    void CreateWorldFromSavedFile() {

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));

        world = (World) serializer.Deserialize(reader);
        reader.Close();

        Tile centerTile = world.GetTileAt(world.Width / 2, world.Height / 2);
        Vector3  centerPosition = GetTilePositionAtWorldCoord(centerTile);
        centerPosition.z = Camera.main.transform.position.z;
        Camera.main.transform.position = centerPosition;
    }
}
