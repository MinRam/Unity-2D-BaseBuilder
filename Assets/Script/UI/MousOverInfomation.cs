using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MousOverInfomation : MonoBehaviour
{
    /**
     *  Every frame, this script checks to see which tile
     *  is under the mouse and then update the Getcomponent<text>
     *  parameter
     */

    Dictionary<string, GameObject> infoGameObject;
    MouseController mouseController;

    // Start is called before the first frame update
    void Start()
    {
        infoGameObject = new Dictionary<string, GameObject>();

        // Reister several game object.
        Transform[] childTrans = GetComponentsInChildren<Transform>();
        foreach (Transform transform in childTrans) {
            GameObject childTextType = transform.gameObject;
            infoGameObject.Add(childTextType.name, childTextType);
        }

        mouseController = GameObject.FindObjectOfType<MouseController>();
        if (null == mouseController) {
            Debug.LogError("we don't have an instance of mouse controller.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Tile curTile = mouseController.GetMouseOverTIle();

        infoGameObject["Tile Type"].GetComponent<Text>().text = "Tile Type: " + curTile.Type;
        infoGameObject["Tile Posi"].GetComponent<Text>().text = "Tile Position: " + curTile.X + "," + curTile.Y;

        string furnitureType = curTile.furniture == null ? "NULL" : curTile.furniture.objectType;
        infoGameObject["Furniture Type"].GetComponent<Text>().text = "Furniture Type: " + furnitureType;
        infoGameObject["Room Index"].GetComponent<Text>().text = "Room Index: " + curTile.World.rooms.IndexOf(curTile.room);
    }
}
