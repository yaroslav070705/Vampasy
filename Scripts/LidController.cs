using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LidController : MonoBehaviourPunCallbacks, ISelectable
{
    [SerializeField] Animator animator;
    [SerializeField] PhotonView view;
    MeshRenderer meshRenderer;

    Color materialColor;
    public string color { get; set; }
    public Vector3 pos { get; set; }
    public bool isEmpty { get; set; } = true;

    private void Start() {
        pos = transform.parent.transform.position + transform.localPosition;   
    }

    [PunRPC]
    public void OnInstantinated(string color) {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load($"Materials/{color}Lid", typeof(Material)) as Material;
        materialColor = meshRenderer.material.color;
        this.color = color;
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
