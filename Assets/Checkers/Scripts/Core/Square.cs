using NetMessaging.GameLogic;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    [System.Serializable]
    public class Square
    {
		/// <summary>
		/// Id of square.
		/// </summary>
		public int Id;

		/// <summary>
		/// Position of square.
		/// </summary>
		public Position Position;

		/// <summary>
		/// Color of square.
		/// </summary>
		public SquareColor Color;

		/// <summary>
		/// Checker which placed on square.
		/// </summary>
		public Checker Checker;

		/// <summary>
		/// Create new Square instance.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="position"></param>
		/// <param name="color"></param>
        public Square(int id, Position position, SquareColor color)
        {
            Id = id;
            Position = position;
            Color = color;
        }

		/// <summary>
		/// Setup checker on square.
		/// </summary>
        public void SetChecker(Checker checker)
        {
            Checker = checker;
        }
        
		/// <summary>
		/// Is square contains checker.
		/// </summary>
        public bool HasChecker()
        {
            return Checker != null && Checker.Id != 0;
        }
        
		/// <summary>
		/// Return neibours of square.
		/// </summary>
		/// <param name="all"></param>
  //      public IEnumerable<Position> Neighbours(bool all, CheckerColor currentCheckerColor)
  //      {
  //          if (!HasChecker()) yield break;

  //          if (Checker.IsSuperChecker)
  //              all = true;
  //          if (all || Checker.Color  == currentCheckerColor)
  //          {
  //              //return top left
  //              yield return new Position(Position.X - 1, Position.Y - 1);
		//		//return right
		//		yield return new Position(Position.X + 1, Position.Y - 1);
  //          }

		//	if (all || Checker.Color != currentCheckerColor)
		//	{
		//		//return bottom left
		//		yield return new Position(Position.X - 1, Position.Y + 1);
		//		//return bottom right
		//		yield return new Position(Position.X + 1, Position.Y + 1);
		//	}
		//}

        public IEnumerable<Position> Neighbours(bool all, CheckerColor currentCheckerColor, GameMode gameMode)
        {
            if (!HasChecker()) yield break;

            if (gameMode == GameMode.PVP)
            {
                if (all || Checker.Color == currentCheckerColor)
                {
                    //return top left
                    yield return new Position(Position.X - 1, Position.Y - 1);
                    //return right
                    yield return new Position(Position.X + 1, Position.Y - 1);
                }

                if (all || Checker.Color != currentCheckerColor)
                {
                    //return bottom left
                    yield return new Position(Position.X - 1, Position.Y + 1);
                    //return bottom right
                    yield return new Position(Position.X + 1, Position.Y + 1);
                }
            }
            else
            {
                if (all || currentCheckerColor == CheckerColor.Black)
                {
                    //return bottom left
                    yield return new Position(Position.X - 1, Position.Y + 1);
                    //return bottom right
                    yield return new Position(Position.X + 1, Position.Y + 1);
                }
                
                if(all || currentCheckerColor == CheckerColor.White)
                {
                    //return top left
                    yield return new Position(Position.X - 1, Position.Y - 1);
                    //return right
                    yield return new Position(Position.X + 1, Position.Y - 1);
                }
            }
        }



        public IEnumerable<Position> NeighboursSuperRightTop(int i)
        {
            if (!HasChecker()) yield break;

            yield return new Position(Position.X + i, Position.Y - i);

        }

        public IEnumerable<Position> NeighboursSuperLeftTop(int i)
        {
            if (!HasChecker()) yield break;
            yield return new Position(Position.X - i, Position.Y - i);
        }


        public IEnumerable<Position> NeighboursSuperRightBottom(int i)
        {
            if (!HasChecker()) yield break;
            yield return new Position(Position.X + i, Position.Y + i);

        }

        public IEnumerable<Position> NeighboursSuperLeftBottom(int i)
        {
            if (!HasChecker()) yield break;
            yield return new Position(Position.X - i, Position.Y + i);
        }
    }
}