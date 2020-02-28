using DG.Tweening;
using UnityEngine;

namespace Checkers
{
	public class Initializer : MonoBehaviour
	{
		[Header("Components: ")]
		public BoardController BoardViewComponent;
		//public AdsController AdsControllerComponent;

		private void Awake()
		{
			InitializeDOTween();
			BoardViewComponent.InitColor();
			//AdsControllerComponent.InitializeADS();
		}

		/// <summary>
		/// Init dotween for freeze excluding.
		/// </summary>
		private void InitializeDOTween()
		{
			transform.DOKill();
			transform.DOLocalMove(Vector3.zero, 0.001f);
			transform.DORotate(Vector3.zero, 0.001f);
			transform.DOShakePosition(0.001f, Vector3.zero, vibrato: 10);

			float variable = 0f;
			DOTween.To(() => variable, x => variable = x, 0f, 0.001f);
		}
	}
}