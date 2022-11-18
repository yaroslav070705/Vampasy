using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerInformationManager : MonoBehaviourPunCallbacks
{
    PlayerManager player;
    int vampiresNum { get; set; } = 0;
    int garlicsNum { get; set; } = 0;
    int sticksNum { get; set; } = 0;
    VampireCard leftCard { get; set; }
    VampireCard rightCard { get; set; }
    [SerializeField] Text nickNameField;
    [SerializeField] Text sticksNumField;
    //[SerializeField] Canvas canvas;

    public void UpdateInformation() {
        vampiresNum = player.vampireCardsList.Count;
        garlicsNum = player.garlicCardsList.Length;
        sticksNum = player.sticksNum;
        rightCard = player.rightVampireCard;
        leftCard = player.leftVampireCard;
    }
}
