using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour
{
    FurnitureSpriteController furnitureSpriteController;
    Dictionary<Job, GameObject> jobGameObjectMap;
    // Start is called before the first frame update
    void Start()
    {
        furnitureSpriteController = GameObject.FindObjectOfType<FurnitureSpriteController>();
        jobGameObjectMap = new Dictionary<Job, GameObject>();

        // JobQueue.RegisterJobCreationCallback(OnJobCreated);
        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnJobCreated(Job j) {

        if (j == null) return;

        GameObject job_go;
        if (!jobGameObjectMap.ContainsKey(j)) {
            // Debug.Log("new Job create at " + j.tile.X + "_" + j.tile.Y);
            job_go = new GameObject();
            jobGameObjectMap.Add(j, job_go);
        }
        else
            // Job Already Created.
            // Debug.Log("Job reflesh at " + j.tile.X + "_" + j.tile.Y);
            return;

        job_go.name = "JOB" + j.jobObjectType + "_" +j.tile.X + "_" + j.tile.Y ;
        job_go.transform.position = WorldController.Instance.GetTilePositionAtWorldCoord(j.tile);
        job_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr =  job_go.AddComponent<SpriteRenderer>();
        sr.sprite = furnitureSpriteController.GetSpriteForFurniture(j.jobObjectType);
        sr.color = new Color( 0.5f, 1f, 0.5f, 0.5f);
        sr.sortingLayerName = "Furniture";

        // We can only do furniture-building jobs.
        j.RegisterJobCompleteCallback(OnJobEnded);
        j.RegisterJobCancelCallback(OnJobEnded);
    }

    void OnJobEnded(Job j) {
        GameObject job_go = jobGameObjectMap[j];

        j.UnRegisterJobCompleteCallback(OnJobEnded);
        j.UnRegisterJobCancelCallback(OnJobEnded);

        Destroy(job_go);
    }


}
