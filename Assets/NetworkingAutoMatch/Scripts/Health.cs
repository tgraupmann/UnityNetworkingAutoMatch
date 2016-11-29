using UnityEngine;
using UnityEngine.Networking;

namespace NetworkingAutoMatch
{
    public class Health : NetworkBehaviour
    {
        public const int MAX_HEALTH = 100;

        [SyncVar(hook = "OnChangeHealth")]
        public int _mCurrentHealth = MAX_HEALTH;

        public RectTransform _mHealthBar = null;

        private NetworkStartPosition[] _mSpawnPoints = null;

        public bool _mDestroyOnDeath = false;

        void Start()
        {
            if (isLocalPlayer)
            {
                _mSpawnPoints = FindObjectsOfType<NetworkStartPosition>();
            }
        }

        void OnChangeHealth(int health)
        {
            if (_mHealthBar)
            {
                _mHealthBar.sizeDelta = new Vector2(health, _mHealthBar.sizeDelta.y);
            }
        }

        [ClientRpc]
        void RpcRespawn()
        {
            if (isLocalPlayer)
            {
                // Set the spawn point to origin as a default value
                Vector3 spawnPoint = Vector3.zero;

                // If there is a spawn point array and the array is not empty, pick one at random
                if (_mSpawnPoints != null && _mSpawnPoints.Length > 0)
                {
                    spawnPoint = _mSpawnPoints[Random.Range(0, _mSpawnPoints.Length)].transform.position;
                }

                // Set the player’s position to the chosen spawn point
                transform.position = spawnPoint;
            }
        }

        public void TakeDamage(int amount)
        {
            if (!isServer)
            {
                return;
            }
            _mCurrentHealth -= amount;
            if (_mCurrentHealth <= 0)
            {
                _mCurrentHealth = MAX_HEALTH;

                if (_mDestroyOnDeath)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // called on the Server, but invoked on the Clients
                    RpcRespawn();
                }
            }
        }
    }
}
