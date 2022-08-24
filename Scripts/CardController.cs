using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardController : MonoBehaviourPunCallbacks, ISelectable {
    Material redMaterial;
    Material blueMaterial;
    Material greenMaterial;
    Material whiteMaterial;
    Material blackMaterial;
    Material orangeMaterial;
    MeshRenderer meshRenderer;
    Color ccolor;
    [SerializeField] Animator animator;
    public string color { get; set; }
    public bool isFlipped { get; set; } = false;

    private void Awake() {
       /* redMaterial = Resources.Load("Materials/RedCard", typeof(Material)) as Material;
        blueMaterial = Resources.Load("Materials/BlueCard", typeof(Material)) as Material;
        greenMaterial = Resources.Load("Materials/GreenCard", typeof(Material)) as Material;
        whiteMaterial = Resources.Load("Materials/WhiteCard", typeof(Material)) as Material;
        blackMaterial = Resources.Load("Materials/BlackCard", typeof(Material)) as Material;
        orangeMaterial = Resources.Load("Materials/OrangeCard", typeof(Material)) as Material;*/
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    [PunRPC]
    public void OnInstantinated(string color) {
      /*  switch (color) {
            case "red":
                meshRenderer.material = redMaterial;
                goto default;

            case "blue":
                meshRenderer.material = blueMaterial;
                goto default;

            case "green":
                meshRenderer.material = greenMaterial;
                goto default;

            case "white":
                meshRenderer.material = whiteMaterial;
                goto default;

            case "black":
                meshRenderer.material = blackMaterial;
                goto default;

            case "orange":
                meshRenderer.material = orangeMaterial;
                goto default;

            default:
                break;
        }*/
        meshRenderer.material = Resources.Load($"Materials/{color}Card", typeof(Material)) as Material;
        this.color = color;
        ccolor = meshRenderer.material.color;
    }

    public void Flip() {
        PlayAnimation("Flip");
    }

    public void SetAtributesUnactive() {
        animator.enabled = false;
    }

    public void Selected () {
        //animator.SetTrigger("Selected");
        meshRenderer.material.color = Color.gray;
    }

    public void Deselected() {
        //animator.SetTrigger("Deselected");
        meshRenderer.material.color = ccolor;
    }

    public void Clicked() {
        animator.SetTrigger("Clicked");
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

