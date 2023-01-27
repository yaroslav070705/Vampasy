using UnityEngine;
using Photon.Pun;

public class GarlicCard : GameUnit
{
    public int ownerId { get; set; }
    public override void Interact(int playerId) {
        gameManager.Interact(this, playerId);
    }

    [PunRPC]
    public override void OnInstantinated(string color, int viewId = 0) {
       // transform.parent.transform.localScale = new Vector3(scale, scale, scale);
        meshRenderer.material = Resources.Load($"Materials/{color}Garlic", typeof(Material)) as Material;
        this.color = color;
        materialColor = meshRenderer.material.color;
    }
}
