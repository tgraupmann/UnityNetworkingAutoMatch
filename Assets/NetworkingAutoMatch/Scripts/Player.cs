using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

namespace NetworkingAutoMatch
{
    public class Player : NetworkBehaviour
    {
        public float _mSpeed = 10f;
        private DateTime _mUpTimer = DateTime.MinValue;
        public Material _mMaterial = null;
        public Material _mMaterialInstance = null;
        public MeshRenderer _mMeshPlayer = null;
        public MeshRenderer _mMeshVisor = null;
        public Camera _mCamera = null;
        public RigidbodyFirstPersonController _mController = null;
        public AudioListener _mAudioListener = null;
        public HeadBob _mHeadBob = null;
        public NetworkIdentity _mIdentity = null;
        public GameObject _mBulletPrefab = null;
        public Transform _mBulletSpawn = null;
        public float _mBulletSpeed = 20f;
        private GameObject _mMainCamera = null;
        private bool _mIsLocal = false;

        private Color[] _mColors =
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
        };

        public void OnGUI()
        {
            if (isLocalPlayer)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
                GUILayout.FlexibleSpace();
                GUILayout.Label(string.Format("You are the {0}", isServer ? "SERVER" : "CLIENT"));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        private void HidePlayer()
        {
            _mIsLocal = true;
            _mMainCamera = GameObject.Find("Main Camera");
            if (_mMainCamera)
            {
                _mMainCamera.gameObject.SetActive(false);
            }
            if (_mMeshPlayer)
            {
                _mMeshPlayer.gameObject.SetActive(false);
            }
        }

        public override void OnStartLocalPlayer()
        {
            Debug.Log(string.Format("Local Player: {0}", netId.Value));
            HidePlayer();
        }

        public void Start()
        {
            if (!isLocalPlayer)
            {
                Debug.Log(string.Format("Server Player: {0}", netId.Value));
                if (_mCamera)
                {
                    _mCamera.enabled = false;
                }
                if (_mController)
                {
                    _mController.enabled = false;
                }
                if (_mAudioListener)
                {
                    _mAudioListener.enabled = false;
                }
                if (_mHeadBob)
                {
                    _mHeadBob.enabled = false;
                }
                if (_mMeshPlayer)
                {
                    if (_mMaterial)
                    {
                        _mMaterialInstance = (Material)Instantiate(_mMaterial);
                        _mMeshPlayer.material = _mMaterialInstance;
                        _mMeshPlayer.material.color = _mColors[netId.Value % _mColors.Length];
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if (_mIsLocal)
            {
                if (_mMainCamera)
                {
                    _mMainCamera.gameObject.SetActive(true);
                }
            }
            if (_mMaterialInstance)
            {
                Destroy(_mMaterialInstance);
            }
        }

        public void Hit()
        {
            if (isLocalPlayer)
            {
                _mUpTimer = DateTime.Now + TimeSpan.FromSeconds(0.1f);
            }
        }

        private void Fire()
        {
            // Create the Bullet from the Bullet Prefab
            GameObject bullet = (GameObject)Instantiate(_mBulletPrefab, _mBulletSpawn.position, _mBulletSpawn.rotation);

            // Add velocity to the bullet
            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * _mBulletSpeed;

            // Spawn the bullet on the Clients
            NetworkServer.Spawn(bullet);

            // Destroy the bullet after 2 seconds
            Destroy(bullet, 2.0f);
        }

        /// <summary>
        /// ClientRpc calls are sent from objects on the server to objects on clients.
        /// They can be sent from any server object with a NetworkIdentity that has
        /// been spawned. Since the server has authority, then there no security
        /// issues with server objects being able to send these calls. To make a
        /// function into a ClientRpc call, add the [ClientRpc] custom attribute to
        /// it, and add the “Rpc” prefix. This function will now be run on clients
        /// when it is called on the server. Any arguments will automatically be
        /// passed to the clients with the ClientRpc call.
        /// ClientRpc functions must have the prefix “Rpc”. This is a hint when
        /// reading code that calls the method - this function is special and is
        /// not invoked locally like a normal function.
        /// </summary>
        [ClientRpc]
        public void RpcFire()
        {
            Fire();
        }

        /// <summary>
        /// The [Command] attribute indicates that the following function
        /// will be called by the Client, but will be run on the Server.
        /// </summary>
        [Command]
        public void CmdFire()
        {
            Fire();
        }

        // Update is called once per frame
        void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (_mController)
                {
                    _mController.enabled = !_mController.enabled;
                    Cursor.visible = !_mController.enabled;
                    if (Cursor.visible)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                }
            }

            Vector3 pos = transform.position;
            if (DateTime.Now < _mUpTimer)
            {
                pos += Vector3.up * _mSpeed * Time.deltaTime;
            }
            transform.position = pos;

            if (_mController &&
                _mController.enabled &&
                Input.GetMouseButtonDown(0))
            {
                CmdFire();
            }
        }
    }
}
