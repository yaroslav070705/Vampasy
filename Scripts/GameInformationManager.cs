using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameInformationManager : MonoBehaviourPunCallbacks
{
    public List<PlayerInformationManager> playerInformationManagers { get; set; }
    public string playerInTurnNickName { get; set; }

    private void Awake() {
        Debug.Log("init");
        playerInformationManagers = new List<PlayerInformationManager>(6);
    }


    [PunRPC]
    public void UpdatePlayerInformation(int pInfIndex) {
        playerInformationManagers[pInfIndex].UpdateInformation();
    }

}
