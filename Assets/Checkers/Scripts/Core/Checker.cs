using NetMessaging.GameLogic;

namespace Checkers
{
	[System.Serializable]
    public class Checker
    {
		/// <summary>
		/// Checker id.
		/// </summary>
		public int Id;

		/// <summary>
		/// Reference to square.
		/// </summary>
		public Square Square;

		/// <summary>
		/// Color of checker.
		/// </summary>
		public CheckerColor Color;

		/// <summary>
		/// True when checker is super.
		/// </summary>
		public bool IsSuperChecker;

		/// <summary>
		/// True when checker was beat.
		/// </summary>
		public bool IsBeat;

		/// <summary>
		/// Create new checker instance.
		/// </summary>
        public Checker(int id, Square square, CheckerColor color)
        {
            Id = id;
            Square = square;
            Color = color;
            IsSuperChecker = false;
            IsBeat = false;
        }

		/// <summary>
		/// Change square ref of checker
		/// </summary>
		/// <param name="square"></param>
		public void MoveTo(Square square)
        {
            Square = square;
        }

		/// <summary>
		/// Set super checker state.
		/// </summary>
        public void BecomeSuperChecker()
        {
            IsSuperChecker = true;
        }

		/// <summary>
		/// Set beat state.
		/// </summary>
        public void WasBeat()
        {
            IsBeat = true;
        }
    }
}
