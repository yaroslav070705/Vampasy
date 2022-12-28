using UnityEngine;
using Photon.Pun;

public class GameFieldManager : MonoBehaviourPunCallbacks
{

    [SerializeField] Animator animator;
    public GameManager gameManager { get; set; }
    public delegate void State();
    public event State rotationEnded;

    [PunRPC]
    public void Rotate()
    {
        animator.SetTrigger("Rotate");
    }

    public void UpdatePlatesPositions() {
        rotationEnded();
    }

}
