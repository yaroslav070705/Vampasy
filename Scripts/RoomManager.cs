using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks {
    [SerializeField] GameObject gameManagerPrefab;
    [SerializeField] GameObject gameInformationPrefab;
    [SerializeField] GameObject player;
    [SerializeField] GameObject startGameButton;
    GameManager gameManager;
    GameObject gameInf;
    [SerializeField] Canvas canvas;
   // [SerializeField] Button button;

    private void Start() {
        player = PhotonNetwork.Instantiate(player.name, Vector3.zero, new Quaternion(0, 0, 0, 0));
      //  button.onClick.AddListener(() => player.GetComponent<PlayerManager>().EndTurn());
        gameInf = PhotonNetwork.Instantiate(gameInformationPrefab.name, canvas.transform.position, canvas.transform.localRotation);
        player.GetComponent<PlayerManager>().photonView.RPC("SetGameInformation", RpcTarget.AllBuffered ,gameInf.GetPhotonView().ViewID);
// gameInf.GetComponent<GameInformation>().canvas = canvas;
        gameInf.GetComponent<GameInformation>().photonView.RPC("StartSettings", RpcTarget.AllBuffered, canvas.gameObject.GetPhotonView().ViewID, PhotonNetwork.PlayerList.Length, PhotonNetwork.NickName);
        // photonView.RPC("AddGameInfo", RpcTarget.AllBuffered);
        if (PhotonNetwork.IsMasterClient) {
            startGameButton.SetActive(true);
            player.GetComponent<PlayerManager>().isMyTurn = true;
        }
       // gameManager = PhotonNetwork.Instantiate(gameManagerPrefab.name, Vector3.zero, new Quaternion(0, 0, 0, 0)).GetComponent<GameManager>();
        //gameManager.playerManager = player.GetComponent<PlayerManager>();
        //if (gameManager.playerManager == null) {
        //    Debug.Log("no");
        //}
        //else {
         //   Debug.Log("yes");
        //}
    }

    [PunRPC]
     public void StartGame() {
         PhotonNetwork.CurrentRoom.IsOpen = false;
     }

    public void StartGameButtonFunc() {
        startGameButton.SetActive(false);
        gameManager = PhotonNetwork.Instantiate(gameManagerPrefab.name, Vector3.zero, new Quaternion(0, 0, 0, 0)).GetComponent<GameManager>();
        gameManager.playerManager = player.GetComponent<PlayerManager>();
        gameManager.gameInformation = gameInf.GetComponent<GameInformation>();
       // this.photonView.RPC("ChangeTurnName", RpcTarget.AllBuffered);
        this.photonView.RPC("StartGame", RpcTarget.AllBuffered);
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
    }

    public override void OnPlayerLeftRoom(Player player) {
        Debug.Log($"{player.NickName} disconnected");
        Debug.Log(PhotonNetwork.IsMasterClient);
    }
}


