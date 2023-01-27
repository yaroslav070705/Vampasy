using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System;

public class PlayerInformationManager : MonoBehaviourPunCallbacks
{
    public PlayerManager player { get; set; }
    public string nickName { get; private set; }
    private int vampiresNum { get; set; } = 0;
    private int garlicsNum { get; set; } = 0;
    public int sticksNum { get; private set; } = 0;
    VampireCard leftCard { get; set; }
    VampireCard rightCard { get; set; }
    [SerializeField] Text nickNameField;
    [SerializeField] Text sticksNumField;
    //[SerializeField] Canvas canvas;

    public void OnAdded(int index) {
        transform.localScale = new Vector3(5, 5, 5);
        transform.localPosition = new Vector3(675, 400 - index * 150, 10);
        transform.localRotation = new Quaternion(0, 0, 0, 0);
    }

    public void SetInformation(string name) {
        nickName = name;
        nickNameField.text = nickName;
    }

    public void UpdateInformation() {
        vampiresNum = player.vampireCardsList.Count;
        garlicsNum = player.garlicCardsList.Length;
        sticksNum = player.sticksNum;
        rightCard = player.rightVampireCard;
        leftCard = player.leftVampireCard;
        nickNameField.text = nickName;
        sticksNumField.text = sticksNum.ToString();
        photonView.RPC("UpdateInformationsToOther",RpcTarget.Others, vampiresNum, garlicsNum, sticksNum);
    }
    [PunRPC]
    private void UpdateInformationsToOther(int vampireCardsNum, int garlicCardsNum, int sticksNum) {
        vampiresNum = vampireCardsNum;
        garlicsNum = garlicCardsNum;
        this.sticksNum = sticksNum;
        nickNameField.text = nickName;
        sticksNumField.text = sticksNum.ToString();
    }
}
