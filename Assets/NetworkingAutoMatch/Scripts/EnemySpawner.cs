using UnityEngine;
using UnityEngine.Networking;

namespace NetworkingAutoMatch
{
    public class EnemySpawner : NetworkBehaviour
    {
        public GameObject _mEnemyPrefab;

        public int _mNumberOfEnemies;

        private const float RANGE = 100f;

        public override void OnStartServer()
        {
            for (int i = 0; i < _mNumberOfEnemies; ++i)
            {
                var spawnPosition = new Vector3(
                    Random.Range(-RANGE, RANGE),
                    2f,
                    Random.Range(-RANGE, RANGE));

                var spawnRotation = Quaternion.Euler(
                    0.0f,
                    Random.Range(0, 180),
                    0.0f);


                var enemy = (GameObject)Instantiate(_mEnemyPrefab, spawnPosition, spawnRotation);
                NetworkServer.Spawn(enemy);
            }
        }
    }
}
