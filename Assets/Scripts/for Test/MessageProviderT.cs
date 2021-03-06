﻿namespace Mapbox.Unity.Ar
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Mapbox.Unity.Utilities;
	using Mapbox.Utils;
	using Mapbox.Unity.Map;

	public class MessageProviderT : MonoBehaviour {
		/// <summary>
		/// 이 클래스에서는 GPS좌표에 따라 메시지가 로드되고 제거되며 
		/// scene 내에서 메시지의 위치가 조정됩니다.
		/// </summary>
		private static MessageProviderT _instance;
		public static MessageProviderT Instance { get { return _instance; } }

		[SerializeField]
		private AbstractMap _map;

		[HideInInspector]
		public List<GameObject> currentMessages = new List<GameObject>();
		[HideInInspector]
		private bool gotInitialAlignment = false;

		public Mapbox.Unity.Location.DeviceLocationProviderAndroidNative deviceLocation;

		void Awake(){
			_instance = this;
		}

		public void GotAlignment(){
				if (!gotInitialAlignment) {
					gotInitialAlignment = true;
					// 인증되면 UI활성화 설정
					UIBehaviorT.Instance.ShowHomeUI ();
					// 첫번째 메시지 로드
					MessageServiceT.Instance.LoadMessages ();
					Unity.Utilities.Console.Instance.Log("Loading UI and initial messages!", "lightblue");
				} else {
					UpdateARMessageLocations (deviceLocation.CurrentLocation.LatitudeLongitude);
					Unity.Utilities.Console.Instance.Log("Repositioning messages!", "lightblue");
				}
		}

		public void RemoveCurrentMessages(){
			foreach (GameObject messageObject in currentMessages) {
				Destroy (messageObject);
			}
			currentMessages.Clear ();
		}

		public void LoadARMessages(List<GameObject> messageObjectList){
			StartCoroutine (LoadARMessagesRoutine (messageObjectList));
		}

		// 서버에서 로드된 후 초기 메시지를 배치합니다.
		IEnumerator LoadARMessagesRoutine(List<GameObject> messageObjectList){

			RemoveCurrentMessages ();

			yield return new WaitForSeconds(2f);

			foreach (GameObject messageObject in messageObjectList) {

				Message thisMessage = messageObject.GetComponent<Message> ();

				Vector3 _targetPosition = _map.Root.TransformPoint(Conversions.GeoToWorldPosition(thisMessage.latitude,thisMessage.longitude, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz());

				Debug.Log ("~~~~TARGET POSITION: " + _targetPosition);

				messageObject.transform.position = _targetPosition;
				messageObject.GetComponent<Message> ().SetText (thisMessage.text);
				// 나중에 위치를 업데이트할 수 있도록 목록에 추가
				currentMessages.Add(messageObject);
			}
		}
		// 이 위치는 우리의 위치가 업데이트될 때마다 메시지를 표시합니다.
		public void UpdateARMessageLocations(Vector2d currentLocation){

			if (currentMessages.Count > 0) {

				Debug.Log ("Repositioning Messages...");

				foreach (GameObject messageObject in currentMessages) {

					Message message = messageObject.GetComponent<Message> ();

					Vector3 _targetPosition = _map.Root.TransformPoint(Conversions.GeoToWorldPosition(message.latitude,message.longitude, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz()); 
					//_map.Root.TransformPoint(Conversions.~
					messageObject.transform.position = _targetPosition;
				}
			}
		}
	}
}

