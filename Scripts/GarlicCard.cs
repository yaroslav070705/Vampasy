using UnityEngine;
using Photon.Pun;

public class GarlicCard : CardController
{
    [PunRPC]
    public override void OnInstantinated(string color, float scale) {
        transform.parent.transform.localScale = new Vector3(scale, scale, scale);
        meshRenderer.material = Resources.Load($"Materials/{color}Garlic", typeof(Material)) as Material;
        this.color = color;
        materialColor = meshRenderer.material.color;
    }
}
