using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;


public class MouseController : MonoBehaviour
{
    Vector3 lastFramePosition;
    Vector3 currFramePosition;

    Vector3 dragStartPosition;
    List<GameObject> dragPreviewGameObjects;

    public GameObject cursor;

    // Start is called before the first frame update
    void Start()
    {
        dragPreviewGameObjects = new List<GameObject>();

        // SimplePool.Preload(cursor, 30);
    }

    // Update is called once per frame
    void Update()
    {
        currFramePosition =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        // MoveCursorInTile();

        UpdateCameraMovement();

        DragSeveralTileByLeftClick();

        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;   // confirm the diff of z between lastFram and curFram is zero.
    }

    void MoveCursorInTile() {
        Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoord(currFramePosition);

        if (tileUnderMouse != null) {
            cursor.SetActive(true);
            Vector3 cursorPosition = new Vector3(tileUnderMouse.X, tileUnderMouse.Y, 0);
            cursor.transform.position = cursorPosition;
        } else {
            cursor.SetActive(false);
        }
    }

    void UpdateCameraMovement() {
        // move Mouse Camera
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
            Vector3 diff = lastFramePosition - currFramePosition;
            Camera.main.transform.Translate(diff);
        }

        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f);
    }

    void DragSeveralTileByLeftClick() {
        // bail out from this if we're over a UI element.
        if ( EventSystem.current.IsPointerOverGameObject())
            return;

        // Start Drag
		if( Input.GetMouseButtonDown(0) ) {
			dragStartPosition = currFramePosition;
		}

        int start_x = Mathf.FloorToInt( dragStartPosition.x );
        int end_x = Mathf.FloorToInt( currFramePosition.x );
        if (start_x > end_x) {
                int tmp = end_x;
                end_x = start_x;
                start_x = tmp;
            }

        int start_y = Mathf.FloorToInt( dragStartPosition.y );
        int end_y = Mathf.FloorToInt( currFramePosition.y );
        if (start_y > end_y) {
            int tmp = end_y;
            end_y = start_y;
            start_y = tmp;
        }

        int lastIndex = dragPreviewGameObjects.Count;
        while (--lastIndex >= 0) {
            GameObject go = dragPreviewGameObjects[lastIndex];
            dragPreviewGameObjects.RemoveAt(lastIndex);
            SimplePool.Despawn(go);
        }

        if (Input.GetMouseButton(0)) {
            for (int x = start_x; x <= end_x; ++x) {
                for (int y = start_y; y <= end_y; ++y) {
                    Tile t = WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y,0));
                    if (t != null) {
                        GameObject go = SimplePool.Spawn( cursor, new Vector3(x, y, 0), Quaternion.identity );
                        go.transform.SetParent(this.transform, true);
                        dragPreviewGameObjects.Add(go);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            BuildModeController bmc = GameObject.FindObjectOfType<BuildModeController>();

            for (int x = start_x; x <= end_x; ++x) {
                for (int y = start_y; y <= end_y; ++y) {
                    Tile selectedTile = WorldController.Instance.GetTileAtWorldCoord(new Vector3(x,y,0));

                    if (selectedTile != null) {
                        // Call BuildModeController
                        bmc.DoBuild(selectedTile);
                    }
                }
            }
        }
    }
}
