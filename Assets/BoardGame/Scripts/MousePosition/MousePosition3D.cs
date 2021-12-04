using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MousePosition3D : MonoBehaviour
{
	[SerializeField] private Camera _mainCamera;
	[SerializeField] private LayerMask _layerMask;

	private PreviewMessage _previewMessage;

	private void Awake()
	{
		_previewMessage = Resources.FindObjectsOfTypeAll<PreviewMessage>()[0];
	}

	private void Update()
	{
		Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, _layerMask))
		{
			transform.position = raycastHit.point;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out Point point) == false
			|| point.EffectType == EffectType.Normal)
		{
			return;
		}

		_previewMessage.transform.LookAt(_mainCamera.transform);

		Quaternion tempQuaternion = Quaternion.identity;
		tempQuaternion.x = 0.5f;
		_previewMessage.transform.rotation = tempQuaternion;

		_previewMessage.gameObject.SetActive(true);
		var text = _previewMessage.GetComponentInChildren<TextMeshProUGUI>();
		text.text = point.Message;
		//Vector3 tempPosition = Input.mousePosition.normalized;
		//_previewMessage.transform.position = tempPosition;
	}



	private void OnTriggerExit(Collider other)
	{
		_previewMessage.gameObject.SetActive(false);
	}
}
