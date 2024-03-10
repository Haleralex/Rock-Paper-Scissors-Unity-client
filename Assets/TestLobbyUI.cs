using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestLobbyUI : MonoBehaviour
{
    [SerializeField] private TestLobby testLobby;
    [SerializeField] private Button _connectButton;
    [SerializeField] private TextMeshProUGUI _currentConnection;

    void OnEnable()
    {
        testLobby.LobbyConnectingStarted += OnLobbyConnectingStarted;
        testLobby.LobbyConnectingCompleted += OnLobbyConnectingCompleted;
    }

    void OnDisable()
    {
        testLobby.LobbyConnectingStarted -= OnLobbyConnectingStarted;
        testLobby.LobbyConnectingCompleted -= OnLobbyConnectingCompleted;
    }

    private void OnLobbyConnectingCompleted(string obj)
    {
        _currentConnection.gameObject.SetActive(true);
        _currentConnection.text = $"CURRENT LOBBY ID = {obj}";
    }

    private void OnLobbyConnectingStarted()
    {
        _connectButton.gameObject.SetActive(false);
    }
}
