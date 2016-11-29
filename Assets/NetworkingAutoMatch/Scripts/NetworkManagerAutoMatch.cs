using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

namespace NetworkingAutoMatch
{
    public class NetworkManagerAutoMatch : MonoBehaviour
    {
        public NetworkManager _mNetworkManager = null;

        public void CallbackJoinMatch(bool success, string extendedInfo, MatchInfo matchResponse)
        {
            Debug.Log(string.Format("CallbackJoinMatch: success={0} info={1} auth={2}", success, extendedInfo, matchResponse.accessToken.GetByteString()));
            if (success)
            {
                // JOIN INTERNET MATCH
                NetworkManager.singleton.StartClient(matchResponse);
            }
        }

        public void CallbackCreateMatch(bool success, string extendedInfo, MatchInfo matchResponse)
        {
            Debug.Log(string.Format("CallbackCreateMatch: success={0} info={1} auth={2}", success, extendedInfo, matchResponse.accessToken.GetByteString()));
            if (success)
            {
                // CREATE INTERNET MATCH
                NetworkServer.Listen(matchResponse, 9000);
                NetworkManager.singleton.StartHost(matchResponse);
            }
        }

        public void CallbackListMatches(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
        {
            Debug.Log(string.Format("CallbackListMatches: success={0} info={1} count={2}", success, extendedInfo, responseData.Count));
            if (responseData.Count == 0)
            {
                // CREATE INTERNET MATCH
                string matchName = "default";
                uint matchSize = 16;
                bool matchAdvertise = true;
                string matchPassword = string.Empty;
                string publicClientAddress = string.Empty;
                string privateClientAddress = string.Empty;
                int eloScoreForMatch = 0;
                int requestDomain = 0;
                _mNetworkManager.matchMaker.CreateMatch(matchName, matchSize, matchAdvertise, matchPassword, publicClientAddress, privateClientAddress, eloScoreForMatch, requestDomain, CallbackCreateMatch);
            }
            else
            {
                // PRINT MATCHES
                foreach (MatchInfoSnapshot matchInfoSnapshot in responseData)
                {
                    Debug.Log(string.Format("Join Match:{0}", matchInfoSnapshot.name));

                    // JOIN INTERNET MATCH
                    NetworkID netId = matchInfoSnapshot.networkId;
                    string matchPassword = string.Empty;
                    string publicClientAddress = string.Empty;
                    string privateClientAddress = string.Empty;
                    int eloScoreForClient = 0;
                    int requestDomain = 0;
                    _mNetworkManager.matchMaker.JoinMatch(netId, matchPassword, publicClientAddress, privateClientAddress, eloScoreForClient, requestDomain, CallbackJoinMatch);
                    break;
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            if (_mNetworkManager)
            {
                // ENABLE MATCH MAKER
                _mNetworkManager.StartMatchMaker();
                if (_mNetworkManager.matchMaker)
                {
                    // FIND MATCHES
                    _mNetworkManager.matchMaker.ListMatches(0, 10, "default", false, 0, 0, CallbackListMatches);
                }
            }
        }
    }
}
