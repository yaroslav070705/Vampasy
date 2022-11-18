using UnityEngine;
using Photon.Pun;

public class GameFieldManager : MonoBehaviourPunCallbacks
{

    [SerializeField] Animator animator;

    [PunRPC]
    public void Rotate()
    {
        animator.SetTrigger("Rotate");
    }

}
