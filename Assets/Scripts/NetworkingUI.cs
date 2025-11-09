using Unity.Netcode;

using UnityEngine;
using UnityEngine.UI;

public class NetworkingUI : MonoBehaviour
{

  public Button hostButton;

  public Button clientButton;

  private void Awake()
  {
    hostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
    clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
  }
}
