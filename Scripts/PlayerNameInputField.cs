using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(InputField))]

public class PlayerNameInputField : MonoBehaviour
{

    const string playerNamePrefKey = "PlayerName";

    private void Start()
    {
        InputField inputField = GetComponent<InputField>();

        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {

            inputField.text = PlayerPrefs.GetString(playerNamePrefKey);
            PhotonNetwork.NickName = inputField.text;

        }

    }

    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.Log("string is null");
            return;
        }

        PhotonNetwork.NickName = name;
        PlayerPrefs.SetString(playerNamePrefKey, name);
    }

}
