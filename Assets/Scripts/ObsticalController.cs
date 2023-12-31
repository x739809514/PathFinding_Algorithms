using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObsticalController : MonoBehaviour
{
    public int width = 20;
    public int height = 20;

    public GameObject wall;
    public GameObject player;

    public bool playerSpawn;
    public NavMeshSurface surface;
    public Transform wallParent;

    private void Start()
    {
        GenerateLevel();
        surface.BuildNavMesh();
    }

    private void GenerateLevel()
    {
        for (int x = 0; x <= width; x+=2)
        {
            for (int y = 0; y < height; y+=2)
            {
                if (Random.value > 0.7f)
                {
                    var pos = new Vector3(x - width / 2f, 1, y - height / 2f);
                    var go = Instantiate(wall, pos, Quaternion.identity);
                    go.transform.SetParent(wallParent);
                }
                else
                {
                    if (playerSpawn == false)
                    {
                        var pos = new Vector3(x - width / 2f, 1.25f, y - height / 2f);
                        Instantiate(player, pos, Quaternion.identity);
                        playerSpawn = true;
                    }
                }
            }
        }
    }
}
