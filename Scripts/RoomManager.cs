using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks {

    [SerializeField] GameObject gameManagerPrefab;
    GameManager gameManager;
    int necessaryPlayerNum = 1;

    private void Start() {
       // gameManager.GameStarted += () => PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    public void LeaveGame() {
        Debug.Log("leave room");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnMasterClientSwitched(Player newMasterClient) {
        Debug.Log(newMasterClient.NickName);
    }

    public override void OnLeftRoom() {
        Debug.Log("room left");
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player player) {
        Debug.Log($"{player.NickName} connected");
        if(PhotonNetwork.PlayerList.Length == necessaryPlayerNum) {
            gameManager.EnableToStartGame();
           // gameManager.GameStarted += () => PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    public override void OnPlayerLeftRoom(Player player) {
        Debug.Log($"{player.NickName} disconnected");
        Debug.Log(PhotonNetwork.IsMasterClient);
    }

    private void Destroy() {
        if (!PhotonNetwork.IsMasterClient) {
            Destroy(this);
        }
    }
}


