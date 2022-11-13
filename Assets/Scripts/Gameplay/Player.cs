using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkManager _netManager => NetworkManager.Singleton;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private GameObject _crown;

    private void Awake()
    {
        _crown.transform.localPosition = new Vector3(0, _renderer.bounds.size.x, 0);
    }

    public void Init(int playerNumber)
    {
        SetPlayerColorClientRpc(playerNumber);
    }

    [ClientRpc]
    private void SetPlayerColorClientRpc(int playerNumber)
    {
        if (playerNumber == 0)
            _crown.GetComponent<Renderer>().material = Managers.Resource.Load<Material>("Materials/Player0CrownMat");
        else if(playerNumber == 1)
            _crown.GetComponent<Renderer>().material = Managers.Resource.Load<Material>("Materials/Player1CrownMat");
    }

    private void Update()
    {
       
    }
}
