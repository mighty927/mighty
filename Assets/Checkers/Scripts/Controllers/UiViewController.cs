using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using NetMessaging.GameLogic;

namespace Checkers
{
    public class UiViewController : Singleton<UiViewController>
    {
        [Header("For Disable")]
        public List<GameObject> UIForDisable;

        [Header("For Enable")]
        public List<GameObject> UIForEnable;

		[Header("UI Panels:")]
		public GameObject WinPanel;
		public GameObject InfoPanel;
		public GameObject ChooseSidePanel;
		public GameObject ChooseModePanel;

		[Header("UI Buttons:")]
		public Image ResetCameraImage;
		public Image NoAdsImage;

		[Header("UI components:")]
		public Image MainButttonImage;
		public Text WinMessageText;
		public TextMesh EnemyScoreTextMesh;
		public TextMesh PlayerScoreTextMesh;
		[Space]
		public TextMesh EnemyNameTextMesh;
		public TextMesh PlayerNameTextMesh;

		public UnityEvent ReplayRequested;

        public UnityEvent ChangeCamera;

        public UnityEvent ShowNoAdsAction;

		[Header("Info texts:")]
		//public string PlayerOneName = "Player 1";
		public string SecondPlayerName = "Player 2";
		public string FirstPlayerName = "You";
		public string AiName = "Computer";

		/// <summary>
		/// Enable reset camera button.
		/// </summary>
		public void OnActivateResetCameraButton()
        {
			ResetCameraImage.enabled = true;
        }

		/// <summary>
		/// Disable reset camera button.
		/// </summary>
		public void OnDeactivateResetCameraButton()
		{
			ResetCameraImage.enabled = false;
		}

		/// <summary>
		/// Disable no ads button.
		/// </summary>
		public void DeactivateNoAdsButton()
        {
            NoAdsImage.enabled = (false);
        }

		/// <summary>
		/// Show no ads info pop up.
		/// </summary>
		public void ShowNoAdsInfoPopUp()
        {
            AlertPopUpController.ShowAlertPopUp("After watching rewarded video Ads will be disappear for one game session. Video start from " + DataConfig.Instance.NoAdsRewardAppearDelay + " seconds.");
			StartCoroutine(NoAdsClickedCoroutine());
        }

		/// <summary>
		/// Show win panel with message of user win.
		/// </summary>
		public void ShowUserWinMessage()
        {
			GameMode mode = BoardController.Instance.GameControllerComponent.Mode;

			WinMessageText.text = $"{FirstPlayerName} Победил";
            WinPanel.SetActive(true);
        }

		public void ShowWinMessage(string victory)
		{			

			WinMessageText.text = $"{victory} Победил!";
			WinPanel.SetActive(true);
		}

		/// <summary>
		/// Show win panel with message of AI win.
		/// </summary>
		public void ShowAIWinMessage()
		{
			GameMode mode = BoardController.Instance.GameControllerComponent.Mode;

			WinMessageText.text = $"{(mode == GameMode.Mining ? AiName : SecondPlayerName)}  Победил";
			WinPanel.SetActive(true);
		}

		/// <summary>
		/// Show win panel with message of user lose.
		/// </summary>
		public void ShowPlayerNoMovesMessage()
		{
			GameMode mode = BoardController.Instance.GameControllerComponent.Mode;

			WinMessageText.text = $"{(mode == GameMode.Mining ? AiName : SecondPlayerName)} Победил! У ({FirstPlayerName}) нет доступных шагов!";
			WinPanel.SetActive(true);
		}

		/// <summary>
		/// Show win panel with message of AI lose.
		/// </summary>
		public void ShowAINoMovesMessage()
		{
			GameMode mode = BoardController.Instance.GameControllerComponent.Mode;

			WinMessageText.text = $"{FirstPlayerName} Победил! Компьютер не смог противостоять!";
			WinPanel.SetActive(true);
		}

		/// <summary>
		/// Show info panel after click on Info button.
		/// </summary>
		public void ShowInfoPanel()
        {
            InfoPanel.SetActive(true);
        }

		/// <summary>
		/// Close choose side panel. Called when click on CloseButton button.
		/// </summary>
		public void CloseChooseSidePanel()
        {
            ChooseSidePanel.SetActive(false);
        }

		/// <summary>
		/// Enable/disable object depending on game state. 
		/// </summary>
        public void SetUIByGameState(bool state)
        {
            //UIForDisable.ForEach(x => x.gameObject.SetActive(state));
            UIForEnable.ForEach(x => x.gameObject.SetActive(!state));
        }

		/// <summary>
		/// Called when click on replay button.
		/// </summary>
		public void ReplayClicked()
        {
            ReplayRequested?.Invoke();
        }

		/// <summary>
		/// Called when click on Main Button.
		/// </summary>
		public void MainButtonActivate()
        {
            if(!MainButttonImage.enabled)
                MainButttonImage.enabled = true;
        }

		/// <summary>
		/// Called when click on ChangeCamera button.
		/// </summary>
		public void ChangeCameraClicked()
        {
            ChangeCamera?.Invoke();
		}

		private IEnumerator NoAdsClickedCoroutine()
		{
			yield return new WaitForSeconds(DataConfig.Instance.NoAdsRewardAppearDelay);
			NoAdsRewardedClicked();
		}

		/// <summary>
		/// Called when click on NoAds button.
		/// </summary>
		public void NoAdsRewardedClicked()
        {
            ShowNoAdsAction?.Invoke();
		}

		/// <summary>
		/// Open url when click on like us button.
		/// </summary>
        public void LikeUs(string url)
        {
            Application.OpenURL(url);
        }

		/// <summary>
		/// Reset ui view visual.
		/// </summary>
        public void ResetUIView()
        {
            WinPanel.SetActive(false);
            UpdateGameScore(12, 12);
			UpdateUserNames();
		}

		/// <summary>
		/// Change game score (user vs ai).
		/// </summary>
		public void UpdateGameScore(int playerScoreValue, int enemyScoreValue)
        {
            PlayerScoreTextMesh.text = playerScoreValue.ToString();
            EnemyScoreTextMesh.text = enemyScoreValue.ToString();
        }

		/// <summary>
		/// Close mode panel and Open choose color panel.
		/// </summary>
		public void OpenChooseSidePanel()
		{
			//ChooseSidePanel.SetActive(true);
			ChooseModePanel.SetActive(false);
		}

		/// <summary>
		/// Closed mode panel action.
		/// </summary>
		public void CloseChooseModePanel()
		{
			ChooseModePanel.SetActive(false);
		}

		/// <summary>
		/// Update user names fron fields in inspector.
		/// </summary>
		public void UpdateUserNames()
		{
			GameMode mode = BoardController.Instance.GameControllerComponent.Mode;

			PlayerNameTextMesh.text = FirstPlayerName;
			EnemyNameTextMesh.text = (mode == GameMode.Mining ? AiName : SecondPlayerName);
		}
	}
}
