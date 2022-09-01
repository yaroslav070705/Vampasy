using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameInformation : MonoBehaviourPunCallbacks
{
    [SerializeField] Text nickName;
    [SerializeField] Text sticksNumLabel;
    public Canvas canvas { get; set; }

    [PunRPC]
    void SetStartSettings(int canvasViewID, int playersNum, string nickName) {
        if(canvas == null) {
            Debug.Log("null canvas");
        }
        canvas = PhotonView.Find(canvasViewID).gameObject.GetComponent<Canvas>();
        transform.SetParent(canvas.transform);
        transform.localScale = Vector3.one;
        transform.localPosition = transform.localPosition + new Vector3(700, 300, 0) - new Vector3(0, (playersNum - 1) * 300, 0);
        this.nickName.text = $"Player: {nickName}";
        sticksNumLabel.text = "0";
    }

    [PunRPC]
    void SetTurn() {
        nickName.text += "\n turn";
    }

}
