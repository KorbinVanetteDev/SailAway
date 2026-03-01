using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockedShipSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform dockSpawnPoint;
    public bool spawnOnStart = true;

    private GameObject currentDockedShip;

    void Start()
    {
        if(spawnOnStart)
        {
            SpawnDockedShip();
        }
    }

    public void SpawnDockedShip()
    {
        if(currentDockedShip != null)
        {
            Destroy(currentDockedShip);
        }

        if(ShipManager.Instance == null)
        {
            return;
        } 

        GameObject dockedShipPrefab = ShipManager.Instance.GetDockedShipModel();

        if(dockedShipPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;

        if(dockSpawnPoint != null)
        {
            spawnPos = dockSpawnPoint.position;
            spawnRot = dockSpawnPoint.rotation;
        }

        currentDockedShip = Instantiate(dockedShipPrefab, spawnPos, spawnRot);
        currentDockedShip.name = "DockedShip";
    }
}
