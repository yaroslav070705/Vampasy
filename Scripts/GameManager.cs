using UnityEngine;
using Unity.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

public class GameManager : MonoBehaviourPunCallbacks {
    [SerializeField] GameObject garlicPutCardPrefab;
    [SerializeField] GameObject vampirePutCardPrefab;

    [SerializeField] GameObject garlicCardPrefab;
    [SerializeField] GameObject vampireCardPrefab;
    [SerializeField] GameObject platePrefab;
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject cardPlaceHolderPrefab;

    Player[] networkPlayersList;
    List<PlayerManager> gamePlayersList;
    string[] cardsListPlayersGave;
    PlayerManager playerInTurn;
    int playerInTurnIndex = -1;
    Plate openedPlate = null;
    int playersHaveToGiveCardsNum;
    CardPlaceHolder[] cardPlaceHolders;

    public delegate void GameState();
    public event GameState GameStarted;
    public event GameState TurnEnded;
    public delegate void State(bool result);
    public event State interactionResult;

    private void Start() {
        PlayerManager player;
        gamePlayersList = new List<PlayerManager>();
        playerPrefab = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, new Quaternion(0, 0, 0, 0));
        player = playerPrefab.GetComponent<PlayerManager>();
        player.gameManager = this;
        photonView.RPC("AddPlayer", RpcTarget.MasterClient, player.photonView.ViewID);

        cardPlaceHolders = new CardPlaceHolder[2];
        cardPlaceHolders[0] = Instantiate(cardPlaceHolderPrefab, Vector3.zero, new Quaternion(0,0,0,0)).GetComponent<CardPlaceHolder>();
        cardPlaceHolders[0].transform.Rotate(90f,0f,0f);
        cardPlaceHolders[0].side = "left";
        cardPlaceHolders[0].gameObject.SetActive(false);
        cardPlaceHolders[0].gameManager = this;
        cardPlaceHolders[1] = Instantiate(cardPlaceHolderPrefab, Vector3.zero, new Quaternion(0, 0, 0, 0)).GetComponent<CardPlaceHolder>();
        cardPlaceHolders[1].transform.Rotate(90f, 0f, 0f);
        cardPlaceHolders[1].side = "right";
        cardPlaceHolders[1].gameManager = this;
        cardPlaceHolders[1].gameObject.SetActive(false);
    }

    public void EnableToStartGame() {
        startGameButton.SetActive(true);
    }

    public void StartGame() {
        //GameStarted.Invoke();
        startGameButton.SetActive(false);
        photonView.RPC("SetNetworkPlayersList",RpcTarget.All);
        SetGamePLayersList();
        DistributeCards();
        SetPlates();
        photonView.RPC("ChangeTurn", RpcTarget.All);
    }
    private void FinishGame() {

    }

    private void DistributeCards() {
        Dictionary<int, string> colors = new Dictionary<int, string>() { { 0, "red" }, { 1, "blue" }, { 2, "green" }, { 3, "white" }, { 4, "black" }, { 5, "orange" } };
        List<string> garlicColors = new List<string>() { "Red", "Blue", "Green", "Orange", "Black", "White" };

        int redNum = 10;
        int blueNum = 10;
        int greenNum = 10;
        int whiteNum = 10;
        int blackNum = 10;
        int orangeNum = 10;

        List<int> keys;
        int key;
        int cardsNum = 60 / networkPlayersList.Length;
        for (int i = 0; i < networkPlayersList.Length; i++) {
            string[] cardsColors = new string[cardsNum];
            int index = Random.Range(0, garlicColors.Count);
            string garlicColor = garlicColors[index];
            garlicColors.RemoveAt(index);
            for (int j = 0; j < cardsNum; j++) {
                keys = colors.Keys.ToList();
                key = keys[Random.Range(0, keys.Count)];
                switch (colors[key]) {
                    case "red":
                        redNum--;
                        if (redNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Red";
                        break;

                    case "blue":
                        if (blueNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Blue";
                        break;

                    case "green":
                        if (greenNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Green";
                        break;

                    case "white":
                        if (whiteNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "White";
                        break;

                    case "black":
                        if (blackNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Black";
                        break;

                    case "orange":
                        if (orangeNum == 0) {
                            colors.Remove(key);
                        }
                        cardsColors[j] = "Orange";
                        break;
                }
            }

            Debug.Log(gamePlayersList.Count);
            photonView?.RPC("SetCards", networkPlayersList[i], cardsColors, garlicColor);
        }
    }

    [PunRPC]
    private void SetCards(string[] vampireCardsColors, string garlicColor) {
        PlayerManager player = playerPrefab.GetComponent<PlayerManager>();
        LinkedList<VampireCard> vampireCards = new LinkedList<VampireCard>();
        GarlicCard[] garlicCards = new GarlicCard[3];
        VampireCard vampireCard = null;
        GarlicCard garlicCard = null;
        float x = -(vampireCardsColors.Length + 3) / 4 + 0.5f;
        float y = -0.8f;
        float z = -4.3f;
        for (int i=0;i<vampireCardsColors.Length;i++) {
            vampireCard = Instantiate(vampireCardPrefab,new Vector3(x,y,z),new Quaternion(0,0,0,0)).GetComponentInChildren<VampireCard>();
            vampireCard.OnInstantinated(vampireCardsColors[i]);
            vampireCards.AddLast(vampireCard);
            vampireCard.gameManager = this;
            x += 0.5f;
        }
        z -= 2;
        for(int i = 0; i < 3; i++) {
            garlicCard = Instantiate(garlicCardPrefab, new Vector3(x,y,z), new Quaternion(0,0,0,0)).GetComponentInChildren<GarlicCard>();
            garlicCard.OnInstantinated(garlicColor);
            garlicCard.gameManager = this;
            garlicCards[i] = garlicCard;
            x += 0.5f;
        }
        player.vampireCardsList = vampireCards;
        player.garlicCardsList = garlicCards;
        player.garlicColor = garlicColor;
        player.rightVampireCard = vampireCards.Last.Value;
        player.leftVampireCard = vampireCards.First.Value;
        player.vampireCardsNum = vampireCards.Count;
    }

    private void SetPlates() {
        List<(float, float, float)> positions = new List<(float, float, float)>() {(3.55f,-0.778f,-2.39f),(5.17f,-0.778f,-2.39f),(6.8f,-0.778f,-2.39f),(8.43f,-0.778f,-2.39f),
                                                           (3.55f,-0.778f,-0.166f),(5.17f,-0.778f,-0.166f),(8.43f,-0.778f,-0.166f),
                                                           (3.55f,-0.778f,2.06f),(5.17f,-0.778f,2.06f),(6.8f,-0.778f,2.06f),(8.43f,-0.778f,2.06f),
                                                           (3.55f,-0.778f,4.28f),(5.17f,-0.778f,4.28f),(6.8f,-0.778f,4.28f),(8.43f,-0.778f,4.28f),
                                                           (6.8f,-0.778f,6.5f),(8.43f,-0.778f,6.5f),

                                                           (-1.33f,-0.778f,-2.39f),(-2.96f,-0.778f,-2.39f),(-6.21f,-0.778f,-2.39f),(-7.84f,-0.778f,-2.39f),
                                                           (-1.33f,-0.778f,-0.166f),(-2.96f,-0.778f,-0.166f),(-4.59f,-0.778f,-0.166f),(-6.21f,-0.778f,-0.166f),(-7.84f,-0.778f,-0.166f),
                                                           (-1.33f,-0.778f,2.06f),(-2.96f,-0.778f,2.06f),(-4.59f,-0.778f,2.06f),(-6.21f,-0.778f,2.06f),(-7.84f,-0.778f,2.06f),
                                                           (-1.33f,-0.778f,4.28f),(-2.96f,-0.778f,4.28f),

                                                           (-1.33f,-0.778f,8.725f),(-2.96f,-0.778f,8.725f),(-4.59f,-0.778f,8.725f),(-6.21f,-0.778f,8.725f),(-7.84f,-0.778f,8.725f),
                                                           (-1.33f,-0.778f,10.94f),(-2.96f,-0.778f,10.94f),(-6.21f,-0.778f,10.94f),(-7.84f,-0.778f,10.94f),
                                                           (-1.33f,-0.778f,13.17f),(-2.96f,-0.778f,13.17f),(-4.59f,-0.778f,13.17f),(-6.21f,-0.778f,13.17f),(-7.84f,-0.778f,13.17f),
                                                           (-6.21f,-0.778f,6.5f),(-7.84f,-0.778f,6.5f),

                                                           (1.925f,-0.778f,8.725f),
                                                           (1.925f,-0.778f,10.94f),(3.55f,-0.778f,10.94f),(5.17f,-0.778f,10.94f),(6.8f,-0.778f,10.94f),(8.43f,-0.778f,10.94f),
                                                           (1.925f,-0.778f,13.17f),(3.55f,-0.778f,13.17f),(5.17f,-0.778f,13.17f),(6.8f,-0.778f,13.17f),(8.43f,-0.778f,13.17f) };
        (float x, float y, float z) pos;
        string[] colors = { "Red", "Blue", "Green", "White", "Black", "Orange" };
        int ind = 0;
        Plate plate;

        for (int i = 0; i < 54; i++) {
            ind = Random.Range(0, positions.Count);
            pos = positions[ind];
            plate = PhotonNetwork.Instantiate(platePrefab.name, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<Plate>();
            plate.photonView.RPC("OnInstantinated", RpcTarget.All, colors[i / 9], photonView.ViewID);
            positions.RemoveAt(ind);
        }
    }

    public void Interact(VampireCard vampireCard, int playerId) {
        if (playerId == playerInTurn.photonView.ViewID) {

            if (openedPlate != null) {

                if (openedPlate.isEmpty) {
                    bool isRight = false;
                    bool isLeft = false;
                    if(vampireCard == playerInTurn.rightVampireCard) {
                        isRight = true;
                    }
                    else if(vampireCard == playerInTurn.leftVampireCard) {
                        isLeft = true;
                    }
                    if (isRight || isLeft) {
                        if (vampireCard.color == openedPlate.color) {
                            PutCard(vampireCard);
                            if (isLeft) {
                                cardPlaceHolders[0].transform.position = new Vector3(cardPlaceHolders[0].transform.position.x + 0.5f, cardPlaceHolders[0].transform.position.y, cardPlaceHolders[0].transform.position.z);
                            }
                            else {
                                cardPlaceHolders[1].transform.position = new Vector3(cardPlaceHolders[1].transform.position.x - 0.5f, cardPlaceHolders[1].transform.position.y, cardPlaceHolders[1].transform.position.z);
                            }
                            return;
                        }
                        else {

                        }
                    }
                    else {

                    }
                }
                if (playerInTurn.haveToTakeCardsNum != 0) {
                    if (playerInTurn.vampiresCardsPlayerHaveToTake.Contains<VampireCard>(vampireCard)) {
                        playerInTurn.selectedVampireCard = vampireCard;
                        return;
                    }
                }
            }
            playerInTurn.selectedVampireCard = null;
        }
        else {
            PlayerManager player = PhotonView.Find(playerId).GetComponent<PlayerManager>();
            if (player.haveToGiveCardsNum != 0) {
                if (vampireCard == player.rightVampireCard || vampireCard == player.leftVampireCard) {
                    interactionResult(true);
                    Destroy(vampireCard.gameObject);
                    photonView.RPC("TakeCardFromPlayer", networkPlayersList[playerInTurnIndex], vampireCard.color);
                }
            }
        }
    }
    public void Interact(GarlicCard garlicCard, int playerId) {
        if (playerId == playerInTurn.photonView.ViewID) {

            if (openedPlate != null) {
                if (openedPlate.isEmpty) {
                    PutCard(garlicCard);
                }
            }
            else {

            }
        }


    }
    public void Interact(Plate plate, int playerId) {
        if (playerId == playerInTurn.photonView.ViewID) {
            Debug.Log("plate");
            if(openedPlate == null) {
                openedPlate = plate;
                plate.photonView.RPC("PlayAnimation", RpcTarget.All, "Opened");
                if (openedPlate.isEmpty == false) {

                    if(openedPlate.card is VampireCard) {

                        if (GiveStic()){
                            EndTurn();
                        }
                        else {
                            SetPlayersHaveToGiveCards(1);
                        }
                    }
                    else {

                        if(openedPlate.card.color == playerInTurn.garlicColor) {
                            SetPlayersHaveToGiveCards(2);
                        }
                        else {
                            Debug.Log("Garlic opened");
                            SetPlayersHaveToGiveCards(1);
                        }
                    }
                }
            }
        }
    }
    public void Interact(CardPlaceHolder placeHolder, int playerId) {
        Vector3 cardHolderPosition = placeHolder.transform.position;
        if (playerId == playerInTurn.photonView.ViewID) {
            if (playerInTurn.selectedVampireCard) {
                if(placeHolder.side == "right") {
                    playerInTurn.vampireCardsList.AddLast(playerInTurn.selectedVampireCard);
                    placeHolder.transform.position = new Vector3(placeHolder.transform.position.x+0.5f, placeHolder.transform.position.y, placeHolder.transform.position.z);
                }
                else {
                    playerInTurn.vampireCardsList.AddFirst(playerInTurn.selectedVampireCard);
                    placeHolder.transform.position = new Vector3(placeHolder.transform.position.x - 0.5f, placeHolder.transform.position.y, placeHolder.transform.position.z);
                }
                playerInTurn.selectedVampireCard.transform.parent.transform.position = cardHolderPosition;
                playerInTurn.selectedVampireCard = null;
                if(--playerInTurn.haveToTakeCardsNum == 0) {
                    foreach(CardPlaceHolder holder in cardPlaceHolders) {
                        holder.gameObject.SetActive(false);
                    }
                }
            }
        }
 
    }
    private void PutCard(GameUnit card) {
        interactionResult(true);
        string color = card.color;
        if (card is VampireCard ) {
            Destroy(card.gameObject);
            card = PhotonNetwork.Instantiate(vampirePutCardPrefab.name, new Vector3(openedPlate.position.x, openedPlate.position.y + 0.66f, openedPlate.position.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<GameUnit>();
        }
        else {
            Destroy(card.gameObject);
            card = PhotonNetwork.Instantiate(garlicPutCardPrefab.name, new Vector3(openedPlate.position.x, openedPlate.position.y+0.66f,openedPlate.position.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<GameUnit>();
        }
        { //openedPlate.photonView.RPC("PutCard", RpcTarget.All,card.photonView.ViewID);
          //card.photonView.RPC("OnInstantinated", RpcTarget.All, color,1);
          //card.animationEnded += openedPlate.PlayAnimation;
          // card.photonView.RPC("PlayAnimation", RpcTarget.All, "Put");
        }
        photonView.RPC("PutCardSync", RpcTarget.All, openedPlate.photonView.ViewID ,card.photonView.ViewID,color);
        EndTurn();

    }
    [PunRPC]
    private void PutCardSync(int plateId, int cardId, string color) {
        GameUnit card = PhotonView.Find(cardId).GetComponent<GameUnit>();
        Plate openedPlate = PhotonView.Find(plateId).GetComponent<Plate>();
        openedPlate.card = card;
        openedPlate.isEmpty = false;
        card.OnInstantinated(color);
        // card.animationEnded += openedPlate.PlayAnimation;
        card.animationEnded += EndTurn;
        //card.photonView.RPC("PlayAnimation", RpcTarget.All, "Put");
        card.PlayAnimation("Put");
    }
    
    private void GiveCardsToPlayer() {
        Debug.Log("GiveCardsToPlayer");
        float x = -(60/networkPlayersList.Length + 3) / 4;
        float y = -0.8f;
        float z = -4.3f;
        cardPlaceHolders[0].transform.position = new Vector3(x,y,z);
        cardPlaceHolders[1].transform.position = new Vector3(x+(0.5f* 60/networkPlayersList.Length+1)-0.5f, y,z);
        cardPlaceHolders[0].gameObject.SetActive(true);
        cardPlaceHolders[1].gameObject.SetActive(true);
        VampireCard[] cards = new VampireCard[cardsListPlayersGave.Length];
        for(int i=0;i<cardsListPlayersGave.Length;i++) {
            cards[i] = Instantiate(vampireCardPrefab, new Vector3(x+10,y,z+5), new Quaternion(0,0,0,0)).GetComponentInChildren<VampireCard>();
            cards[i].gameManager = this;
            cards[i].OnInstantinated(cardsListPlayersGave[i]);
            x += 1;
        }
        playerInTurn.haveToTakeCardsNum = cardsListPlayersGave.Length;
        playerInTurn.vampiresCardsPlayerHaveToTake = cards;

    }
    [PunRPC]
    public void TakeCardFromPlayer(string color) {
        cardsListPlayersGave[playersHaveToGiveCardsNum-1] = color;
        playersHaveToGiveCardsNum--;
        if(playersHaveToGiveCardsNum == 0) {
            GiveCardsToPlayer();
        }
    }
    private void SetPlayersHaveToGiveCards(int cardsNum) {
        for (int i = 0; i < networkPlayersList.Length; i++) {
            if (gamePlayersList[i] != playerInTurn) {
                gamePlayersList[i].photonView.RPC("HaveToGiveCards", networkPlayersList[i], cardsNum);
            }
        }
        playersHaveToGiveCardsNum = (networkPlayersList.Length-1)* cardsNum;
        cardsListPlayersGave = new string[playersHaveToGiveCardsNum];
    }
    private bool GiveStic() { 
        if(playerInTurn.sticksNum < 2) {
            playerInTurn.sticksNum++;
            return true;
        }
        else {
            playerInTurn.sticksNum = 0;
            return false;
        }
    }

    public void EndTurn(PlayerManager player) {
        if(player == playerInTurn) {
            if(playerInTurn.haveToTakeCardsNum == 0) {
                EndTurn();
            }
        }

    }
    public void EndTurn() {
        openedPlate.PlayAnimation("Closed");
        //photonView.RPC("ChangeTurn",RpcTarget.All);
        ChangeTurn();
        openedPlate = null;
    }
    public void ChangeTurn(int playerId) {
        photonView.RPC("ChangeTurn", RpcTarget.All);
    }
    [PunRPC]
    private void ChangeTurn() {
        if (playerInTurnIndex == networkPlayersList.Length - 1) {
            playerInTurnIndex = 0;
        }
        else {
            playerInTurnIndex++;
        }
        Debug.Log(gamePlayersList.Count);
        playerInTurn = gamePlayersList[playerInTurnIndex];
    }
    [PunRPC]
    private void SetNetworkPlayersList() {
        networkPlayersList = PhotonNetwork.PlayerList;
    }
    private void SetGamePLayersList() {
        foreach (PlayerManager player in gamePlayersList) {
            photonView.RPC("AddPlayer", RpcTarget.Others, player.photonView.ViewID);
        }
    }
    [PunRPC]
    private void AddPlayer(int playerId) {
        gamePlayersList.Add(PhotonView.Find(playerId).GetComponent<PlayerManager>());
    }

}
