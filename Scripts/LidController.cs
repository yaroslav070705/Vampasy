using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Threading;

public class LidController : MonoBehaviourPunCallbacks, ISelectable
{
    [SerializeField] Animator animator;
    [SerializeField] PhotonView view;
    MeshRenderer meshRenderer;

    Color materialColor;

    public delegate void LidStateHandler();
    public event LidStateHandler LidOpened;
    public string color { get; set; }
    public Vector3 pos { get; set; }
    public bool isEmpty { get; set; } = true;
    public GameManager gameManager { get; set; }

    public CardController card;

    private void Start() {
        pos = transform.parent.transform.position + transform.localPosition;   
    }

    private void OnLidOpened() {
        //  LidOpened?.Invoke();
        gameManager.OnLidOpened(this);
    }

    [PunRPC]
    public void OnInstantinated(string color, int viewID) {
        gameManager = PhotonView.Find(viewID).GetComponent<GameManager>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load($"Materials/{color}Lid", typeof(Material)) as Material;
        materialColor = meshRenderer.material.color;
        this.color = color;
    }

    [PunRPC]
    public void PutCard(int viewID) {
        card = PhotonView.Find(viewID).GetComponent<CardController>();
        isEmpty = false;
    }

    [PunRPC]
    public void TakeCard() {
        card = null;
        isEmpty = true;
    }

    public void Selected() {
        //view.RPC("PlayAnimation", RpcTarget.AllBuffered, "Selected");
        meshRenderer.material.color = Color.magenta;
       //transform.localScale = new Vector3(120,120,120);
    }

    public void Deselected() {
        //view.RPC("PlayAnimation", RpcTarget.AllBuffered, "Deselected");
        meshRenderer.material.color = materialColor;
        //transform.localScale = new Vector3(100, 100, 100);
    }

    public void Clicked() {
        view.RPC("PlayAnimation", RpcTarget.AllBuffered, "Opened");
        Debug.Log("Click");
    }

    [PunRPC]
    public void PlayAnimation(string text) {
        animator.SetTrigger(text);
        Debug.Log(text);
    }
}
