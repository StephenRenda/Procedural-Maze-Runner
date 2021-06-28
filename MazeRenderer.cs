// https://www.youtube.com/watch?v=ya1HyptE5uc
// Original code modified to generate maze for host and draw for clients using MLAPI

using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;
using System;
public class MazeRenderer : NetworkBehaviour
{
    [SerializeField]
    [Range(1,50)]
    private int width = 10; 

    [SerializeField]
    [Range(1,50)]
    private int height = 10;

    [SerializeField]
    private float size = 1f;


    [SerializeField]
    private Transform floorPrefab;

    [SerializeField]
    private Transform wallPrefab;

    [SerializeField]
    private Vector3 positionOffset = new Vector3 (-120.0f,0,0);

    public Button btn = null;

    private NetworkVariable<WallState[]> maze1d = new NetworkVariable<WallState[]>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.Everyone});

    private int playerCount = 1;

    public void Start()
    {
        maze1d.Value = new WallState[width*height];

        if(GameObject.FindWithTag("HostButton"))
        {
            btn = GameObject.FindWithTag("HostButton").GetComponent<Button>();
            btn.onClick.AddListener(SetMazeServerRpc);
        }
        // if(GameObject.FindWithTag("JoinButton") )
        // {
        //     btn = GameObject.FindWithTag("JoinButton").GetComponent<Button>();
        //     btn.onClick.AddListener(JoinServerRpc);
        //     Debug.Log("Clicked the join button");
        // }
        
    }
    public WallState[,] Maze1dto2d(WallState[] m1d)
    {
        WallState[,] m2d = new WallState[width,height];

        int i,j,k = 0;
        for (i = 0; i < width; i++) {
            for (j = 0; j < height; j++) {
                m2d[i, j] = m1d[k++];
            }
        }
        return m2d;
    }
    public void Update()
    {
        if(IsServer)
        {
            Debug.Log("Server");
            if(Input.GetKeyDown("right ctrl")){
                SetMazeServerRpc();
            }

            var count = 0;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in players)
            count++; 
            Debug.Log("Player count: " + count);    

            if(count > playerCount)
            {
                Debug.Log("count: " + count + " Player count: " + playerCount) ;    
                SetMazeServerRpc();
                playerCount++;
            }     
        }
        else
        {
            Debug.Log("Not Server");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetMazeServerRpc()
    {
        // Cant send a 2d array with mlapi yet.
        // So we get a 2d and turn it into a 1d
        // Then on client we can turn back into 2d
        var maze2d =MazeGenerator.Generate(width, height);
        int i,j,k = 0;
        for (i = 0; i < width; i++) {
            for (j = 0; j < height; j++) {
                maze1d.Value[k++] = maze2d[i, j];
            }
        }
        SetMazeClientRpc(maze1d.Value);
    }
    [ClientRpc]
    void SetMazeClientRpc(WallState[] m1)
    {
        // Now we need to turn 1d back into 2d array for Draw function
        // StartCoroutine(ResetMaze(m1));
        ResetMaze(m1);

    }
    public void ResetMaze(WallState[] m1)
    {
        DestroyEverything();
        //yield return new WaitForSeconds(0.5f);
        Draw(Maze1dto2d(m1));
    }
    public void DestroyEverything()
    {
        Debug.Log("Destroying everything");
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        Destroy(GameObject.FindWithTag("Wall"));
        foreach(GameObject wall in walls)
        GameObject.Destroy(wall);

        GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");
        foreach(GameObject floor in floors)
        GameObject.Destroy(floor);
    }

    public void Draw(WallState[,] maze)
    {
        var floor = Instantiate(floorPrefab, transform);
        var roof = Instantiate(floorPrefab, transform);
        floor.localScale = new Vector3(size, 1,size);
        floor.position = new Vector3 (-30.0f,0,40);
        
        roof.localScale = new Vector3(size, 1,size);
        roof.position = new Vector3 (-30.0f,10.0f,40);
        roof.eulerAngles = new Vector3(180,0,0);
        for (int i=0; i < width; i++)
        {
            for (int j=0; j < height; j++)
            {
                var cell = maze[i, j];
                var position = new Vector3(-width/2 + size*i, size/2, -height/2 + size*j) + positionOffset;

                Debug.Log(size/2);
                
                if(cell.HasFlag(WallState.UP))
                {
                    var topWall = Instantiate(wallPrefab, transform) as Transform;
                    topWall.position = position + new Vector3(0,0,size/2);
                    topWall.localScale = new Vector3(size, topWall.localScale.y *size, topWall.localScale.z);
                }


                if(cell.HasFlag(WallState.LEFT))
                {
                    var leftWall = Instantiate(wallPrefab, transform) as Transform;
                    leftWall.position = position + new Vector3(-size/2,0,0);
                    leftWall.localScale = new Vector3(size, leftWall.localScale.y *size, leftWall.localScale.z);
                    leftWall.eulerAngles = new Vector3(0,90,0);
                }
                if(i == width -1)
                {
                    if(cell.HasFlag(WallState.RIGHT))
                    {
                        var rightWall = Instantiate(wallPrefab, transform) as Transform;
                        rightWall.position = position + new Vector3(+size/2,0,0);
                        rightWall.localScale = new Vector3(size, rightWall.localScale.y *size, rightWall.localScale.z);
                        rightWall.eulerAngles = new Vector3(0,90,0);
                    }
                }

                if(j==0)
                {
                    if(cell.HasFlag(WallState.DOWN))
                    {
                        var bottomWall = Instantiate(wallPrefab, transform) as Transform;
                        bottomWall.position = position + new Vector3(0,0,-size/2) ;
                        bottomWall.localScale = new Vector3(size, bottomWall.localScale.y *size, bottomWall.localScale.z);
                    }
                }
            }
        }
    }

}
