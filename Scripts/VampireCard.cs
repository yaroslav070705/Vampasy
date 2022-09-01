using UnityEngine;
using Photon.Pun;

public class VampireCard : CardController
{

    [PunRPC]
    public override void OnInstantinated(string color, float scale) {
        transform.parent.transform.localScale = new Vector3(scale, scale, scale);
        Debug.Log("VampireInstantinated");
        meshRenderer.material = Resources.Load($"Materials/{color}Vampire", typeof(Material)) as Material;
        this.color = color;
        materialColor = meshRenderer.material.color;
    }

}
