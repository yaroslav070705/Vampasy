using UnityEngine;
using Photon.Pun;

public class GameFieldManager : MonoBehaviourPunCallbacks
{

    [SerializeField] Animator animator;
    [SerializeField] PhotonView view;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && view.IsMine)
        {
            view.RPC("Rotate",RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void Rotate()
    {
        animator.SetTrigger("Rotate");
    }

}
