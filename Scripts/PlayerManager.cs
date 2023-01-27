using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System;
using Photon.Realtime;
using System.Net.NetworkInformation;
//using UnityEditor.Animations;

public class PlayerManager : MonoBehaviourPunCallbacks {
    public GameManager gameManager { get;set;}
    public LinkedList<VampireCard> vampireCardsList { get; set; }
    public VampireCard[] vampiresCardsPlayerHaveToTake { get; set; }
    public GarlicCard[] garlicCardsList { get; set; }
    public List<CardPlaceHolder> cardPlaceHoldersList { get; set; }
    public VampireCard rightVampireCard { get; set; }
    public VampireCard leftVampireCard { get; set; }
    public VampireCard selectedVampireCard { get; set; }
    public int vampireCardsNum { get; set; }
    public int sticksNum { get; set; }
    public int haveToGiveCardsNum { get; set; }
    public int haveToTakeCardsNum { get; set; }
    public string garlicColor { get; set; }
    public string nickName { get; set; }
    public bool haveToTakeCards { get; set; } = false;

    private ISelectable selectable = null;
    private ISelectable prefSelected = null;
    private Ray arm;
    private RaycastHit raycastHit;
    private float distance = 500f;

    private void Start() {
        arm = new Ray();
    }
    private void Update() {
        if (photonView.IsMine) {
            arm = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(arm.origin, arm.direction * 500, Color.red);
            if (Physics.Raycast(arm, out raycastHit, distance)) {
                selectable = raycastHit.collider.gameObject.GetComponent<ISelectable>();
                if (selectable != null) {
                    if (selectable == prefSelected) { }
                    else {
                        selectable.Selected();
                        prefSelected?.Deselected();
                        prefSelected = selectable;
                    }
                }
                else {
                    prefSelected?.Deselected();
                    prefSelected = null;
                }
            }
            else {
                prefSelected?.Deselected();
                prefSelected = null;
                selectable = null;
            }
            if (Input.GetMouseButtonDown(0)) {
                if (selectable != null) {
                    gameManager.interactionResult += InteractionResult;
                    selectable.Interact(photonView.ViewID);
                }
            }
            if (Input.GetKeyDown(KeyCode.S)) {
                gameManager.EndTurn(this);
            }
            if (Input.GetKeyDown(KeyCode.E)) {
                gameManager.RotateField(photonView.ViewID);
            }
        }
    }

    [PunRPC]
    public void HaveToGiveCards(int cardsNum) {
        haveToGiveCardsNum = cardsNum;
        Debug.Log($"{haveToGiveCardsNum} cards");
    }

    public void TurnEnded() {
        rightVampireCard = vampireCardsList.Last.Value;
        leftVampireCard = vampireCardsList.First.Value;
    }

    private void InteractionResult(bool result) {
        if (result) {
            selectable?.Deselected();
            prefSelected?.Deselected();
            selectable = null;
            prefSelected = null;
            vampireCardsNum--;
        }
        gameManager.interactionResult -= InteractionResult;
    }
}
