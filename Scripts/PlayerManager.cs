using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System;
using Photon.Realtime;
using UnityEngine.UI;
//using UnityEditor.Animations;

public class PlayerManager : MonoBehaviourPunCallbacks {
    GameObject garlicCardPrafab;
    GameObject vampireCardPrefab;
    Ray arm;
    RaycastHit raycastHit;
    ISelectable selectableObject;
    ISelectable prefSelectableObject;
    float distance = 500f;
   // GameObject lid;
    public string lidColor;
    string garlicColor;
    bool lidOpened = false;
    public Vector3 lidPos;
    RuntimeAnimatorController animatorController;
    int cardNum;
    public LidController lid;
    int sticksNum = 0;
    CardController rightCard;
    CardController leftCard;

    //public bool isMyTurn { get; set; } = false;
    public bool isMyTurn { get; set; } = false;
    public bool needChooseVampire { get; set; } = false;

    [SerializeField] public PhotonView view;
    List<CardController> vampireCardsList;
    public GameManager gameManager { get; set; }
    public GameInformation gameInf;

    private void Start() {
        //view = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient) {
            isMyTurn = true;
        }
        arm = new Ray();
        animatorController = Resources.Load("Animation/Card", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
        vampireCardPrefab = Resources.Load("VampireCardPrefab", typeof(GameObject)) as GameObject;
        garlicCardPrafab = Resources.Load("GarlicCardPrefab", typeof(GameObject)) as GameObject;
    }

    void Update() {

        //arm.origin = Camera.main.transform.position;
        //arm.direction = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000);
        if (isMyTurn && photonView.IsMine) {
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
                if (lidOpened) {
                    EndTurn();
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                if (selectableObject is LidController lidController && !lidOpened) {
                    lid = lidController;
                   // lid.LidOpened += OnLidOpened;
                   // gameManager.OpenLid(lid);
                    lidController.Clicked();

                    //selectableObject = null;
                  //  prefSelectableObject = null;
                }

                else if (selectableObject is CardController cardController) {
                    if (lidOpened) {
                        if (selectableObject is VampireCard) {
                            bool isLeft = cardController.Equals(leftCard);
                            bool isRight = cardController.Equals(rightCard);
                            if (cardController.color == lidColor && (isLeft || isRight)) {
                                prefSelectableObject = null;
                                Destroy(cardController.gameObject);
                                PutVampireCard(lidColor, isRight);
                            }
                        }

                        else {
                            PutGarlicCard();
                        }
                    }

                    else {
                        if (!cardController.isFlipped) {
                            cardController.Flip();
                        }
                    }
                }
            }

            if (needChooseVampire) {
                if (Input.GetMouseButtonDown(0)) {
                    if (selectableObject is VampireCard vampireCard) {
                        GiveCard(vampireCard.color);
                    }
                }
            }
        }
    }

    private void PutVampireCard(string color, bool isRight) {
        if (isRight) {
            vampireCardsList.RemoveAt(cardNum-1);
        }
        else {
            vampireCardsList.RemoveAt(0);
        }
        cardNum--;
        CardController card = null;
        card = PhotonNetwork.Instantiate(vampireCardPrefab.name, lidPos, new Quaternion(0, 0, 0, 0)).GetComponentInChildren<CardController>();
        card.photonView.RPC("OffCollider", RpcTarget.All);
        card.photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, color, (float)(1 / 0.6));
        card.photonView.RPC("SetAnimatorController", RpcTarget.AllBuffered);
        lid.photonView.RPC("PutCard", RpcTarget.All, card.gameObject.GetPhotonView().ViewID);
        EndTurn();
    }

    private void PutGarlicCard() {
        CardController card = null;
        card = PhotonNetwork.Instantiate(garlicCardPrafab.name, lidPos, new Quaternion(0, 0, 0, 0)).GetComponentInChildren<CardController>();
        card.photonView.RPC("OffCollider", RpcTarget.All);
        card.photonView.RPC("OnInstantinated", RpcTarget.AllBuffered, garlicColor, 1.2f);
        card.photonView.RPC("SetAnimatorController", RpcTarget.AllBuffered);
        lid.photonView.RPC("PutCard", RpcTarget.All, card.gameObject.GetPhotonView().ViewID);
        EndTurn();
    }

    private void GiveCard(string color) {
        gameManager.GiveCard(color);
    }

    private void TakeCard(GameObject card) {
        Instantiate(card);

    }

   /* private void OnLidOpened() {
        lid.LidOpened -= OnLidOpened;

        if (lid.isEmpty) {
            EmptyLidOpened();
        }   

        else {
            NotEmptyLidOpened();
        }
    }*/

    private void SetLidInformation() {
        lidOpened = true;
        lidColor = lid.color;
        lidPos = lid.pos;
    }

    /*private void EmptyLidOpened() {
        lidOpened = true;
        lidColor = lid.color;
        lidPos = lid.pos;
        selectableObject = null;
        prefSelectableObject = null;
    }*/

    /*private void NotEmptyLidOpened() {
        CardController card = lid.card;

        if(card is VampireCard vampireCard) {
            GetStick();
        }

        else if(card is GarlicCard garlicCard){
            if(garlicCard.color == garlicColor) {

            }

            else {

            }

            lid.photonView.RPC("TakeCard", RpcTarget.All);
            gameManager.DestroyCard(card.gameObject);
        }
    }*/

    private void GetStick() {
        if(sticksNum < 2) {
            sticksNum++;
        }

        else {
            sticksNum = 0;
        }
    }

    private void ChangeTurn() {
        gameManager.ChangeTurn();
    }

    [PunRPC]
    public void SetCards(string[] cardsColors, string garlicColor) {
        this.garlicColor = garlicColor;
        vampireCardsList = new List<CardController>(cardsColors.Length);
        cardNum = cardsColors.Length;
        float x = -(cardsColors.Length+3) / 4 + 0.5f;
        float y = -0.8f;
        float z = -4.3f;
        Material material = null;
        string cardColor = null;
        GameObject card;
        CardController cardController;

        foreach (string color in cardsColors) {
            cardColor = color;
            card = Instantiate(vampireCardPrefab, new Vector3(x, y, z), new Quaternion(0, 0, 0, 0));
            cardController = card.GetComponentInChildren<CardController>();
            cardController.OnInstantinated(cardColor,1f);
            vampireCardsList.Add(cardController);
            x += 0.5f;
        }

        for(int i = 0; i<3; i++) {
            GameObject garlic = Instantiate(garlicCardPrafab, new Vector3(x, y, z), new Quaternion(0, 0, 0, 0));
            garlic.GetComponentInChildren<CardController>().OnInstantinated(garlicColor, 0.6f);
            x += 0.5f;
        }

        leftCard = vampireCardsList[0];
        rightCard = vampireCardsList[cardNum - 1];
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
    public void SetTurn() {
        //gameManager.gameInf.photonView.RPC("SetTurn", RpcTarget.AllBuffered);
        if(gameInf == null) {
            Debug.Log("null gameInf");
        }
        gameInf.GetComponent<GameInformation>().photonView.RPC("SetTurn", RpcTarget.All);
        isMyTurn = true;
    }

    [PunRPC]
    public void SetGameInformation(int viewID) {
        gameInf = PhotonView.Find(viewID).gameObject.GetComponent<GameInformation>();
    }

    [PunRPC]
    public void SetGameManager(int viewID) {
        gameManager = PhotonView.Find(viewID).gameObject.GetComponent<GameManager>();
    }
}
