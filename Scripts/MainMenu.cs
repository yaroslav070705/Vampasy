using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MainMenu : MonoBehaviourPunCallbacks
{

    [SerializeField] Text log;
    [SerializeField] GameObject buttonsPanel;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = "1";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void Log(string message)
    {
        Debug.Log(message);
        log.text += message;
        log.text += '\n';
    }

    public void ConnectToGame()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void CreateGame()
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions());
    }

    public override void OnConnectedToMaster()
    {
        buttonsPanel.SetActive(true);
        Log("Connected to MasterServer");
    }

    public override void OnJoinedRoom()
    {
        Log("Connected to Room");
        Log(PhotonNetwork.IsMasterClient.ToString());
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("aa");
        Log("Connected to Room");
        Log(PhotonNetwork.IsMasterClient.ToString());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Log("Failed to join Room");
    }
}
