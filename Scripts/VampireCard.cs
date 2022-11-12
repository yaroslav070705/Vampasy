using UnityEngine;
using Photon.Pun;

public class VampireCard : GameUnit
{
    public override void Interact(int playerId) {
        gameManager.Interact(this, playerId);
    }

    [PunRPC]
    public override void OnInstantinated(string color, int viewId = 0) {
       // transform.parent.transform.localScale = new Vector3(scale, scale, scale);
        Debug.Log("VampireInstantinated");
        meshRenderer.material = Resources.Load($"Materials/{color}Vampire", typeof(Material)) as Material;
        this.color = color;
        materialColor = meshRenderer.material.color;
    }

}
