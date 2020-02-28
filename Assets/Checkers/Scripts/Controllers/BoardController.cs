using DG.Tweening;
using NetMessaging.GameLogic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Checkers
{
	public class BoardController : Singleton<BoardController>
	{
		[Header("Components:")]
		public GameController GameControllerComponent;
		public CameraController CameraControllerComponent;
		
		[Space(5f)]
		[Header("Current turn components:")]
		public GameObject TurnObjectParentPerspective;
		public GameObject TurnObjectParentTopDown;

		[Space(5f)]
		public Sprite CurrentColorBlackSprite;
		public Sprite CurrentColorWhiteSprite;
		public Image CurrentTurnColor;
		public Image CurrentTurnBlackColorImageForTopDownCamera;
		public Image CurrentTurnWhiteColorImageForTopDownCamera;

		[Header("Square/Checkers arrays:")]
		public List<SquareVisual> BlackSquares = new List<SquareVisual>();
		public List<SquareVisual> WhiteSquares = new List<SquareVisual>();
		public List<CheckerVisual> Checkers = new List<CheckerVisual>();

		[Header("Choose side components:")]
		public Toggle WhiteToggle;
		public Toggle BlackToggle;
		public ToggleGroup ToggleGroup;

		[Space(10)]
		public Image BlockRaycastImage;

		[Header("Checker materials:")]
		[SerializeField]
		private Material _whiteCheckerMaterial;

		[SerializeField]
		private Material _blackCheckerMaterial;
		[SerializeField]
		private Material _shadowCheckerMaterial;

		[Header("Checker sprites")]
		[SerializeField]
		private Sprite _blackCheckerSprite;
		[SerializeField]
		private Sprite _whiteCheckerSprite;

		[Header("Checker shadow colors:")]
		[SerializeField]
		private Color _shadowWhiteColor;
		[SerializeField]
		private Color _shadowBlackColor;

		[Header("Checker start position:")]
		[SerializeField]
		private Transform _startPositionForBlack;
		[SerializeField]
		private Transform _startPositionForWhite;

		private CheckerColor _choosenCheckerColor;
		private readonly IDictionary<int, CheckerVisual> _checkersViews = new Dictionary<int, CheckerVisual>();
		private readonly IDictionary<int, SquareVisual> _squaresViews = new Dictionary<int, SquareVisual>();
		private int _squareSize;
		private float _blackRemoveStackPosition;
		private float _whiteRemoveStackPosition;
		private bool _isRandomColor;
		private bool _isBoardPreparing;
		private Color _blackSquareColor;
		private IEnumerator _startGameCoroutine; 
		private IEnumerator _randColorCoroutine; 

		/// <summary>
		/// Initialize of checkers stacks.
		/// </summary>
		private void InitCheckersStacks()
		{
			_blackRemoveStackPosition = 0f;
			_whiteRemoveStackPosition = 0f;
		}

		/// <summary>
		/// Initialize color of all checkers on start game.
		/// </summary>
		public void InitColor()
		{
			foreach (var item in Checkers)
			{
				item.CheckerRenderer.material = new Material(_whiteCheckerMaterial);
			}
		}

		/// <summary>
		/// Called after click on black/white color buttons.
		/// </summary>
		/// <param name="userColor"></param>
		public void SetColorPlayer(UserColor userColor)
		{
			if (_isRandomColor)
				return;

			BlockRaycastImage.enabled = true;

			GameControllerComponent.CurrentUserColor = userColor;

			if (userColor == UserColor.White)
			{
				_choosenCheckerColor = CheckerColor.White;
			}
			else
			{
				_choosenCheckerColor = CheckerColor.Black;
			}
			SetMaterialToShadowChecker(userColor);

			StartNewGame();
		}

		/// <summary>
		/// Start game action.
		/// </summary>
		private void StartNewGame()
		{
			if (_startGameCoroutine != null)
			{
				StopCoroutine(_startGameCoroutine);
			}
			_startGameCoroutine= StartGame();
			StartCoroutine(_startGameCoroutine);
		}

		/// <summary>
		/// Start of game coroutine.
		/// </summary>
		private IEnumerator StartGame()
		{
			yield return new WaitForSeconds(.5f);
			//ToggleGroup.SetAllTogglesOff();
			GameControllerComponent.StartGame();
			BlockRaycastImage.enabled = false;
		}

		/// <summary>
		/// Called after click on random color button.
		/// </summary>
		public void IsRandomColor()
		{
			if (_randColorCoroutine != null)
			{
				StopCoroutine(_randColorCoroutine);
			}
			_randColorCoroutine = RandColor();
			StartCoroutine(_randColorCoroutine);
		}

		/// <summary>
		/// User color Randomization action.
		/// </summary>
		private IEnumerator RandColor()
		{
			_isRandomColor = true;

			BlockRaycastImage.enabled = true;

			yield return new WaitForSeconds(.5f);

			int count = Random.Range(5, 10);

			for (int i = 1; i <= count; i++)
			{
				AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Random);
				WhiteToggle.isOn = true;
				yield return new WaitForSeconds(.5f / i);
				AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Random);
				BlackToggle.isOn = true;
				yield return new WaitForSeconds(.5f / i);
			}

			yield return new WaitForSeconds(.5f / count);

			_isRandomColor = false;

			ToggleGroup.SetAllTogglesOff();

			AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Random);
			if (count % 2 == 0)
			{
				WhiteToggle.isOn = true;
			}
			else
			{
				BlackToggle.isOn = true;
			}
		}

		/// <summary>
		/// Board initialization.
		/// </summary>
		public void OnBoardPrepared(List<Square> squaresInitialPositions, List<Checker> checkersInitialPositions)
		{
			_isBoardPreparing = true;

			InitCheckersStacks();

			_squareSize = squaresInitialPositions.Max(square => square.Position.Y) + 1;

			int blackCounter = 0, whiteCounter = 0;
			foreach (var square in squaresInitialPositions)
			{
				SquareVisual squareView;

				if (square.Color == SquareColor.White)
				{
					squareView = WhiteSquares[whiteCounter];
					whiteCounter++;
				}
				else
				{
					squareView = BlackSquares[blackCounter];
					squareView.ShadowRenderer.enabled = false;
					blackCounter++;
				}

				squareView.transform.localPosition = SquareScreenPosition(square.Position);

				squareView.Init(square.Id);
				_squaresViews[square.Id] = squareView;

				if (square.Color == SquareColor.Black && _blackSquareColor == Color.clear)
					_blackSquareColor = squareView.SquareMaterial.color;
			}

			float intervalWhite = 0.15f, intervalBlack = 0.15f;

			int CheckerCounter = 0;

			foreach (var checker in checkersInitialPositions)
			{
				CheckerVisual checkerView = Checkers[CheckerCounter];

				checkerView.SuperCheckerRenderer.enabled = false;

				if (checker.Color == CheckerColor.White)
				{
					checkerView.CheckerRenderer.material = new Material(_whiteCheckerMaterial);  //(_choosenCheckerColor == CheckerColor.White) ? new Material(_whiteCheckerMaterial) : new Material(_blackCheckerMaterial);
					checkerView.transform.localPosition = new Vector3(_startPositionForWhite.position.x, _startPositionForWhite.position.y + intervalWhite, _startPositionForWhite.position.z);
					intervalWhite += 0.15f;
				}
				else
				{
					checkerView.CheckerRenderer.material = new Material(_blackCheckerMaterial); //(_choosenCheckerColor == CheckerColor.Black) ? new Material(_whiteCheckerMaterial) : new Material(_blackCheckerMaterial);
					checkerView.transform.localPosition = new Vector3(_startPositionForBlack.position.x, _startPositionForBlack.position.y + intervalBlack, _startPositionForBlack.position.z);
					intervalBlack += 0.15f;
				}
				CheckerCounter++;
				checkerView.Init(checker.Id);
				_checkersViews[checker.Id] = checkerView;
				checkerView.CheckerCollider.enabled = true;
			}

			StartCoroutine(StartAnimation(checkersInitialPositions));
		}

		/// <summary>
		/// Called when user click on checker.
		/// </summary>
		/// <param name="id"></param>
		public void OnCheckerClicked(int id)
		{
			if (_isBoardPreparing)
			{
				return;
			}

			Checker choosenChecker = GameControllerComponent.CoreInstance._checkersData[id];

			if (choosenChecker != null)
			{
				if (GameControllerComponent.CoreInstance.IsEnemyMove)
				{
					ShakeChecker(choosenChecker);
					return;
				}


				GameControllerComponent.CoreInstance.FindAvailableSquares(id);
			}
		}

		/// <summary>
		/// Called when user click on square.
		/// </summary>
		/// <param name="squareId"></param>
		public void OnSquareClicked(int squareId)
		{
			if (_isBoardPreparing)
			{
				return;
			}

			GameControllerComponent.CoreInstance.TryToMoveChecker(squareId);
		}

		/// <summary>
		/// Dealing checkers animation on game start.
		/// </summary>
		private IEnumerator StartAnimation(List<Checker> checker)
		{
			for (int i = checker.Count / 2; i > 0; i--)
			{
				var viewEnemy = _checkersViews[checker[i - 1].Id];
				viewEnemy.transform.DOKill();
				viewEnemy.transform.DOLocalMove(CheckerScreenPosition(checker[i - 1].Square.Position), 0.2f).SetEase(Ease.OutSine);
				AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Move);

				var viewPlayer = _checkersViews[checker[(checker.Count / 2) + i - 1].Id];
				viewPlayer.transform.DOKill();
				viewPlayer.transform.DOLocalMove(CheckerScreenPosition(checker[(checker.Count / 2) + i - 1].Square.Position), 0.2f).SetEase(Ease.OutSine);
				AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Move);

				yield return new WaitForSeconds(0.1f);
			}

			_isBoardPreparing = false;
		}

		/// <summary>
		/// Position of square at screen.
		/// </summary>
		private Vector3 SquareScreenPosition(Position position)
		{
			return new Vector3(0.5f - _squareSize / 2f + position.X, 0, -0.5f + _squareSize / 2f - position.Y);
		}

		/// <summary>
		/// Position of checker at screen.
		/// </summary>
		private Vector3 CheckerScreenPosition(Position position)
		{
			return new Vector3(0.5f - _squareSize / 2f + position.X, .28f, -0.5f + _squareSize / 2f - position.Y);
		}

		/// <summary>
		/// Change shadow material depending on color of user checker.
		/// </summary>
		private void SetMaterialToShadowChecker(UserColor userColor)
		{
			_shadowCheckerMaterial.color = userColor == UserColor.White ? _shadowWhiteColor : _shadowBlackColor;
		}

		/// <summary>
		/// Called when need change current turn.
		/// </summary>
		public void ChangeCurrentTurnImage()
		{
			if (CameraControllerComponent.IsPerspectiveCamera)
			{
				InitTopDownCurrentTurnObectsValues();
			}
			else
			{
				InitPerspectiveCurrentTurnObectsValues();
			}
		}

		/// <summary>
		/// Called when need change current turn.
		/// </summary>
		public void InitCurrentTurnObjects()
		{
			TurnObjectParentPerspective.SetActive((CameraControllerComponent.IsPerspectiveCamera) ? true : false);
			TurnObjectParentTopDown.SetActive((CameraControllerComponent.IsPerspectiveCamera) ? false : true);

			ChangeCurrentTurnImage();
		}

		/// <summary>
		/// Change cuurent turn player sprite of top/down objects.
		/// </summary>
		private void InitTopDownCurrentTurnObectsValues()
		{
			if (GameControllerComponent.CoreInstance.CurrentMoveColor == CheckerColor.White)
			{
				CurrentTurnColor.sprite = CurrentColorWhiteSprite;//() ? CurrentColorWhiteSprite : CurrentColorBlackSprite;
			}
			else
			{
				CurrentTurnColor.sprite = CurrentColorBlackSprite;//(_choosenCheckerColor == CheckerColor.Black) ? CurrentColorWhiteSprite : ;
			}
		}

		/// <summary>
		/// Switch visual elements of current turn player of perspective objects.
		/// </summary>
		private void InitPerspectiveCurrentTurnObectsValues()
		{
			if (GameControllerComponent.CoreInstance.CurrentMoveColor == CheckerColor.White)
			{
				CurrentTurnWhiteColorImageForTopDownCamera.enabled = (GameControllerComponent.CoreInstance.CurrentMoveColor == CheckerColor.White);
				CurrentTurnBlackColorImageForTopDownCamera.enabled = !(GameControllerComponent.CoreInstance.CurrentMoveColor == CheckerColor.White);
				CurrentTurnWhiteColorImageForTopDownCamera.sprite = CurrentColorWhiteSprite; //(_choosenCheckerColor == CheckerColor.White) ? CurrentColorWhiteSprite : CurrentColorBlackSprite;
			}
			else
			{
				CurrentTurnWhiteColorImageForTopDownCamera.enabled = !(GameControllerComponent.CoreInstance.CurrentMoveColor == CheckerColor.Black);
				CurrentTurnBlackColorImageForTopDownCamera.enabled = (GameControllerComponent.CoreInstance.CurrentMoveColor == CheckerColor.Black);
				CurrentTurnBlackColorImageForTopDownCamera.sprite = CurrentColorBlackSprite; //(_choosenCheckerColor == CheckerColor.Black) ? CurrentColorWhiteSprite : CurrentColorBlackSprite;
			}
		}

		/// <summary>
		/// Move checker animation.
		/// </summary>
		/// <param name="checker"></param>
		public void OnMoveChecker(Checker checker)
		{
			var view = _checkersViews[checker.Id];
			AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Move);
			view.transform.DOKill();
			view.transform.DOLocalMove(CheckerScreenPosition(checker.Square.Position), 0.4f).SetEase(Ease.OutSine);
		}

		/// <summary>
		/// Remove checker action.
		/// </summary>
		/// <param name="checker"></param>
		public void OnRemoveChecker(Checker checker)
		{
			var view = _checkersViews[checker.Id];

			Vector3 stackPosition;
			if (checker.Color == CheckerColor.White)
			{
				_blackRemoveStackPosition += 0.15f;
				stackPosition = new Vector3(_startPositionForBlack.position.x,
											_startPositionForBlack.position.y + _blackRemoveStackPosition,
											_startPositionForBlack.position.z);

			}
			else
			{
				_whiteRemoveStackPosition += 0.15f;
				stackPosition = new Vector3(_startPositionForWhite.position.x,
											_startPositionForWhite.position.y + _whiteRemoveStackPosition,
											_startPositionForWhite.position.z);
			}

			Vector3 upPosition = CheckerScreenPosition(checker.Square.Position);
			view.transform.DOKill(true);
			view.transform.DOLocalMove(new Vector3(upPosition.x, upPosition.y + 0.2f, upPosition.z), 1f).SetEase(Ease.OutSine).OnComplete(delegate { CompleteRemoveAnimation(view, stackPosition, checker.Id); });
			view.CheckerCollider.enabled = false;
		}

		/// <summary>
		/// Remove animation of checker.
		/// </summary>
		private void CompleteRemoveAnimation(CheckerVisual view, Vector3 stackPosition, int Id)
		{
			view.transform.DOKill();
			view.transform.DOLocalMove(stackPosition, 0.2f).SetEase(Ease.InExpo);
			_checkersViews.Remove(Id);
		}

		/// <summary>
		/// Shake animation of checker.
		/// </summary>
		/// <param name="checker"></param>
		public void ShakeChecker(Checker checker)
		{
			var view = _checkersViews[checker.Id];
			view.transform.DOKill();
			view.transform.DOShakePosition(0.2f, new Vector3(0.05f, 0, 0.05f), vibrato: 15);
		}

		/// <summary>
		/// Turn on shadows of available moves for checker.
		/// </summary>
		public void MarkSquares(List<Square> availableSquares)
		{
			foreach (var square in availableSquares)
			{
				var view = _squaresViews[square.Id];
				view.ShadowRenderer.enabled = true;
			}
		}

		/// <summary>
		/// Turn off shadows of available moves for checker.
		/// </summary>
		public void UnmarkSquares(List<Square> availableSquares)
		{
			foreach (var square in availableSquares)
			{
				var view = _squaresViews[square.Id];
				view.ShadowRenderer.enabled = false;
			}
		}

		/// <summary>
		/// Change state of checker to Super.
		/// </summary>
		public void SetSuperChecker(Checker checker)
		{
			var view = _checkersViews[checker.Id].SuperCheckerRenderer;
			view.enabled = true;

			if (checker.Color == CheckerColor.White)
			{
				view.sprite = _blackCheckerSprite; //(_choosenCheckerColor == CheckerColor.White) ? _blackCheckerSprite : _whiteCheckerSprite;
			}
			else
			{
				view.sprite = _whiteCheckerSprite; //(_choosenCheckerColor == CheckerColor.Black) ? _blackCheckerSprite : _whiteCheckerSprite;
			}
		}

		public void Reset()
		{
			InitCheckersStacks();
		}
	}
}
