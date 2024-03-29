using UnityEngine;
using Unity.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

public class GameManager : MonoBehaviourPunCallbacks {
    [SerializeField] GameObject garlicPutCardPrefab;
    [SerializeField] GameObject vampirePutCardPrefab;

    [SerializeField] GameObject garlicCardPrefab;
    [SerializeField] GameObject vampireCardPrefab;
    [SerializeField] GameObject platePrefab;
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject cardPlaceHolderPrefab;
    [SerializeField] GameInformationManager gameInformationManager;
    [SerializeField] GameObject playerInformationManagerPrefab;
    [SerializeField] Canvas canvas;
    [SerializeField] GameFieldManager gameField;
    public GameFieldManager GameField { get { return gameField; } }

    Player[] networkPlayersList;
    List<PlayerManager> gamePlayersList;
    string[] cardsListPlayersGave;
    PlayerManager playerInTurn;
    int playerInTurnIndex = -1;
    Plate openedPlate = null;
    int playersHaveToGiveCardsNum;
    CardPlaceHolder[] cardPlaceHolders;

    float cardWidth = 0.5f;

    public delegate void GameState();
    public event GameState GameStarted;
    public event GameState TurnEnded;
    public delegate void State(bool result);
    public event State interactionResult;

    private void Start() {
        if (PhotonNetwork.IsMasterClient) {
            startGameButton.SetActive(true);
        }
        PlayerManager player;
        PlayerInformationManager playerInformation = PhotonNetwork.Instantiate(playerInformationManagerPrefab.name,new Vector3(0,0,0), new Quaternion(0,0,0,0)).GetComponent<PlayerInformationManager>();
        gamePlayersList = new List<PlayerManager>();
        playerPrefab = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, new Quaternion(0, 0, 0, 0));
        player = playerPrefab.GetComponent<PlayerManager>();
        player.gameManager = this;
        player.nickName = PhotonNetwork.NickName;
        photonView.RPC("AddPlayer", RpcTarget.MasterClient, player.photonView.ViewID, playerInformation.photonView.ViewID, player.nickName);
        gameField.gameManager = this;


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
        Debug.Log($"{networkPlayersList[playerInTurnIndex].NickName} �������");
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
        cardPlaceHolders[0].transform.position = new Vector3(x-0.5f, y, z);
        cardPlaceHolders[1].transform.position = new Vector3(x + (0.5f * 60 / networkPlayersList.Length + 1) - 1f, y, z);
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
            garlicCard.ownerId = player.photonView.ViewID;
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
        bool isRight = false;
        bool isLeft = false;
        if (playerId == playerInTurn.photonView.ViewID) {

            if (openedPlate != null) {

                if (openedPlate.isEmpty && playerInTurn.haveToTakeCards==false) {
                    if(vampireCard == playerInTurn.rightVampireCard) {
                        isRight = true;
                    }
                    else if(vampireCard == playerInTurn.leftVampireCard) {
                        isLeft = true;
                    }
                    if (isRight || isLeft) {
                        if (vampireCard.color == openedPlate.color) {
                            if (isLeft) {
                                UpdateCards("left", cardWidth,playerInTurn);
                            }
                            else {
                                UpdateCards("right", cardWidth,playerInTurn);
                            }
                            PutCard(vampireCard);
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
                if (vampireCard == player.rightVampireCard) {
                    isRight = true;
                }
                else if (vampireCard == player.leftVampireCard) {
                    isLeft = true;
                }
                if (isLeft || isRight) {
                    if (isRight) {
                        UpdateCards("right", cardWidth,player);
                    }
                    else {
                        UpdateCards("left", cardWidth,player);
                    }
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
                if (openedPlate.isEmpty && playerInTurn.haveToTakeCards==false) {
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
                            openedPlate.photonView.RPC("PlayAnimation", RpcTarget.All,"Closed");
                            EndTurn();
                        }
                        else {
                            SetPlayersHaveToGiveCards(1);
                        }
                    }
                    else {
                        playerInTurn.haveToTakeCards = true;
                        GarlicCard card = plate.card as GarlicCard;
                        PlayerManager player = PhotonView.Find(card.ownerId).GetComponent<PlayerManager>();
                        photonView.RPC("GiveGarlicCardToPlayer", networkPlayersList[gamePlayersList.IndexOf(player)], gamePlayersList.IndexOf(player), card.color);
                        photonView.RPC("TakeCardFromPlate",RpcTarget.All, openedPlate.photonView.ViewID, card.photonView.ViewID);
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
                    UpdateCards("right", -cardWidth, playerInTurn, playerInTurn.selectedVampireCard);
                }
                else {
                    UpdateCards("left", -cardWidth,playerInTurn,playerInTurn.selectedVampireCard);  
                }
                playerInTurn.selectedVampireCard.transform.parent.transform.position = cardHolderPosition;
                playerInTurn.selectedVampireCard = null;
                if(--playerInTurn.haveToTakeCardsNum == 0) {
                    playerInTurn.haveToTakeCards = false;
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
        int ownerId = 0;
        if (card is VampireCard ) {
            Destroy(card.gameObject);
            card = PhotonNetwork.Instantiate(vampirePutCardPrefab.name, new Vector3(openedPlate.position.x, openedPlate.position.y + 0.66f, openedPlate.position.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<GameUnit>();
        }
        else {
            ownerId = (card as GarlicCard).ownerId;
            Destroy(card.gameObject);
            Debug.Log("Garlic");
            card = PhotonNetwork.Instantiate(garlicPutCardPrefab.name, new Vector3(openedPlate.position.x, openedPlate.position.y+0.66f,openedPlate.position.z), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<GameUnit>();
        }
        { //openedPlate.photonView.RPC("PutCard", RpcTarget.All,card.photonView.ViewID);
          //card.photonView.RPC("OnInstantinated", RpcTarget.All, color,1);
          //card.animationEnded += openedPlate.PlayAnimation;
          // card.photonView.RPC("PlayAnimation", RpcTarget.All, "Put");
        }
        photonView.RPC("PutCardSync", RpcTarget.All,openedPlate.photonView.ViewID ,card.photonView.ViewID,color, ownerId);
        EndTurn();

    }
    [PunRPC]
    private void PutCardSync(int plateId, int cardId, string color, int ownerId=0) {
        GameUnit card = PhotonView.Find(cardId).GetComponent<GameUnit>();
        Plate openedPlate = PhotonView.Find(plateId).GetComponent<Plate>();
        if (ownerId != 0) {
            (card as GarlicCard).ownerId = ownerId;
        }
        openedPlate.card = card;
        openedPlate.isEmpty = false;
        card.transform.parent.SetParent(gameField.transform);
        card.OnInstantinated(color);
        card.animationEnded += openedPlate.PlayAnimation;
        card.PlayAnimation("Put");
    }
    [PunRPC]
    private void TakeCardFromPlate(int plateId, int cardId) {
        GarlicCard card = PhotonView.Find(cardId).GetComponent<GarlicCard>();
        Plate plate = PhotonView.Find(plateId).GetComponent<Plate>();
        plate.isEmpty = true;
        if(card is not null && PhotonNetwork.IsMasterClient) {
            PhotonNetwork.Destroy(card.photonView);
        }
    }
    
    private void GiveVampireCardsToPlayer() {
        Debug.Log("GiveCardsToPlayer");
        float x = -(60/networkPlayersList.Length + 3) / 4;
        float y = -0.8f;
        float z = -4.3f;

        cardPlaceHolders[0].gameObject.SetActive(true);
        cardPlaceHolders[1].gameObject.SetActive(true);
        VampireCard[] cards = new VampireCard[cardsListPlayersGave.Length];
        for(int i=0;i<cardsListPlayersGave.Length;i++) {
            cards[i] = Instantiate(vampireCardPrefab, new Vector3(x+10,y,z+5), new Quaternion(0,0,0,0)).GetComponentInChildren<VampireCard>();
            cards[i].gameManager = this;
            cards[i].OnInstantinated(cardsListPlayersGave[i]);
            x += 1;
        }
        playerInTurn.haveToTakeCardsNum += cardsListPlayersGave.Length;
        playerInTurn.vampiresCardsPlayerHaveToTake = cards;

    }
    [PunRPC]
    private void GiveGarlicCardToPlayer(int playerIndex,string color) {
        float zPos = -6.4f;
        float xPos = 5;
        float yPos = -0.8f;
        for (int i = 0; i < 3; i++) {
            if (gamePlayersList[playerIndex].garlicCardsList[i] == null) {
                GarlicCard card = Instantiate(garlicCardPrefab, new Vector3(xPos+i*0.5f,yPos,zPos), new Quaternion(0, 0, 0, 0)).GetComponentInChildren<GarlicCard>();
                card.gameManager = this;
                card.OnInstantinated(color);
            }
        }
    }

    [PunRPC]
    public void TakeCardFromPlayer(string color) {
        cardsListPlayersGave[playersHaveToGiveCardsNum-1] = color;
        playersHaveToGiveCardsNum--;
        if(playersHaveToGiveCardsNum == 0) {
            GiveVampireCardsToPlayer();
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

    private void UpdateCards(string side, float delta,PlayerManager player, VampireCard card = null) {
        if (side == "left") {
            if(card != null) {
                player.vampireCardsList.AddFirst(card);
            }
            else {
                player.vampireCardsList.RemoveFirst();
            }
            player.leftVampireCard = player.vampireCardsList.First.Value;
            cardPlaceHolders[0].transform.position = new Vector3(cardPlaceHolders[0].transform.position.x + delta, cardPlaceHolders[0].transform.position.y, cardPlaceHolders[0].transform.position.z);
        }
        else {
            if (card != null) {
                player.vampireCardsList.AddLast(card);
            }
            else {
                player.vampireCardsList.RemoveLast();
            }
            player.rightVampireCard = player.vampireCardsList.Last.Value;
            cardPlaceHolders[1].transform.position = new Vector3(cardPlaceHolders[1].transform.position.x - delta, cardPlaceHolders[1].transform.position.y, cardPlaceHolders[1].transform.position.z);
        }
        if(player != playerInTurn) {
            player.haveToGiveCardsNum--;
        }
    }

    public void EndTurn(PlayerManager player) {
        if(player == playerInTurn) {
            if(playerInTurn.haveToTakeCards==false && openedPlate!=null) {
                openedPlate.photonView.RPC("PlayAnimation",RpcTarget.All,"Closed");
                EndTurn();
            }
        }
    }

    [PunRPC]
    public void EndTurn() {
        Debug.Log("EndTurn");
        if (playerInTurn.photonView.IsMine) {
            openedPlate = null;
            //gameInformationManager.photonView.RPC("UpdatePlayerInformation", RpcTarget.All,playerInTurnIndex);
            gameInformationManager.UpdatePlayerInformation(playerInTurnIndex);
            if (playerInTurn.vampireCardsList.Count == 0) {
                FinishGame();
                return;
            }
            photonView.RPC ("ChangeTurn",RpcTarget.All);
        }
    }

    [PunRPC]
    private void ChangeTurn() {
        Debug.Log("ChangeTurn");
        if (playerInTurnIndex == networkPlayersList.Length - 1) {
            playerInTurnIndex = 0;
        }
        else {
            playerInTurnIndex++;
        }
        playerInTurn = gamePlayersList[playerInTurnIndex];
    }

    public void RotateField(int playerId) {
        if(playerId == playerInTurn.photonView.ViewID) {
            gameField.photonView.RPC("Rotate",RpcTarget.All);
        }
    }

    [PunRPC]
    private void SetNetworkPlayersList() {
        networkPlayersList = PhotonNetwork.PlayerList;
    }

    private void SetGamePLayersList() {
        for(int i = 0; i < gamePlayersList.Count; i++) {
            int playerId = gamePlayersList[i].photonView.ViewID;
            int playerInfId = gameInformationManager.playerInformationManagers[i].photonView.ViewID;
            string nickName = gamePlayersList[i].nickName;
            photonView.RPC("AddPlayer", RpcTarget.Others, playerId, playerInfId, nickName);
        }
    }
    [PunRPC]
    private void AddPlayer(int playerId, int playerInformationId,string nickName) {
        Debug.Log("add player");
        PlayerManager player = PhotonView.Find(playerId).GetComponent<PlayerManager>();
        player.nickName = nickName;
        gamePlayersList.Add(player);
        PlayerInformationManager pI = PhotonView.Find(playerInformationId).GetComponent<PlayerInformationManager>();
        gameInformationManager.playerInformationManagers.Add(pI);
        pI.transform.SetParent(gameInformationManager.transform);
        int index = gameInformationManager.playerInformationManagers.Count-1;
        pI.OnAdded(index);
        pI.SetInformation(nickName);
        if (pI.photonView.IsMine) {
            pI.player = player;
        }
       // pI. = PhotonNetwork.NickName;
        Debug.Log("Inf added to master");
    }

}
