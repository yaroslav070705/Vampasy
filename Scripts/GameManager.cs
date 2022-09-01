using UnityEngine;
using Unity.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks {
    [SerializeField] GameObject gameField;
    [SerializeField] GameObject lidPrefab;
    [SerializeField] PhotonView view;
    [SerializeField] GameObject player;
    [SerializeField] public GameObject gameInf;
    [SerializeField] GameObject startGameButton;
    [SerializeField] Canvas canvas;

    Player[] networkPlayersList;
    List<PlayerManager> playersList;
    PlayerManager playerInTurn;
    LidController openedLid;
    //List<(float, float, float)> positions;
    int turnIndex = 0;

    public PhotonView View { 
        get { return view; }
        set { view = value; }
    }

    public GameInformation gameInformation { get; set; }

  //  public PlayerManager playerManager { get; set; }
    public RoomManager roomManager { get; set; }
   // public Text text;

    private void Start() {
        playersList = new List<PlayerManager>();

        player = PhotonNetwork.Instantiate(player.name, Vector3.zero, new Quaternion(0, 0, 0, 0));
        // photonView.RPC("SetGameInformation", RpcTarget.AllBuffered,PhotonNetwork.Instantiate(gameInf.name, canvas.transform.position, canvas.transform.localRotation).GetPhotonView().ViewID);
        player.GetComponent<PlayerManager>().photonView.RPC("SetGameManager", RpcTarget.AllBuffered, this.gameObject.GetPhotonView().ViewID);
        gameInf = PhotonNetwork.Instantiate(gameInf.name, canvas.transform.position, canvas.transform.localRotation);
        player.GetComponent<PlayerManager>().photonView.RPC("SetGameInformation", RpcTarget.AllBuffered, gameInf.GetPhotonView().ViewID);
        gameInf.GetComponent<GameInformation>().photonView.RPC("SetStartSettings", RpcTarget.AllBuffered, canvas.gameObject.GetPhotonView().ViewID, PhotonNetwork.PlayerList.Length, PhotonNetwork.NickName);
        photonView.RPC("AddPlayer", RpcTarget.MasterClient, player.GetPhotonView().ViewID);

        if (PhotonNetwork.IsMasterClient) {
            startGameButton.SetActive(true);
        }
    }

    private void DistributeCards() {
        Dictionary<int, string> colors = new Dictionary<int, string>() { { 0, "red" }, { 1, "blue" }, { 2, "green" }, { 3, "white" }, { 4, "black" }, { 5, "orange" } };
        List<string> garlicColors = new List<string>(){"Red", "Blue", "Green", "Orange", "Black", "White"};

        int redNum = 10;
        int blueNum = 10;
        int greenNum = 10;
        int whiteNum = 10;
        int blackNum = 10;
        int orangeNum = 10;

        List<int> keys;
        int key;
        int cardsNum = 60 / networkPlayersList.Length;
        for (int i = 0; i < networkPlayersList.Length; i++) {
            string[] cardsColors = new string[cardsNum];
            int index = Random.Range(0, garlicColors.Count);
            string garlicColor = garlicColors[index];
            garlicColors.RemoveAt(index);
            for (int j = 0; j < cardsNum; j++) {
                keys = colors.Keys.ToList();
                key = keys[Random.Range(0, keys.Count)];
                switch (colors[key]) {
                    case "red":
                        redNum--;
                        if (redNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Red";
                        break;

                    case "blue":
                        if (blueNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Blue";
                        break;

                    case "green":
                        if (greenNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Green";
                        break;

                    case "white":
                        if (whiteNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "White";
                        break;

                    case "black":
                        if (blackNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Black";
                        break;

                    case "orange":
                        if (orangeNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Orange";
                        break;
                }
            }

            Debug.Log(playersList.Count);
            playersList[i].view?.RPC("SetCards", networkPlayersList[i], cardsColors, garlicColor);
        }
    }

    private void SetLids() {
        List<(float, float, float)> positions = new List<(float, float, float)>() {(3.55f,-0.778f,-2.39f),(5.17f,-0.778f,-2.39f),(6.8f,-0.778f,-2.39f),(8.43f,-0.778f,-2.39f),
                                                           (3.55f,-0.778f,-0.166f),(5.17f,-0.778f,-0.166f),(8.43f,-0.778f,-0.166f),
                                                           (3.55f,-0.778f,2.06f),(5.17f,-0.778f,2.06f),(6.8f,-0.778f,2.06f),(8.43f,-0.778f,2.06f),
                                                           (3.55f,-0.778f,4.28f),(5.17f,-0.778f,4.28f),(6.8f,-0.778f,4.28f),(8.43f,-0.778f,4.28f),
                                                           (6.8f,-0.778f,6.5f),(8.43f,-0.778f,6.5f),

                                                           (-1.33f,-0.778f,-2.39f),(-2.96f,-0.778f,-2.39f),(-6.21f,-0.778f,-2.39f),(-7.84f,-0.778f,-2.39f),
                                                           (-1.33f,-0.778f,-0.166f),(-2.96f,-0.778f,-0.166f),(-4.59f,-0.778f,-0.166f),(-6.21f,-0.778f,-0.166f),(-7.84f,-0.778f,-0.166f),
                                                           (-1.33f,-0.778f,2.06f),(-2.96f,-0.778f,2.06f),(-4.59f,-0.778f,2.06f),(-6.21f,-0.778f,2.06f),(-7.84f,-0.778f,2.06f),
                                                           (-1.33f,-0.778f,4.28f),(-2.96f,-0.778f,4.28f),

                                                           (-1.33f,-0.778f,8.725f),(-2.96f,-0.778f,8.725f),(-4.59f,-0.778f,8.725f),(-6.21f,-0.778f,8.725f),(-7.84f,-0.778f,8.725f),
                                                           (-1.33f,-0.778f,10.94f),(-2.96f,-0.778f,10.94f),(-6.21f,-0.778f,10.94f),(-7.84f,-0.778f,10.94f),
                                                           (-1.33f,-0.778f,13.17f),(-2.96f,-0.778f,13.17f),(-4.59f,-0.778f,13.17f),(-6.21f,-0.778f,13.17f),(-7.84f,-0.778f,13.17f),
                                                           (-6.21f,-0.778f,6.5f),(-7.84f,-0.778f,6.5f),

                                                           (1.925f,-0.778f,8.725f),
                                                           (1.925f,-0.778f,10.94f),(3.55f,-0.778f,10.94f),(5.17f,-0.778f,10.94f),(6.8f,-0.778f,10.94f),(8.43f,-0.778f,10.94f),
                                                           (1.925f,-0.778f,13.17f),(3.55f,-0.778f,13.17f),(5.17f,-0.778f,13.17f),(6.8f,-0.778f,13.17f),(8.43f,-0.778f,13.17f) };
        (float x, float y, float z) pos;
        string[] colors = {"Red", "Blue", "Green", "White", "Black", "Orange"};
        int ind = 0;
        LidController lid;
       
        for(int i=0; i<54; i++) {
            ind = Random.Range(0, positions.Count);
            pos = positions[ind];
            lid = PhotonNetwork.Instantiate(lidPrefab.name, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<LidController>();
            lid.photonView.RPC("OnInstantinated", RpcTarget.All, colors[i/9], this.gameObject.GetPhotonView().ViewID);
            positions.RemoveAt(ind);
        }
    }
    
    private void EmptyLidOpened() {

    }

    private void NotEmptyLidOpened() {

    }

    public void ChangeTurn() {
        // text.text = playerList[turnIndex].NickName + " Turn";
        photonView.RPC("ChangeTurnIndex", RpcTarget.All);
        Debug.Log($"turnIndex:{turnIndex} \n nickName: {networkPlayersList[turnIndex].NickName} \n playerNum:{playersList.Count}");
        playersList[turnIndex].view.RPC("SetTurn", networkPlayersList[turnIndex]);
    }

   /* public void OpenLid(LidController lid) {
        lid.LidOpened += OnLidOpened;
    }*/

    public void OnLidOpened(LidController lid) {
        openedLid = lid;

        if (lid.isEmpty) {
            EmptyLidOpened();
        }
        else {
            NotEmptyLidOpened();
        }
    }


    public void GiveCard(string color) {

    }

    [PunRPC]
    private void ChangeTurnIndex() {
        if (turnIndex < networkPlayersList.Length - 1) {
            turnIndex++;
        }
        else {
            turnIndex = 0;
        }
    }

    [PunRPC]
    private void AddPlayer(int viewID) {
        Debug.Log("Add Player");
        if (playersList is null) {
            Debug.Log("Player Manager list is null");
        }
        playersList.Add(PhotonView.Find(viewID).GetComponent<PlayerManager>());
       // roomManager.photonView.RPC("PlayerAdded", RpcTarget.MasterClient);
    }

    [PunRPC]
    private void SetNetworkPlayersList() {
        networkPlayersList = PhotonNetwork.PlayerList;
    }

    [PunRPC]
    public void SetPlayerInTurn(int viewID) {
        playerInTurn = PhotonView.Find(viewID).GetComponent<PlayerManager>();
    }

    public void StartGame() {
        photonView.RPC("SetNetworkPlayersList", RpcTarget.All);

        foreach(PlayerManager player in playersList) {
            photonView.RPC("AddPlayer", RpcTarget.Others, player.gameObject.GetPhotonView().ViewID);
        }
        photonView.RPC("SetPlayerInTurn", RpcTarget.All, playersList[0].gameObject.GetPhotonView().ViewID);

        startGameButton.SetActive(false);
        
        SetLids();
        DistributeCards();
    }

    public void DestroyCard(GameObject card) {
        PhotonNetwork.Destroy(card);
    }
}
