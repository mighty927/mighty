using NetMessaging.GameLogic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Checkers
{
	public class GameController :  MonoBehaviour
	{
		public UnityEvent StartNewGameEvent;

		[Header("Components: ")]
		public BoardController BoardViewCompoennt;
		public UiViewController UiViewComponent;

		[Header("States: ")]
		public bool IsGameStart;
		public UserColor CurrentUserColor;


		[Space]
		public Core CoreInstance;

		public IEnumerator _moveCoroutine { get; set; }

		public GameMode Mode;

		public void Awake()
		{
			IsGameStart = false;
			if(Client.instance.StartCommand && Client.instance?.lobbyInfo?.GameMode == GameMode.Mining)
			{
				StartMining();
			}
			else if(Client.instance.StartCommand && Client.instance?.lobbyInfo?.GameMode == GameMode.PVP)
			{
				StartPVPGame();
			}
			else
			{
				//close scene;
			}
		}


		private void Update()
		{
			if(IsGameStart)
			{
				if (CoreInstance.CurrentMoveColor == CheckerColor.Black)
				{
					if(Mode == GameMode.Mining)
					{
						if (!CoreInstance.IsEnemyMove)
						{
							if (_moveCoroutine != null)
							{
								StopCoroutine(_moveCoroutine);
							}

							_moveCoroutine = CoreInstance.BotMove();
							StartCoroutine(_moveCoroutine);
						}
					}
				}

				if (NetworkController.instance?.GameEndCommand == true)
				{
					if (UserContoller.instance.endGameCoroutine != null)
					{
						StopCoroutine(UserContoller.instance.endGameCoroutine);
						StartCoroutine(UserContoller.instance.endGameCoroutine);
					}
				}
			}
		}

		


		public void StartPVPGame()
		{
			//START PVP
			var firstPlayer = Client.instance?.lobbyInfo?.FirstPlayer;
			var secondPlayer = Client.instance?.lobbyInfo?.SecondPlayer;
			var firstTurnNamePlayer = Client.instance?.lobbyInfo?.FirstTurnPlayerName;
			var me = Client.instance?.connectedClient?.currentUser?.UserName;

			UiViewComponent.FirstPlayerName = firstPlayer;
			UiViewComponent.SecondPlayerName = secondPlayer;
			ChooseMode(GameMode.PVP);


			if (firstTurnNamePlayer.Equals(me))
			{
				Client.instance.UserCheckerColor = CheckerColor.White;
				BoardViewCompoennt.SetColorPlayer(UserColor.White);
			}
			else
			{
				Client.instance.UserCheckerColor = CheckerColor.Black;
				BoardViewCompoennt.SetColorPlayer(UserColor.Black);

				if (_moveCoroutine != null)
					StopCoroutine(_moveCoroutine);

				_moveCoroutine = CoreInstance.SecondPlayerMove();
				StartCoroutine(_moveCoroutine);
			}

			IsGameStart = true;
		}

		public void StartMining()
		{
			//STRT MINING
			UiViewComponent.FirstPlayerName = Client.instance?.connectedClient?.currentUser?.UserName;
			ChooseMode(GameMode.Mining);
			BoardViewCompoennt.SetColorPlayer(UserColor.White);

			IsGameStart = true;
		}


		/// <summary>
		/// Start game action.
		/// </summary>
		public void StartGame()
		{
			IsGameStart = true;

			UiViewComponent.SetUIByGameState(!IsGameStart);
			UiViewComponent.UpdateGameScore(12, 12);
			UiViewComponent.UpdateUserNames();
			UiViewComponent.MainButtonActivate();

			CoreInstance = new Core();
			BoardViewCompoennt.InitCurrentTurnObjects();

			//CoreInstance.FirstTurnPlayer((CurrentUserColor == UserColor.White) ? CheckerColor.White : CheckerColor.Black);
			CoreInstance.FirstTurnPlayer();

			
			CoreInstance.PrepareBoard(CurrentUserColor);
			StartNewGameEvent?.Invoke();
		}

		/// <summary>
		/// Restart game action.
		/// </summary>
		public void Restart()
		{
			IsGameStart = false;

			if (_moveCoroutine != null)
			{
				StopCoroutine(_moveCoroutine);
			}

			BoardViewCompoennt.Reset();
			UiViewComponent.ResetUIView();
			StartGame();
		}

		/// <summary>
		/// Change game mode.
		/// </summary>
		public void ChooseMode(GameMode mode)
		{
			Mode = mode;
		}
	}
}