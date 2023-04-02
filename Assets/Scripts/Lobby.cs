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
        public Button btnKick;
        private bool isCleared = false;

        void Start()
        {
            startedClear();
            //btnKick.gameObject.SetActive(false);

            btnKick = GameObject.Find("BtnKick").GetComponent<Button>();
            btnStart = GameObject.Find("BtnStartGame").GetComponent<Button>();
            btnStart.onClick.AddListener(BtnStartGameOnClick);
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

        private void startedClear()
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

        public override void OnNetworkSpawn()
        {
            /*
            if (IsServer)
            {
                NetworkHandler.Singleton.allPlayers.OnListChanged += LogNetworkListEvent;
            }

            btnStart.gameObject.SetActive(IsHost);
            */
        }

        /*private PlayerCard AddPlayerCard(ulong clientId)
        {
            //It4080.PlayerCard newCard = ConnectedPlayers.AddPlayer();
        }
        */
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
    }
}
