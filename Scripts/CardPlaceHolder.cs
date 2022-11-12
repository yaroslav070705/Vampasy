using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardPlaceHolder : MonoBehaviourPunCallbacks, ISelectable {

    MeshRenderer meshRenderer;
    Color materialColor;
    public GameManager gameManager { get; set; }
    public string side { get; set; }

    private void Start() {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        materialColor = meshRenderer.material.color;
    }

    public void Interact (int playerId) {
        gameManager.Interact(this, playerId);
    }

    public void Selected() {
        meshRenderer.material.color = Color.grey;
    }

    public void Deselected() {
        meshRenderer.material.color=materialColor;
    }
}
