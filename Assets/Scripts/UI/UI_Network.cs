using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.Netcode;
using System;

public class UI_Network : UI_Base
{
    private NetworkManager _netManager => NetworkManager.Singleton;

    enum Buttons
    {
        StartServer,
        StartHost,
        StartClient,
        Shutdown,
    }

    enum Texts
    {
        StartServer,
        StartHost,
        StartClient,
        Shutdown
    }

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        GetButton((int)Buttons.StartServer).gameObject.AddUIEvent(StartServer);
        GetButton((int)Buttons.StartHost).gameObject.AddUIEvent(StartHost);
        GetButton((int)Buttons.StartClient).gameObject.AddUIEvent(StartClient);
        GetButton((int)Buttons.Shutdown).gameObject.AddUIEvent(Shutdown);
    }

    private void StartServer(PointerEventData data)
    {
        if (_netManager.StartServer()) UI_Utilities.Instance.Log("Server started");
        else UI_Utilities.Instance.LogError("Unable to start server");
    }

    private void StartHost(PointerEventData data)
    {
        if (_netManager.StartHost()) UI_Utilities.Instance.Log("Host started");
        else UI_Utilities.Instance.LogError("Unable to start host");
    }

    private void StartClient(PointerEventData data)
    {
        GameNetPortal.Instance.ConnectClient();
    }

    // host라면 server도 같이 shutdown한다.
    private void Shutdown(PointerEventData data)
    {
        if (_netManager.IsServer) GameNetPortal.Instance.resetPlayerNumber();
        _netManager.Shutdown();
        UI_Utilities.Instance.LogServer(false);
        UI_Utilities.Instance.Log("Network Shutdowned");
        SessionManager<SessionPlayerData>.Instance.OnServerEnded();
        UI_Utilities.Instance.SessionText.gameObject.SetActive(false);
    }
}