using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class GameUnit : MonoBehaviourPunCallbacks , ISelectable
{
    protected MeshRenderer meshRenderer;
    protected Color materialColor;
    protected Animator animator;
    public string color { get; protected set; }
    public GameManager gameManager { get; set; }
    public delegate void AnimationState(string trigger);
    public event AnimationState animationEnded;

    private void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        animator = GetComponent<Animator>();
    }


    [PunRPC]
    public abstract void OnInstantinated(string color, int viewId = 0);

    public void Selected() {
        //animator.SetTrigger("Selected");
        meshRenderer.material.color = Color.gray;
    }

    public void Deselected() {
        //animator.SetTrigger("Deselected");
        meshRenderer.material.color = materialColor;
    }
    public virtual void Interact(int playerId) { }
    [PunRPC]
    public void PlayAnimation(string trigger) {
        animator.SetTrigger(trigger);
    }

    public virtual void AnimationEnded() {
        animationEnded("Closed");
    }

}
