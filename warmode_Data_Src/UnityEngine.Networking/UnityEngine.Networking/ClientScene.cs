using System;
using System.Collections.Generic;
using UnityEngine.Networking.NetworkSystem;

namespace UnityEngine.Networking
{
	public class ClientScene
	{
		private struct PendingOwner
		{
			public NetworkInstanceId netId;

			public short playerControllerId;
		}

		private static List<PlayerController> s_LocalPlayers = new List<PlayerController>();

		private static NetworkConnection s_ReadyConnection;

		private static Dictionary<NetworkSceneId, NetworkIdentity> s_SpawnableObjects;

		private static bool s_IsReady;

		private static bool s_IsSpawnFinished;

		private static NetworkScene s_NetworkScene = new NetworkScene();

		private static ObjectSpawnSceneMessage s_ObjectSpawnSceneMessage = new ObjectSpawnSceneMessage();

		private static ObjectSpawnFinishedMessage s_ObjectSpawnFinishedMessage = new ObjectSpawnFinishedMessage();

		private static ObjectDestroyMessage s_ObjectDestroyMessage = new ObjectDestroyMessage();

		private static ObjectSpawnMessage s_ObjectSpawnMessage = new ObjectSpawnMessage();

		private static OwnerMessage s_OwnerMessage = new OwnerMessage();

		private static ClientAuthorityMessage s_ClientAuthorityMessage = new ClientAuthorityMessage();

		private static List<ClientScene.PendingOwner> s_PendingOwnerIds = new List<ClientScene.PendingOwner>();

		public static List<PlayerController> localPlayers
		{
			get
			{
				return ClientScene.s_LocalPlayers;
			}
		}

		public static bool ready
		{
			get
			{
				return ClientScene.s_IsReady;
			}
		}

		public static NetworkConnection readyConnection
		{
			get
			{
				return ClientScene.s_ReadyConnection;
			}
		}

		public static Dictionary<NetworkInstanceId, NetworkIdentity> objects
		{
			get
			{
				return ClientScene.s_NetworkScene.localObjects;
			}
		}

		public static Dictionary<NetworkHash128, GameObject> prefabs
		{
			get
			{
				return NetworkScene.guidToPrefab;
			}
		}

		public static Dictionary<NetworkSceneId, NetworkIdentity> spawnableObjects
		{
			get
			{
				return ClientScene.s_SpawnableObjects;
			}
		}

		internal static void SetNotReady()
		{
			ClientScene.s_IsReady = false;
		}

		internal static void Shutdown()
		{
			ClientScene.s_NetworkScene.Shutdown();
			ClientScene.s_LocalPlayers = new List<PlayerController>();
			ClientScene.s_PendingOwnerIds = new List<ClientScene.PendingOwner>();
			ClientScene.s_SpawnableObjects = null;
			ClientScene.s_ReadyConnection = null;
			ClientScene.s_IsReady = false;
			ClientScene.s_IsSpawnFinished = false;
			NetworkTransport.Shutdown();
			NetworkTransport.Init();
		}

		internal static bool GetPlayerController(short playerControllerId, out PlayerController player)
		{
			player = null;
			if ((int)playerControllerId >= ClientScene.localPlayers.Count)
			{
				if (LogFilter.logWarn)
				{
					Debug.Log("ClientScene::GetPlayer: no local player found for: " + playerControllerId);
				}
				return false;
			}
			if (ClientScene.localPlayers[(int)playerControllerId] == null)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("ClientScene::GetPlayer: local player is null for: " + playerControllerId);
				}
				return false;
			}
			player = ClientScene.localPlayers[(int)playerControllerId];
			return player.gameObject != null;
		}

		internal static void InternalAddPlayer(NetworkIdentity view, short playerControllerId)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("ClientScene::InternalAddPlayer: playerControllerId : " + playerControllerId);
			}
			if ((int)playerControllerId >= ClientScene.s_LocalPlayers.Count)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("ClientScene::InternalAddPlayer: playerControllerId higher than expected: " + playerControllerId);
				}
				while ((int)playerControllerId >= ClientScene.s_LocalPlayers.Count)
				{
					ClientScene.s_LocalPlayers.Add(new PlayerController());
				}
			}
			PlayerController playerController = new PlayerController
			{
				gameObject = view.gameObject,
				playerControllerId = playerControllerId,
				unetView = view
			};
			ClientScene.s_LocalPlayers[(int)playerControllerId] = playerController;
			ClientScene.s_ReadyConnection.SetPlayerController(playerController);
		}

		public static bool AddPlayer(short playerControllerId)
		{
			return ClientScene.AddPlayer(null, playerControllerId);
		}

		public static bool AddPlayer(NetworkConnection readyConn, short playerControllerId)
		{
			return ClientScene.AddPlayer(readyConn, playerControllerId, null);
		}

		public static bool AddPlayer(NetworkConnection readyConn, short playerControllerId, MessageBase extraMessage)
		{
			if (playerControllerId < 0)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " is negative");
				}
				return false;
			}
			if (playerControllerId > 32)
			{
				if (LogFilter.logError)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"ClientScene::AddPlayer: playerControllerId of ",
						playerControllerId,
						" is too high, max is ",
						32
					}));
				}
				return false;
			}
			if (playerControllerId > 16 && LogFilter.logWarn)
			{
				Debug.LogWarning("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " is unusually high");
			}
			while ((int)playerControllerId >= ClientScene.s_LocalPlayers.Count)
			{
				ClientScene.s_LocalPlayers.Add(new PlayerController());
			}
			if (readyConn == null)
			{
				if (!ClientScene.s_IsReady)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("Must call AddPlayer() with a connection the first time to become ready.");
					}
					return false;
				}
			}
			else
			{
				ClientScene.s_IsReady = true;
				ClientScene.s_ReadyConnection = readyConn;
			}
			PlayerController playerController;
			if (ClientScene.s_ReadyConnection.GetPlayerController(playerControllerId, out playerController) && playerController.IsValid && playerController.gameObject != null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ClientScene::AddPlayer: playerControllerId of " + playerControllerId + " already in use.");
				}
				return false;
			}
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::AddPlayer() for ID ",
					playerControllerId,
					" called with connection [",
					ClientScene.s_ReadyConnection,
					"]"
				}));
			}
			AddPlayerMessage addPlayerMessage = new AddPlayerMessage();
			addPlayerMessage.playerControllerId = playerControllerId;
			if (extraMessage != null)
			{
				NetworkWriter networkWriter = new NetworkWriter();
				extraMessage.Serialize(networkWriter);
				addPlayerMessage.msgData = networkWriter.ToArray();
				addPlayerMessage.msgSize = (int)networkWriter.Position;
			}
			ClientScene.s_ReadyConnection.Send(37, addPlayerMessage);
			return true;
		}

		public static bool RemovePlayer(short playerControllerId)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::RemovePlayer() for ID ",
					playerControllerId,
					" called with connection [",
					ClientScene.s_ReadyConnection,
					"]"
				}));
			}
			PlayerController playerController;
			if (ClientScene.s_ReadyConnection.GetPlayerController(playerControllerId, out playerController))
			{
				RemovePlayerMessage removePlayerMessage = new RemovePlayerMessage();
				removePlayerMessage.playerControllerId = playerControllerId;
				ClientScene.s_ReadyConnection.Send(38, removePlayerMessage);
				ClientScene.s_ReadyConnection.RemovePlayerController(playerControllerId);
				ClientScene.s_LocalPlayers[(int)playerControllerId] = new PlayerController();
				UnityEngine.Object.Destroy(playerController.gameObject);
				return true;
			}
			if (LogFilter.logError)
			{
				Debug.LogError("Failed to find player ID " + playerControllerId);
			}
			return false;
		}

		public static bool Ready(NetworkConnection conn)
		{
			if (ClientScene.s_IsReady)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("A connection has already been set as ready. There can only be one.");
				}
				return false;
			}
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::Ready() called with connection [" + conn + "]");
			}
			if (conn != null)
			{
				ReadyMessage msg = new ReadyMessage();
				conn.Send(35, msg);
				ClientScene.s_IsReady = true;
				ClientScene.s_ReadyConnection = conn;
				ClientScene.s_ReadyConnection.isReady = true;
				return true;
			}
			if (LogFilter.logError)
			{
				Debug.LogError("Ready() called with invalid connection object: conn=null");
			}
			return false;
		}

		public static NetworkClient ConnectLocalServer()
		{
			LocalClient localClient = new LocalClient();
			NetworkServer.instance.ActivateLocalClientScene();
			localClient.InternalConnectLocalServer();
			return localClient;
		}

		internal static void HandleClientDisconnect(NetworkConnection conn)
		{
			if (ClientScene.s_ReadyConnection == conn && ClientScene.s_IsReady)
			{
				ClientScene.s_IsReady = false;
				ClientScene.s_ReadyConnection = null;
			}
		}

		internal static void PrepareToSpawnSceneObjects()
		{
			ClientScene.s_SpawnableObjects = new Dictionary<NetworkSceneId, NetworkIdentity>();
			NetworkIdentity[] array = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
			for (int i = 0; i < array.Length; i++)
			{
				NetworkIdentity networkIdentity = array[i];
				if (!networkIdentity.gameObject.activeSelf)
				{
					if (networkIdentity.gameObject.hideFlags != HideFlags.NotEditable && networkIdentity.gameObject.hideFlags != HideFlags.HideAndDontSave)
					{
						if (!networkIdentity.sceneId.IsEmpty())
						{
							ClientScene.s_SpawnableObjects[networkIdentity.sceneId] = networkIdentity;
							if (LogFilter.logDebug)
							{
								Debug.Log("ClientScene::PrepareSpawnObjects sceneId:" + networkIdentity.sceneId);
							}
						}
					}
				}
			}
		}

		internal static NetworkIdentity SpawnSceneObject(NetworkSceneId sceneId)
		{
			if (ClientScene.s_SpawnableObjects.ContainsKey(sceneId))
			{
				NetworkIdentity result = ClientScene.s_SpawnableObjects[sceneId];
				ClientScene.s_SpawnableObjects.Remove(sceneId);
				return result;
			}
			return null;
		}

		internal static void RegisterSystemHandlers(NetworkClient client, bool localClient)
		{
			if (localClient)
			{
				client.RegisterHandlerSafe(1, new NetworkMessageDelegate(ClientScene.OnLocalClientObjectDestroy));
				client.RegisterHandlerSafe(13, new NetworkMessageDelegate(ClientScene.OnLocalClientObjectHide));
				client.RegisterHandlerSafe(3, new NetworkMessageDelegate(ClientScene.OnLocalClientObjectSpawn));
				client.RegisterHandlerSafe(10, new NetworkMessageDelegate(ClientScene.OnLocalClientObjectSpawnScene));
				client.RegisterHandlerSafe(15, new NetworkMessageDelegate(ClientScene.OnClientAuthority));
			}
			else
			{
				client.RegisterHandlerSafe(3, new NetworkMessageDelegate(ClientScene.OnObjectSpawn));
				client.RegisterHandlerSafe(10, new NetworkMessageDelegate(ClientScene.OnObjectSpawnScene));
				client.RegisterHandlerSafe(12, new NetworkMessageDelegate(ClientScene.OnObjectSpawnFinished));
				client.RegisterHandlerSafe(1, new NetworkMessageDelegate(ClientScene.OnObjectDestroy));
				client.RegisterHandlerSafe(13, new NetworkMessageDelegate(ClientScene.OnObjectDestroy));
				client.RegisterHandlerSafe(8, new NetworkMessageDelegate(ClientScene.OnUpdateVarsMessage));
				client.RegisterHandlerSafe(4, new NetworkMessageDelegate(ClientScene.OnOwnerMessage));
				client.RegisterHandlerSafe(9, new NetworkMessageDelegate(ClientScene.OnSyncListMessage));
				client.RegisterHandlerSafe(40, new NetworkMessageDelegate(NetworkAnimator.OnAnimationClientMessage));
				client.RegisterHandlerSafe(41, new NetworkMessageDelegate(NetworkAnimator.OnAnimationParametersClientMessage));
				client.RegisterHandlerSafe(15, new NetworkMessageDelegate(ClientScene.OnClientAuthority));
			}
			client.RegisterHandlerSafe(2, new NetworkMessageDelegate(ClientScene.OnRPCMessage));
			client.RegisterHandlerSafe(7, new NetworkMessageDelegate(ClientScene.OnSyncEventMessage));
			client.RegisterHandlerSafe(42, new NetworkMessageDelegate(NetworkAnimator.OnAnimationTriggerClientMessage));
		}

		internal static string GetStringForAssetId(NetworkHash128 assetId)
		{
			GameObject gameObject;
			if (NetworkScene.GetPrefab(assetId, out gameObject))
			{
				return gameObject.name;
			}
			SpawnDelegate spawnDelegate;
			if (NetworkScene.GetSpawnHandler(assetId, out spawnDelegate))
			{
				return spawnDelegate.Method.Name;
			}
			return "unknown";
		}

		public static void RegisterPrefab(GameObject prefab)
		{
			NetworkScene.RegisterPrefab(prefab);
		}

		public static void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			NetworkScene.RegisterPrefab(prefab, spawnHandler, unspawnHandler);
		}

		public static void UnregisterPrefab(GameObject prefab)
		{
			NetworkScene.UnregisterPrefab(prefab);
		}

		public static void RegisterSpawnHandler(NetworkHash128 assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			NetworkScene.RegisterSpawnHandler(assetId, spawnHandler, unspawnHandler);
		}

		public static void UnregisterSpawnHandler(NetworkHash128 assetId)
		{
			NetworkScene.UnregisterSpawnHandler(assetId);
		}

		public static void ClearSpawners()
		{
			NetworkScene.ClearSpawners();
		}

		public static void DestroyAllClientObjects()
		{
			ClientScene.s_NetworkScene.DestroyAllClientObjects();
		}

		public static void SetLocalObject(NetworkInstanceId netId, GameObject obj)
		{
			ClientScene.s_NetworkScene.SetLocalObject(netId, obj, ClientScene.s_IsSpawnFinished, false);
		}

		public static GameObject FindLocalObject(NetworkInstanceId netId)
		{
			return ClientScene.s_NetworkScene.FindLocalObject(netId);
		}

		private static void ApplySpawnPayload(NetworkIdentity uv, Vector3 position, byte[] payload, NetworkInstanceId netId, GameObject newGameObject)
		{
			uv.transform.position = position;
			if (payload != null && payload.Length > 0)
			{
				NetworkReader reader = new NetworkReader(payload);
				uv.OnUpdateVars(reader, true);
			}
			if (newGameObject == null)
			{
				return;
			}
			newGameObject.SetActive(true);
			uv.SetNetworkInstanceId(netId);
			ClientScene.SetLocalObject(netId, newGameObject);
			if (ClientScene.s_IsSpawnFinished)
			{
				uv.OnStartClient();
				ClientScene.CheckForOwner(uv);
			}
		}

		private static void OnObjectSpawn(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ObjectSpawnMessage>(ClientScene.s_ObjectSpawnMessage);
			if (!ClientScene.s_ObjectSpawnMessage.assetId.IsValid())
			{
				if (LogFilter.logError)
				{
					Debug.LogError("OnObjSpawn netId: " + ClientScene.s_ObjectSpawnMessage.netId + " has invalid asset Id");
				}
				return;
			}
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Client spawn handler instantiating [netId:",
					ClientScene.s_ObjectSpawnMessage.netId,
					" asset ID:",
					ClientScene.s_ObjectSpawnMessage.assetId,
					" pos:",
					ClientScene.s_ObjectSpawnMessage.position,
					"]"
				}));
			}
			NetworkIdentity component;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectSpawnMessage.netId, out component))
			{
				ClientScene.ApplySpawnPayload(component, ClientScene.s_ObjectSpawnMessage.position, ClientScene.s_ObjectSpawnMessage.payload, ClientScene.s_ObjectSpawnMessage.netId, null);
				return;
			}
			GameObject original;
			SpawnDelegate spawnDelegate;
			if (NetworkScene.GetPrefab(ClientScene.s_ObjectSpawnMessage.assetId, out original))
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(original, ClientScene.s_ObjectSpawnMessage.position, Quaternion.identity);
				component = gameObject.GetComponent<NetworkIdentity>();
				if (component == null)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("Client object spawned for " + ClientScene.s_ObjectSpawnMessage.assetId + " does not have a NetworkIdentity");
					}
					return;
				}
				ClientScene.ApplySpawnPayload(component, ClientScene.s_ObjectSpawnMessage.position, ClientScene.s_ObjectSpawnMessage.payload, ClientScene.s_ObjectSpawnMessage.netId, gameObject);
			}
			else if (NetworkScene.GetSpawnHandler(ClientScene.s_ObjectSpawnMessage.assetId, out spawnDelegate))
			{
				GameObject gameObject2 = spawnDelegate(ClientScene.s_ObjectSpawnMessage.position, ClientScene.s_ObjectSpawnMessage.assetId);
				if (gameObject2 == null)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("Client spawn handler for " + ClientScene.s_ObjectSpawnMessage.assetId + " returned null");
					}
					return;
				}
				component = gameObject2.GetComponent<NetworkIdentity>();
				if (component == null)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("Client object spawned for " + ClientScene.s_ObjectSpawnMessage.assetId + " does not have a network identity");
					}
					return;
				}
				component.SetDynamicAssetId(ClientScene.s_ObjectSpawnMessage.assetId);
				ClientScene.ApplySpawnPayload(component, ClientScene.s_ObjectSpawnMessage.position, ClientScene.s_ObjectSpawnMessage.payload, ClientScene.s_ObjectSpawnMessage.netId, gameObject2);
			}
			else if (LogFilter.logError)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Failed to spawn server object, assetId=",
					ClientScene.s_ObjectSpawnMessage.assetId,
					" netId=",
					ClientScene.s_ObjectSpawnMessage.netId
				}));
			}
		}

		private static void OnObjectSpawnScene(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ObjectSpawnSceneMessage>(ClientScene.s_ObjectSpawnSceneMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Client spawn scene handler instantiating [netId:",
					ClientScene.s_ObjectSpawnSceneMessage.netId,
					" sceneId:",
					ClientScene.s_ObjectSpawnSceneMessage.sceneId,
					" pos:",
					ClientScene.s_ObjectSpawnSceneMessage.position
				}));
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectSpawnSceneMessage.netId, out networkIdentity))
			{
				ClientScene.ApplySpawnPayload(networkIdentity, ClientScene.s_ObjectSpawnSceneMessage.position, ClientScene.s_ObjectSpawnSceneMessage.payload, ClientScene.s_ObjectSpawnSceneMessage.netId, networkIdentity.gameObject);
				return;
			}
			NetworkIdentity networkIdentity2 = ClientScene.SpawnSceneObject(ClientScene.s_ObjectSpawnSceneMessage.sceneId);
			if (networkIdentity2 == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("Spawn scene object not found for " + ClientScene.s_ObjectSpawnSceneMessage.sceneId);
				}
				return;
			}
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Client spawn for [netId:",
					ClientScene.s_ObjectSpawnSceneMessage.netId,
					"] [sceneId:",
					ClientScene.s_ObjectSpawnSceneMessage.sceneId,
					"] obj:",
					networkIdentity2.gameObject.name
				}));
			}
			ClientScene.ApplySpawnPayload(networkIdentity2, ClientScene.s_ObjectSpawnSceneMessage.position, ClientScene.s_ObjectSpawnSceneMessage.payload, ClientScene.s_ObjectSpawnSceneMessage.netId, networkIdentity2.gameObject);
		}

		private static void OnObjectSpawnFinished(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ObjectSpawnFinishedMessage>(ClientScene.s_ObjectSpawnFinishedMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log("SpawnFinished:" + ClientScene.s_ObjectSpawnFinishedMessage.state);
			}
			if (ClientScene.s_ObjectSpawnFinishedMessage.state == 0u)
			{
				ClientScene.PrepareToSpawnSceneObjects();
				ClientScene.s_IsSpawnFinished = false;
				return;
			}
			foreach (NetworkIdentity current in ClientScene.objects.Values)
			{
				if (!current.isClient)
				{
					current.OnStartClient();
					ClientScene.CheckForOwner(current);
				}
			}
			ClientScene.s_IsSpawnFinished = true;
		}

		private static void OnObjectDestroy(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ObjectDestroyMessage>(ClientScene.s_ObjectDestroyMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnObjDestroy netId:" + ClientScene.s_ObjectDestroyMessage.netId);
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectDestroyMessage.netId, out networkIdentity))
			{
				networkIdentity.OnNetworkDestroy();
				if (!NetworkScene.InvokeUnSpawnHandler(networkIdentity.assetId, networkIdentity.gameObject))
				{
					if (networkIdentity.sceneId.IsEmpty())
					{
						UnityEngine.Object.Destroy(networkIdentity.gameObject);
					}
					else
					{
						networkIdentity.gameObject.SetActive(false);
						ClientScene.s_SpawnableObjects[networkIdentity.sceneId] = networkIdentity;
					}
				}
				ClientScene.s_NetworkScene.RemoveLocalObject(ClientScene.s_ObjectDestroyMessage.netId);
			}
			else if (LogFilter.logDebug)
			{
				Debug.LogWarning("Did not find target for destroy message for " + ClientScene.s_ObjectDestroyMessage.netId);
			}
		}

		private static void OnLocalClientObjectDestroy(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ObjectDestroyMessage>(ClientScene.s_ObjectDestroyMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnLocalObjectObjDestroy netId:" + ClientScene.s_ObjectDestroyMessage.netId);
			}
			ClientScene.s_NetworkScene.RemoveLocalObject(ClientScene.s_ObjectDestroyMessage.netId);
		}

		private static void OnLocalClientObjectHide(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ObjectDestroyMessage>(ClientScene.s_ObjectDestroyMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnLocalObjectObjHide netId:" + ClientScene.s_ObjectDestroyMessage.netId);
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectDestroyMessage.netId, out networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(false);
			}
		}

		private static void OnLocalClientObjectSpawn(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ObjectSpawnMessage>(ClientScene.s_ObjectSpawnMessage);
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectSpawnMessage.netId, out networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(true);
			}
		}

		private static void OnLocalClientObjectSpawnScene(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ObjectSpawnSceneMessage>(ClientScene.s_ObjectSpawnSceneMessage);
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ObjectSpawnSceneMessage.netId, out networkIdentity))
			{
				networkIdentity.OnSetLocalVisibility(true);
			}
		}

		private static void OnUpdateVarsMessage(NetworkMessage netMsg)
		{
			NetworkInstanceId networkInstanceId = netMsg.reader.ReadNetworkId();
			if (LogFilter.logDev)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::OnUpdateVarsMessage ",
					networkInstanceId,
					" channel:",
					netMsg.channelId
				}));
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(networkInstanceId, out networkIdentity))
			{
				networkIdentity.OnUpdateVars(netMsg.reader, false);
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarning("Did not find target for sync message for " + networkInstanceId);
			}
		}

		private static void OnRPCMessage(NetworkMessage netMsg)
		{
			int num = (int)netMsg.reader.ReadPackedUInt32();
			NetworkInstanceId networkInstanceId = netMsg.reader.ReadNetworkId();
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::OnRPCMessage hash:",
					num,
					" netId:",
					networkInstanceId
				}));
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(networkInstanceId, out networkIdentity))
			{
				networkIdentity.HandleRPC(num, netMsg.reader);
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarning("Did not find target for RPC message for " + networkInstanceId);
			}
		}

		private static void OnSyncEventMessage(NetworkMessage netMsg)
		{
			int cmdHash = (int)netMsg.reader.ReadPackedUInt32();
			NetworkInstanceId networkInstanceId = netMsg.reader.ReadNetworkId();
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnSyncEventMessage " + networkInstanceId);
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(networkInstanceId, out networkIdentity))
			{
				networkIdentity.HandleSyncEvent(cmdHash, netMsg.reader);
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarning("Did not find target for SyncEvent message for " + networkInstanceId);
			}
		}

		private static void OnSyncListMessage(NetworkMessage netMsg)
		{
			NetworkInstanceId networkInstanceId = netMsg.reader.ReadNetworkId();
			int cmdHash = (int)netMsg.reader.ReadPackedUInt32();
			if (LogFilter.logDebug)
			{
				Debug.Log("ClientScene::OnSyncListMessage " + networkInstanceId);
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(networkInstanceId, out networkIdentity))
			{
				networkIdentity.HandleSyncList(cmdHash, netMsg.reader);
			}
			else if (LogFilter.logWarn)
			{
				Debug.LogWarning("Did not find target for SyncList message for " + networkInstanceId);
			}
		}

		private static void OnClientAuthority(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<ClientAuthorityMessage>(ClientScene.s_ClientAuthorityMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::OnClientAuthority for  connectionId=",
					netMsg.conn.connectionId,
					" netId: ",
					ClientScene.s_ClientAuthorityMessage.netId
				}));
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_ClientAuthorityMessage.netId, out networkIdentity))
			{
				networkIdentity.HandleClientAuthority(ClientScene.s_ClientAuthorityMessage.authority);
			}
		}

		private static void OnOwnerMessage(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<OwnerMessage>(ClientScene.s_OwnerMessage);
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ClientScene::OnOwnerMessage - connectionId=",
					netMsg.conn.connectionId,
					" netId: ",
					ClientScene.s_OwnerMessage.netId
				}));
			}
			PlayerController playerController;
			if (netMsg.conn.GetPlayerController(ClientScene.s_OwnerMessage.playerControllerId, out playerController))
			{
				playerController.unetView.SetNotLocalPlayer();
			}
			NetworkIdentity networkIdentity;
			if (ClientScene.s_NetworkScene.GetNetworkIdentity(ClientScene.s_OwnerMessage.netId, out networkIdentity))
			{
				networkIdentity.SetConnectionToServer(netMsg.conn);
				networkIdentity.SetLocalPlayer(ClientScene.s_OwnerMessage.playerControllerId);
				ClientScene.InternalAddPlayer(networkIdentity, ClientScene.s_OwnerMessage.playerControllerId);
			}
			else
			{
				ClientScene.PendingOwner item = new ClientScene.PendingOwner
				{
					netId = ClientScene.s_OwnerMessage.netId,
					playerControllerId = ClientScene.s_OwnerMessage.playerControllerId
				};
				ClientScene.s_PendingOwnerIds.Add(item);
			}
		}

		private static void CheckForOwner(NetworkIdentity uv)
		{
			int i = 0;
			while (i < ClientScene.s_PendingOwnerIds.Count)
			{
				ClientScene.PendingOwner pendingOwner = ClientScene.s_PendingOwnerIds[i];
				if (pendingOwner.netId == uv.netId)
				{
					uv.SetConnectionToServer(ClientScene.s_ReadyConnection);
					uv.SetLocalPlayer(pendingOwner.playerControllerId);
					if (LogFilter.logDev)
					{
						Debug.Log("ClientScene::OnOwnerMessage - player=" + uv.gameObject.name);
					}
					if (ClientScene.s_ReadyConnection.connectionId < 0)
					{
						if (LogFilter.logError)
						{
							Debug.LogError("Owner message received on a local client.");
						}
						return;
					}
					ClientScene.InternalAddPlayer(uv, pendingOwner.playerControllerId);
					ClientScene.s_PendingOwnerIds.RemoveAt(i);
					break;
				}
				else
				{
					i++;
				}
			}
		}
	}
}
