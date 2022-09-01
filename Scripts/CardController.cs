using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class CardController : MonoBehaviourPunCallbacks, ISelectable {
    protected MeshRenderer meshRenderer;
    protected Color materialColor;
    [SerializeField] protected Animator animator;
    public string color { get; set; }
    public bool isFlipped { get; set; } = false;
    public GameManager gameManager { get; set; }

    private void Awake() {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    [PunRPC]
    public abstract void OnInstantinated(string color, float scale);

    public void Flip() {
        PlayAnimation("Flip");
    }

    public void Selected () {
        //animator.SetTrigger("Selected");
        meshRenderer.material.color = Color.gray;
    }

    public void Deselected() {
        //animator.SetTrigger("Deselected");
        meshRenderer.material.color = materialColor;
    }

    public void Clicked() {
        animator.SetTrigger("Clicked");
    }

    [PunRPC]
    public void OffCollider() {
        gameObject.GetComponent<Collider>().enabled = false;
    }

    [PunRPC]
    public void PlayAnimation(string trigger) {
        animator.SetTrigger(trigger);
    }

    [PunRPC]
    public void SetAnimatorController() {
        animator.runtimeAnimatorController = Resources.Load("Animation/Card", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
    }
}

