using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour
{   
    Dictionary<Character, GameObject> characterGameObjectMap;
    Dictionary<string, Sprite> characterSpriteMap;
    World world {
        get { return WorldController.Instance.world;}
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSprite();

        characterGameObjectMap = new Dictionary<Character, GameObject>();

        world.RegisterCharacterCreated(OnCharacterCreated);

        // Debug
        Character c =  world.CreateCharacter(world.GetTileAt(world.Width/2 - 5 , world.Height/2 - 5));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadSprite() {
        characterSpriteMap = new Dictionary<string, Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Characters/");
        foreach( Sprite s in sprites) {
            characterSpriteMap[s.name] = s;
        }
    }

    public void OnCharacterCreated(Character obj) {
        Debug.Log("OnCharacterCreated");

        // Create a GameObject linked to this date
        GameObject obj_go = new GameObject();

        obj_go.name = "Character";
        obj_go.transform.position = WorldController.Instance.GetTilePositionAtWorldCoord(obj.currTile);
        obj_go.transform.SetParent(this.transform, true);

        characterGameObjectMap.Add(obj, obj_go);

        SpriteRenderer obj_sprite =  obj_go.AddComponent<SpriteRenderer>();

        // FIXME: we assume that the object must be a wall, so use
        // the hardcoded reference to the wall sprite.
        obj_sprite.sprite = GetSpriteForCharacter(obj);
        obj_sprite.sortingLayerName = "Character";

        obj.RegisterOnChangedCallBack(OnCharacterChanged);
    }

    void OnCharacterChanged(Character character) {
        if (!characterGameObjectMap.ContainsKey(character)) {
            Debug.Log("character is empty");
            return;
        }

        GameObject chara_go = characterGameObjectMap[character];

        chara_go.transform.position = new Vector3(character.X, character.Y, 0);

        // chara_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForCharacter(character);
    }

    public Sprite GetSpriteForCharacter(Character character) {
        return characterSpriteMap["Idle_0"];
    }
}
