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
        position = gameObject.transform.position;
        meshRenderer.material = Resources.Load($"Materials/{color}Plate", typeof(Material)) as Material;
        gameManager = PhotonView.Find(viewId).GetComponent<GameManager>();
        materialColor = meshRenderer.material.color;
        this.color = color;
    }
    public override void Interact(int playerId) {
        gameManager.Interact(this, playerId);
    }

   /* [PunRPC]
    public void PutCard(int cardId) {
        card = PhotonView.Find(cardId).GetComponent<GameUnit>();
        isEmpty = false;
    }*/

    public void AnimationEnded() {
       // card.animationEnded -= PlayAnimation;
    }
}
