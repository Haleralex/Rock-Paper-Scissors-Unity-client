using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private float _heartbeatTimer = 0;
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
        if (hostLobby != null)
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                _heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {


            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);
            Debug.Log("created Lobby" + lobby.Name + lobby.MaxPlayers);

            hostLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions()
            {
                Count = 25,
                Filters = new List<QueryFilter>(){
                    new(QueryFilter.FieldOptions.AvailableSlots,"0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>(){
                    new(false,QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Founded lobbies:" + queryResponse.Results.Count);
            foreach (var lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void QuickJoinLobby()
    {
        try
        {
            await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void JoinLobby()
    {
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
        await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
    }
}
