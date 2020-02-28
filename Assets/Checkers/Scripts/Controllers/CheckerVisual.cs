using UnityEngine;

namespace Checkers
{
	public class CheckerVisual : MonoBehaviour
	{
		public int Id;
		public Renderer CheckerRenderer;
		public Collider CheckerCollider;
		public SpriteRenderer SuperCheckerRenderer;

		/// <summary>
		/// Initialize checker with id.
		/// </summary>
		public void Init(int id)
		{
			Id = id;
		}

		public void OnMouseDown()
		{
			OnMouseDownClick();
		}

		/// <summary>
		/// Called whe player click on checker.
		/// </summary>
		private void OnMouseDownClick()
		{
			BoardController.Instance.OnCheckerClicked(Id);
		}
	}
}
