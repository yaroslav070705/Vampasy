using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System;
using Photon.Realtime;
using UnityEngine.UI;
//using UnityEditor.Animations;

public class PlayerManager : MonoBehaviourPunCallbacks {
    GameObject cardPrafab;
    Ray arm;
    RaycastHit raycastHit;
    ISelectable selectableObject;
    ISelectable prefSelectableObject;
    float distance = 500f;
   // GameObject lid;
    string lidColor;
    bool lidOpened = false;
    Vector3 lidPos;
    RuntimeAnimatorController animatorController;
    int cardNum;
    public LidController lid;
    int sticksNum = 0;
    CardController rightCard;
    CardController leftCard;

    public bool isMyTurn { get; set; } = false;

    [SerializeField] public PhotonView view;
    List<CardController> vampireCardsList;
    public GameManager gameManager { get; set; }
    public GameInformation gameInf;

    private void Start() {
        //view = GetComponent<PhotonView>();
        arm = new Ray();
        animatorController = Resources.Load("Animation/Card", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
        cardPrafab = Resources.Load("CardPrefab 1", typeof(GameObject)) as GameObject;
    }

    void Update() {

        //arm.origin = Camera.main.transform.position;
        //arm.direction = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000);
        if (isMyTurn) {
            arm = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(arm.origin, arm.direction * 500, Color.red);
            if (Physics.Raycast(arm, out raycastHit, distance)) {
                //    Debug.Log("ray");
                selectableObject = raycastHit.collider.gameObject.GetComponent<ISelectable>();


                if (selectableObject is not null) {
                    //   Debug.Log("not null");
                    if (selectableObject != prefSelectableObject) {
                        //     Debug.Log("!=");

                        selectableObject.Selected();
                        prefSelectableObject?.Deselected();
                        //  Debug.Log(prefSelectableObject);
                        if (prefSelectableObject is not null) Debug.Log("deselected 1");
                    }
                }
                else {
                    //     Debug.Log("null");
                    prefSelectableObject?.Deselected();
                    if (prefSelectableObject is not null) Debug.Log("deselected 2");
                }
                prefSelectableObject = selectableObject;
            }
            else {
                // Debug.Log("not ray");
                prefSelectableObject?.Deselected();
                if (prefSelectableObject is not null) Debug.Log("deselected 3");
                selectableObject = null;
                prefSelectableObject = null;
            }

            if (Input.GetKeyDown(KeyCode.S)) {
                EndTurn();
            }

            if (Input.GetMouseButtonDown(0)) {
                if (selectableObject is LidController lidController && !lidOpened) {
                    selectableObject.Clicked();

                    if (lidController.isEmpty) {
                        lid = lidController;
                        lidOpened = true;
                        lidColor = lidController.color;
                        lidPos = lidController.pos;
                        selectableObject = null;
                        prefSelectableObject = null;
                       /* if(leftCard.color == lidColor || rightCard.color == lidColor) { }
                        else {
                            lid.photonView.RPC("PlayAnimation", RpcTarget.AllBuffered, "Closed");
                            lidOpened = false;
                        }*/
                    }

                    else {
                        GetStick();
                    }
                }

                else if(selectableObject is CardController cardController) {
                    if (lidOpened) {
                        bool isLeft = cardController.Equals(leftCard);
                        bool isRight = cardController.Equals(rightCard);
                        if (cardController.color == lidColor && ( isLeft || isRight)) {
                            prefSelectableObject = null;
                            Destroy(cardController.gameObject);
                            PutCard(lidColor, isRight);
                        }
                    }

                    else {
                        if (!cardController.isFlipped) {
                            cardController.Flip();
                        }
                    }
                }
            }
        }
    }

    private void PutCard(string color, bool isRight) {
        if (isRight) {
            vampireCardsList.RemoveAt(cardNum-1);
        }
        else {
            vampireCardsList.RemoveAt(0);
        }
        cardNum--;
        GameObject card = null;
        card = PhotonNetwork.Instantiate(cardPrafab.name, lidPos, new Quaternion(0, 0, 0, 0));
        card.GetComponentInChildren<CardController>().photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, color);
        card.GetComponentInChildren<CardController>().photonView.RPC("SetAnimatorController", RpcTarget.AllBuffered);
        lid.isEmpty = false;
        EndTurn();
    }

    private void GetStick() {
        if(sticksNum < 2) {
            sticksNum++;
        }

        else {
            sticksNum = 0;
        }
    }

    [PunRPC]
    private void ChangeTurn() {
        if (PhotonNetwork.IsMasterClient) {
            gameManager.ChangeTurn();
        }

        else {
            photonView.RPC("ChangeTurn", RpcTarget.MasterClient);
        }
    }

    public void EndTurn() {
        isMyTurn = false;
        lidOpened = false;
        if (lid == null) {
            Debug.Log(PhotonNetwork.NickName);
            Debug.Log("Lid Null");
        }
        lid.photonView.RPC("PlayAnimation", RpcTarget.AllBuffered, "Closed");
        if (cardNum == 0) {

        }
        else {
            leftCard = vampireCardsList[0];
            rightCard = vampireCardsList[cardNum - 1];
            ChangeTurn();
        }
    }

    [PunRPC]
    public void SetCards(string[] cardsColors) {
        vampireCardsList = new List<CardController>(cardsColors.Length);
        cardNum = cardsColors.Length;
        float x = -cardsColors.Length / 4 + 0.5f;
        float y = -0.8f;
        float z = -4.3f;
        Material material = null;
        string cardColor = "black";
        GameObject card;
        CardController cardController;
        foreach (string color in cardsColors) {
            cardColor = color;
            card = Instantiate(cardPrafab, new Vector3(x, y, z), new Quaternion(0, 0, 0, 0));
            cardController = card.GetComponentInChildren<CardController>();
            cardController.OnInstantinated(cardColor);
            vampireCardsList.Add(cardController);
            x += 0.5f;
            Debug.Log("SetCards");
        }

        leftCard = vampireCardsList[0];
        rightCard = vampireCardsList[cardNum - 1];
    }

    [PunRPC]
    public void SetTurn() {
        gameInf.photonView.RPC("SetTurn", RpcTarget.AllBuffered);
        isMyTurn = true;
    }

    [PunRPC]
    public void SetGameInformation(int viewID) {
        gameInf = PhotonView.Find(viewID).gameObject.GetComponent<GameInformation>();
    }
}
