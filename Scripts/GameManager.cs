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
    Player[] playerList;
    List<(float, float, float)> positions;
    int turnIndex = 0;

    public PhotonView View { 
        get { return view; }
        set { view = value; }
    }

    public GameInformation gameInformation { get; set; }

    public PlayerManager playerManager { get; set; }
   // public Text text;

    private void Start() {
        //,(6.8f,-0.778f,-0.166f)
        if (PhotonNetwork.IsMasterClient) {
            positions = new List<(float, float, float)>() {(3.55f,-0.778f,-2.39f),(5.17f,-0.778f,-2.39f),(6.8f,-0.778f,-2.39f),(8.43f,-0.778f,-2.39f),
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
            SetLids();
            // text.text = PhotonNetwork.MasterClient.NickName;
            playerList = PhotonNetwork.PlayerList;
            playerManager.gameManager = this;
            //gameField = PhotonNetwork.Instantiate(gameField.name, new Vector3(0.29f, -1.12f, 5.38f), new Quaternion(0, 0, 0, 0));
            DistributeCards();
        }
    }

    private void DistributeCards() {
        if (playerManager == null) {
            Debug.Log("playerManager не найден");
        }
        Dictionary<int, string> colors = new Dictionary<int, string>() { { 0, "red" }, { 1, "blue" }, { 2, "green" }, { 3, "white" }, { 4, "black" }, { 5, "orange" } };
        int redNum = 10;
        int blueNum = 10;
        int greenNum = 10;
        int whiteNum = 10;
        int blackNum = 10;
        int orangeNum = 10;

        List<int> keys;
        int key;
        int cardsNum = 60 / playerList.Length;
        for (int i = 0; i < playerList.Length; i++) {
            string[] cardsColors = new string[cardsNum];
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
            if (playerManager?.view == null) {
                Debug.Log("view не найден");
            }
            playerManager?.view?.RPC("SetCards", playerList[i], cardsColors);
            //photonView.RPC("")
        }
    }

    private void SetLids() {
        (float x, float y, float z) pos;
        int ind = 0;
        for (int i = 0; i < 9; i++) {
            ind = Random.Range(0, positions.Count);
            pos = positions[ind];
            PhotonNetwork.Instantiate(lidPrefab.name, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0,0,0,0)).GetComponentInChildren<LidController>().photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, "Red");
            positions.RemoveAt(ind);
        }
        for (int i = 0; i < 9; i++) {
            ind = Random.Range(0, positions.Count);
            pos = positions[ind];
            PhotonNetwork.Instantiate(lidPrefab.name, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<LidController>().photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, "Blue");
            positions.RemoveAt(ind);
        }
        for (int i = 0; i < 9; i++) {
            ind = Random.Range(0, positions.Count);
            pos = positions[ind];
            PhotonNetwork.Instantiate(lidPrefab.name, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<LidController>().photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, "Green");
            positions.RemoveAt(ind);
        }
        for (int i = 0; i < 9; i++) {
            ind = Random.Range(0, positions.Count);
            pos = positions[ind];
            PhotonNetwork.Instantiate(lidPrefab.name, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<LidController>().photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, "White");
            positions.RemoveAt(ind);
        }
        for (int i = 0; i < 9; i++) {
            ind = Random.Range(0, positions.Count);
            pos = positions[ind];
            PhotonNetwork.Instantiate(lidPrefab.name, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<LidController>().photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, "Black");
            positions.RemoveAt(ind);
        }
        for (int i = 0; i < 9; i++) {
            ind = Random.Range(0, positions.Count);
            pos = positions[ind];
            PhotonNetwork.Instantiate(lidPrefab.name, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<LidController>().photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, "Orange");
            positions.RemoveAt(ind);
        }
    }
    
    public void ChangeTurn() {
        if (turnIndex < playerList.Length - 1) {
            turnIndex++;
        }

        else {
            turnIndex = 0;
        }
       // text.text = playerList[turnIndex].NickName + " Turn";
        playerManager.view.RPC("SetTurn", playerList[turnIndex]);
    }
}
