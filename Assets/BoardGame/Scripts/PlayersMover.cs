using System;
using System.Collections;
using UnityEngine;

public class PlayersMover : MonoBehaviour
{
	public System.EventHandler<PlayerEvent> MovedEvent;
	public System.EventHandler<PlayerEvent> FinishMoveEvent;
	public Player NextPlayer => _players[GetNextPlayerIndex()];

	private Player[] _players;
	private Player _activePlayer;
	private Point[] _points;
	private Coroutine _movingCoroutine = null;
	private Coroutine _outlineCoroutine = null;
	private Vector3 _targetPoint;
	private int _indexOfTargetPoint = 0;
	private int _endPointIndex;
	private int _indexOfActivePlayer;
	private const float _speed = 5f;
	private readonly WaitForSeconds _waitforMiliSecond = new WaitForSeconds(0.3f);

	private void OnEnable()
	{
		Initialization();
	}

	public void Initialization()
	{
		MessageMenu messageMenu
			= Resources.FindObjectsOfTypeAll<MessageMenu>()[0];
		messageMenu.gameObject.SetActive(true);

		Dice dice = FindObjectOfType<Dice>();
		dice.RolledEvent += StartMoving;

		Debug.Log(dice.RolledEvent.GetInvocationList());

		Transform path = FindObjectOfType<Path>().transform;
		_points = path.GetComponentsInChildren<Point>();

		_players = FindObjectsOfType<Player>();
		_indexOfActivePlayer = 0;
		_activePlayer = _players[_indexOfActivePlayer];

		_activePlayer.GetComponent<Outline>().enabled = true;
		_outlineCoroutine ??= StartCoroutine(OutlineBlinkingCoroutine());
		messageMenu.ShowMessage($"������ ����� {_activePlayer.Name}");
	}

	private void OnDisable()
	{
		Dice dice = FindObjectOfType<Dice>();
		if (dice != null)
		{
			dice.RolledEvent -= StartMoving;
		}
	}

	private void StartMoving(object sender, DiceEventArgs e)
	{
		_movingCoroutine ??= StartCoroutine(MovmentCoroutine(e.Value));
	}

	private IEnumerator MovmentCoroutine(int diceValue)
	{
		int direction = Mathf.Abs(diceValue) / diceValue;
		_indexOfTargetPoint = _activePlayer.CurrentPoint + direction;
		_endPointIndex = _activePlayer.CurrentPoint + diceValue;

		bool isFrontMovement = diceValue > 0;

		while ((isFrontMovement == true
				&& _indexOfTargetPoint <= _endPointIndex
				&& _indexOfTargetPoint < _points.Length)
			|| (isFrontMovement == false
				&& _indexOfTargetPoint >= _endPointIndex
				&& _indexOfTargetPoint > 0))
		{
			MoveToCurrentPoint();
			yield return SwitchToNextPoint();
		}
	}

	private void MoveToCurrentPoint()
	{
		_targetPoint = _points[_indexOfTargetPoint].transform.position;
		_activePlayer.transform.position = Vector3.MoveTowards
		(
			_activePlayer.transform.position,
			_targetPoint + _activePlayer.Offset,
			_speed * Time.deltaTime
		);
	}

	private IEnumerator SwitchToNextPoint()
	{
		if (_activePlayer.transform.position
			!= _targetPoint + _activePlayer.Offset)
		{
			yield break;
		}

		MovedEvent?.Invoke(this, new PlayerEvent());
		yield return _waitforMiliSecond;
		bool isFrontMove = _indexOfTargetPoint
			> _activePlayer.CurrentPoint;

		if (isFrontMove)
		{
			_indexOfTargetPoint++;
			_activePlayer.CurrentPoint++;
		}
		else
		{
			_indexOfTargetPoint--;
			_activePlayer.CurrentPoint--;
		}

		if (_activePlayer.CurrentPoint == _points.Length - 1)
		{
			//FinishMove();
			FinishGame(isVictory: true);
			yield break;
		}

		if (_activePlayer.CurrentPoint == _endPointIndex)
		{
			FinishMove();
		}
	}

	private void FinishMove()
	{
		StopCoroutine(_movingCoroutine);
		_movingCoroutine = null;

		Point currentPoint = _points[_activePlayer.CurrentPoint];
		EffectType pointType = currentPoint.EffectType;

		MessageMenu messageMenu
			= Resources.FindObjectsOfTypeAll<MessageMenu>()[0];
		messageMenu.gameObject.SetActive(true);

		if (pointType == EffectType.Normal)
		{
			messageMenu.ShowMessage($"������ ����� {NextPlayer.Name}", currentPoint.Background);
			SwitchActivePlayer();
			return;
		}
		else
		{
			messageMenu.ShowMessage(currentPoint.Message, currentPoint.Background);
		}

		if (pointType == EffectType.MoveCount)
		{
			if (currentPoint.EffectValue == 1)
			{
				messageMenu.ShowMessage(currentPoint.Message);
				FinishMoveEvent?.Invoke(this, new PlayerEvent());
				return;
			}

			SwitchActivePlayer();
		}
		else if (pointType == EffectType.Position)
		{
			StartMoving(this, new DiceEventArgs(currentPoint.EffectValue));
		}
	}

	public void FinishGame(bool isVictory)
	{
		foreach (Player player in _players)
		{
			player.gameObject.SetActive(false);
			player.transform.position 
				= _points[0].transform.position + player.Offset;
		}

		StopCoroutine(_outlineCoroutine);
		_outlineCoroutine = null;
		StopCoroutine(_movingCoroutine);
		_movingCoroutine = null;

		MainMenu mainMenu = Resources.FindObjectsOfTypeAll<MainMenu>()[0];

		if (isVictory == true)
		{
			MessageMenu messageMenu
				= Resources.FindObjectsOfTypeAll<MessageMenu>()[0];
			VictorySound victorySoundHolder = FindObjectOfType<VictorySound>();
			AudioSource victorySound = victorySoundHolder.GetComponent<AudioSource>();

			messageMenu.gameObject.SetActive(true);
			victorySound.Play();
			messageMenu.ShowMessage($"������� {_activePlayer.Name}"); 
		}

		mainMenu.gameObject.SetActive(true);

		_activePlayer.GetComponent<Outline>().enabled = false;

		gameObject.SetActive(false);
	}

	private void SwitchActivePlayer()
	{
		if (_outlineCoroutine != null)
		{
			StopCoroutine(_outlineCoroutine);
			_outlineCoroutine = null;
		}
		_activePlayer.GetComponent<Outline>().enabled = false;

		FinishMoveEvent?.Invoke(this, new PlayerEvent());
		_indexOfActivePlayer = GetNextPlayerIndex();
		_activePlayer = _players[_indexOfActivePlayer];

		_activePlayer.GetComponent<Outline>().enabled = true;
		_outlineCoroutine ??= StartCoroutine(OutlineBlinkingCoroutine());
	}

	private IEnumerator OutlineBlinkingCoroutine()
	{
		const float MaxWidth = 13f;
		const float MinWidth = 0f;
		const float Step = 1f;
		Outline outline = _activePlayer.GetComponent<Outline>();
		outline.OutlineWidth = MinWidth;

		while (true)
		{
			if (Mathf.Approximately(outline.OutlineWidth, MinWidth))
			{
				while (outline.OutlineWidth < MaxWidth)
				{
					outline.OutlineWidth += Step;
					yield return new WaitForFixedUpdate();
				}
			}
			else if(Mathf.Approximately(outline.OutlineWidth, MaxWidth))
			{
				while (outline.OutlineWidth > MinWidth)
				{
					outline.OutlineWidth -= Step;
					yield return new WaitForFixedUpdate();
				}
			}
			else
			{
				throw new Exception("InvalidValue");
			}
		}
	}

	private int GetNextPlayerIndex()
	{
		int indexofNextPlayer = _indexOfActivePlayer + 1;

		indexofNextPlayer = indexofNextPlayer < _players.Length
			? indexofNextPlayer
			: 0;

		return indexofNextPlayer;
	}
}