using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Transports.UNET;
using UnityEngine;
using System;
public class MenuScript : NetworkBehaviour
{
    public GameObject menuPanel;
    public GameObject gamePanel;

    // Generated mazes using renderer, not ideal
    [SerializeField]
    private Transform mazePrefab1 = null;
    [SerializeField]
    private Transform mazePrefab2 = null;
    
    public List<Transform> mazeList = new List<Transform>();

    private Transform maze = null;

    public static string myName {get;set;}

    public string ipAddress = "127.0.0.1";

    UNetTransport transport;

    [SerializeField]
    private MazeRenderer mz = null;
    // private NetworkVariable<WallState[,]> mazeNV = new NetworkVariable<WallState[,]>();

    // [ServerRpc]
    // public void SetMazeServerRpc()
    // {
    //     Debug.Log("Maze set: "+ mazeNV.Value);
    //     mazeNV.Value = MazeGenerator.Generate(10, 10);
    // }


    void Awake()
    {
        gamePanel.SetActive(false);
    }
    public void Start()
    {

        //var maze = MazeGenerator.Generate(width, height);
        //mr.Draw(mazeNV.Value);
  
    }
    void Update()
    {
        if (Input.GetKeyDown (KeyCode.Escape)){
            if(gamePanel.active)
            {
                Cursor.lockState = CursorLockMode.Locked;
                gamePanel.SetActive(false);

            } 
            else {
                gamePanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
        }
        // if(IsServer)
        // {
        //     Debug.Log("Server");
        //     if(Input.GetKeyDown("right ctrl")){
        //         //ResetMaze();
        //         SpawnNewLevelServerRpc();
        //     }            
        // }
        // else
        // {
        //     if(Input.GetKeyDown("right ctrl")){
        //         Debug.Log("Client and keypressed, Owner: " + IsOwner);

        //         SpawnNewLevelServerRpc();
        //     }
        // }

        

    }

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        menuPanel.SetActive(false);

        //mz.SetMazeServerRpc();
        // AddMazes();

        // var rnd = new System.Random();

        // Add maze to host game only
        //maze = Instantiate(mazeList[rnd.Next(0,2)], transform);

        // Fit to world
        //maze.localScale = new Vector3(1, 1,1);
        //maze.position = new Vector3 (-2,0.5f,0);

        // Add maze to clients
        // Must be a prefab. 
        // Current issue. Cant generate and send maze to clients dynamically yet.
        //maze.GetComponent<NetworkObject>().Spawn();


    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        bool approve = System.Text.Encoding.ASCII.GetString(connectionData) == "Password1234";
        //callback(true, nulll, approve, GetR)
    }

    public void Join()
    {
        // try{
            transport = NetworkManager.Singleton.GetComponent<UNetTransport>();
            transport.ConnectAddress = ipAddress;
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes("Password1234");
            NetworkManager.Singleton.StartClient();
            menuPanel.SetActive(false);
        // }
        // catch (Exception e){
        //     Debug.Log(e);
        // }
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void AddMazes()
    {
        mazeList.Add(mazePrefab1);
        mazeList.Add(mazePrefab2);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnNewLevelServerRpc()
    {
        SpawnNewLevelClientRpc();
    }

    [ClientRpc]
    void SpawnNewLevelClientRpc()
    {
        Debug.Log("New level");
        StartCoroutine(ResetMaze());

    }

    public IEnumerator ResetMaze()
    {
        Debug.Log("Despawn maze");
        maze.GetComponent<NetworkObject>().Despawn();
        Debug.Log(maze.gameObject.name);
        Destroy(maze.gameObject);

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Wait 2 sec, and spawn new maze");
        var rnd = new System.Random();
        maze = Instantiate(mazeList[rnd.Next(0,2)], transform);
        maze.localScale = new Vector3(1, 1,1);
        maze.position = new Vector3 (-2,0.5f,0);
        maze.GetComponent<NetworkObject>().Spawn();

    }
    

    public void setName(string name)
    {
            myName = name;
            Debug.Log(myName);
    }    

    public void IPAddressChanged(string newAddress)
    {
        this.ipAddress = newAddress;
    }
}
