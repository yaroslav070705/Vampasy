using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Plate : GameUnit
{
    public bool isEmpty { get; set; } = true;
    public GameUnit card { get; set; }
    public Vector3 position { get; set; }

    [PunRPC]
    public override void OnInstantinated(string color, int viewId = 0) {
        meshRenderer.material = Resources.Load($"Materials/{color}Plate", typeof(Material)) as Material;
        gameManager = PhotonView.Find(viewId).GetComponent<GameManager>();
        transform.parent.SetParent(gameManager.GameField.gameObject.transform);
        position = gameObject.transform.position;
        materialColor = meshRenderer.material.color;
        gameManager.GameField.rotationEnded += UpdatePosition;
        this.color = color;
    }
    public override void Interact(int playerId) {
        gameManager.Interact(this, playerId);
    }

    public void UpdatePosition() {
        position = gameObject.transform.position;
    }
   /* [PunRPC]
    public void PutCard(int cardId) {
        card = PhotonView.Find(cardId).GetComponent<GameUnit>();
        isEmpty = false;
    }*/

    public override void AnimationEnded() {
        //card.animationEnded -= PlayAnimation;
    }
}
