using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Unity.Netcode;
using System;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using TMPro;


namespace It4080
{
    public class Lobby : NetworkBehaviour
    {

        public It4080.ConnectedPlayers connectedPlayers;


        public Button btnStart;
        private bool isCleared = false;

        string you = "";
        string who = "";
        string readyStatus = "Not Ready";
        int playersReadyUp;

        //---------------------------------
        // Start

        void Start()
        {
            spawnClear();

            btnStart.onClick.AddListener(BtnStartGameOnClick);
            btnStart.enabled = false;
        }

        private void BtnStartGameOnClick()
        {
            StartGame();
        }

        private void StartGame()
        {
            Debug.Log("");
            NetworkManager.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        
        //----------------------------------------
        // On Network Spawn

        public override void OnNetworkSpawn()
        {
            spawnClear();

            if (IsClient)
            {
                NetworkHandler.Singleton.allPlayers.OnListChanged += ClientOnAllPlayersChanged;
                ConnectedPlayersDataList(NetworkHandler.Singleton.allPlayers);

                if (!IsHost)
                {
                    NetworkManager.OnClientDisconnectCallback += ClientOnDisconnect;
                }
            }

            if (IsServer)
            {
                NetworkHandler.Singleton.allPlayers.OnListChanged += LogNetworkListEvent;
            }

            btnStart.gameObject.SetActive(IsHost);
            
        }

        private void spawnClear()
        {
            if (!isCleared)
            {
                connectedPlayers.Clear();
                isCleared = true;
            }
            else
            {
                isCleared = false;
            }
        }

        //------------------------------
        // Player Card


        private PlayerCard AddPlayerCard(ulong clientId)
        {
            It4080.PlayerCard newCard = connectedPlayers.AddPlayer("temp", clientId);

            newCard.ShowKick(IsServer);
            if (IsServer)
            {
                newCard.KickPlayer += ServerKickPlayer;
            }

            if (clientId == NetworkManager.LocalClientId)
            {
                you = "(you)";
                newCard.ShowReady(true);
                newCard.ReadyToggled += ClientReadyToggled;
            }
            else
            {
                you = "";
                newCard.ShowReady(false);
            }

            if (clientId == NetworkManager.ServerClientId)
            {
                who = "(Host)";
                newCard.ShowKick(false);
                newCard.ShowReady(false);
            }
            else
            {
                who = "(Player)";
            }

            newCard.SetPlayerName($"{who} {clientId}{you}");
            return newCard;
        }

        private void ConnectedPlayersDataList(NetworkList<It4080.PlayerData> playersData)
        {
            connectedPlayers.Clear();

            foreach (It4080.PlayerData players in playersData)
            {
                var currentCard = AddPlayerCard(players.clientId);
                currentCard.SetReady(players.isReady);
                if (players.isReady)
                {
                    readyStatus = "Ready";
                }
                currentCard.SetStatus(readyStatus);
            }

        }

        private void LogNetworkListEvent(NetworkListEvent<It4080.PlayerData> changeEvent)
        {
            Debug.Log($"Player data changed:");
            Debug.Log($"    Change Type:  {changeEvent.Type}");
            Debug.Log($"    Value:        {changeEvent.Value}");
            Debug.Log($"        {changeEvent.Value.clientId}");
            Debug.Log($"        {changeEvent.Value.isReady}");
            Debug.Log($"    Prev Value:   {changeEvent.PreviousValue}");
            Debug.Log($"        {changeEvent.PreviousValue.clientId}");
            Debug.Log($"        {changeEvent.PreviousValue.isReady}");
        }


        //------------------------------
        //Events

        private void ClientOnDisconnect(ulong clientId)
        {
            //txtKicked.gameObject.SetActive(true);
            connectedPlayers.gameObject.SetActive(false);
        }

        private void ClientOnAllPlayersChanged(NetworkListEvent<It4080.PlayerData> changeEvent)
        {
            ConnectedPlayersDataList(NetworkHandler.Singleton.allPlayers);

            if (IsHost)
            {
                IfAllPlayersReady();
            }
        }

        private void IfAllPlayersReady()
        {
            ConnectedPlayersDataList(NetworkHandler.Singleton.allPlayers);
            playersReadyUp = 0;

            foreach (It4080.PlayerData readyPlayers in NetworkHandler.Singleton.allPlayers)
            {
                if (readyPlayers.isReady)
                {
                    playersReadyUp += 1;
                }
            }

            if (playersReadyUp == NetworkHandler.Singleton.allPlayers.Count)
            {
                btnStart.enabled = true;
            }
        }

        private void ClientReadyToggled(bool isReady)
        {
            RequestSetReadyServerRpc(isReady);
        }

        private void ServerKickPlayer(ulong clientId)
        {
            Debug.Log($"Kicked {clientId}");
            NetworkManager.DisconnectClient(clientId);
            NetworkHandler.Singleton.RemovePlayerFromList(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            int playerIndex = NetworkHandler.Singleton.FindPlayerIndex(clientId);
            It4080.PlayerData info = NetworkHandler.Singleton.allPlayers[playerIndex];
            info.isReady = isReady;
            NetworkHandler.Singleton.allPlayers[playerIndex] = info;
        }
    }
}
