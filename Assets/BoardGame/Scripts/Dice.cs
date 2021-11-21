using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class Dice : MonoBehaviour, IPointerClickHandler
{
	public EventHandler<DiceEventArgs> RolledEvent;

	private Rigidbody _rigidbody;

	private const float _jumpForce = 10f;

	public void OnPointerClick(PointerEventData eventData)
	{
		Rolling();
	}

	private void Rolling()
	{
		if (_rigidbody.IsSleeping() == false)
		{
			return;
		}

		_rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

		float randomX = UnityEngine.Random.Range(-360f, 360f);
		float randomY = UnityEngine.Random.Range(-360f, 360f);
		float randomZ = UnityEngine.Random.Range(-360f, 360f);

		_rigidbody.AddTorque(randomX, randomY, randomZ);
	}

	private void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
	}

    private void FixedUpdate()
    {
        if (_rigidbody.IsSleeping() == true)
        {
			DiceSideCheck();
        }
    }

	private void DiceSideCheck()
    {
		foreach (DiceSide side in GetComponentsInChildren<DiceSide>())
		{
			if (side.IsGrounded)
			{
				RolledEvent?.Invoke(this, new DiceEventArgs(side.SideValue));
				return;
			}
		}

		Rolling();
	}
}
