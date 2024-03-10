using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.VisualScripting;
public class TestLobby : MonoBehaviour
{
    private Lobby _hostLobby;
    private Lobby _currentLobby;
    private float _heartbeatTimer = 0;
    private const float HEART_BEAT_TIMER_MAX = 15f;

    public event Action LobbyConnectingStarted;
    public event Action<string> LobbyConnectingCompleted;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHearbeat();
    }


    private async void HandleLobbyHearbeat()
    {
        if (_currentLobby != null)
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer < 0f)
            {
                _heartbeatTimer = HEART_BEAT_TIMER_MAX;

                await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
        }
    }

    async void OnApplicationQuit()
    {
        await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
    }
    
    public async void ConnectOrCreateLobby()
    {
        if (_currentLobby != null)
            return;

        LobbyConnectingStarted?.Invoke();

        var availableLobbies = await ListLobbies();

        if (availableLobbies.Count > 0)
        {
            Debug.Log(availableLobbies[0].Name);
            await QuickJoinLobby();
        }
        else
        {
            await CreateLobby();
        }

        LobbyConnectingCompleted?.Invoke(_currentLobby.Id);
    }

    private async Task CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby" + AuthenticationService.Instance.PlayerId;
            int maxPlayers = 4;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers).ConfigureAwait(false);
            Debug.Log("created Lobby" + lobby.Name + lobby.MaxPlayers);
            _hostLobby = lobby;
            _currentLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async Task<List<Lobby>> ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions()
            {
                Count = 25,

                Filters = new List<QueryFilter>(){
                    new(QueryFilter.FieldOptions.AvailableSlots,"0", QueryFilter.OpOptions.GT),
                },
                Order = new List<QueryOrder>(){
                    new(false,QueryOrder.FieldOptions.Created),
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            return queryResponse.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return new();
        }
    }
    private async Task QuickJoinLobby()
    {
        try
        {
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

            _currentLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
