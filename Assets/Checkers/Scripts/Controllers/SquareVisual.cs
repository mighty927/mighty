#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Checkers
{
	public class SquareVisual : MonoBehaviour
	{
		public int Id;
		public Material SquareMaterial;
		public MeshRenderer ShadowRenderer;

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
			BoardController.Instance.OnSquareClicked(Id);
		}

		public void OnDrawGizmos()
		{
#if UNITY_EDITOR
			//Show positions in scene using Handles
			if (!Application.isMobilePlatform && Application.isPlaying)
			{
				Square square = BoardController.Instance.GameControllerComponent.CoreInstance.GetSquare(Id);
				if (square != null)
				{
					GUIStyle style = new GUIStyle(EditorStyles.textField) { normal = new GUIStyleState() { textColor = Color.red }, fontStyle = FontStyle.Bold};

					Handles.Label(transform.position, string.Format("({0}:{1})",square.Position.X.ToString(), square.Position.Y.ToString()), style);
				}
			}
#endif
		}
	}
}
