using System;

namespace Checkers
{
	public class Position
	{
		public readonly int X;

		public readonly int Y;

		public Position(int x, int y)
		{
			X = x;
			Y = y;
		}

		public Position(Position other)
		{
			X = other.X;
			Y = other.Y;
		}

		public Position(Square left, Square right, int i, bool inverse)
		{
			if (inverse)
			{
				if (left.Position.X > right.Position.X)
				{
					X = right.Position.X + i;
				}
				else
				{
					X = right.Position.X - i;
				}

				if (left.Position.Y > right.Position.Y)
				{
					Y = right.Position.Y + i;
				}
				else
				{
					Y = right.Position.Y - i;
				}
			}
			else
			{
				if (left.Position.X > right.Position.X)
				{
					X = right.Position.X - i;
				}
				else
				{
					X = right.Position.X + i;
				}

				if (left.Position.Y > right.Position.Y)
				{
					Y = right.Position.Y - i;
				}
				else
				{
					Y = right.Position.Y + i;
				}
			}
		}

		public Position Abs()
		{
			return new Position(Math.Abs(X), Math.Abs(Y));
		}

		public static Position operator -(Position leftPos, Position rightPos)
		{
			return new Position(leftPos.X - rightPos.X, leftPos.Y - rightPos.Y);
		}

		public static Position operator +(Position leftPos, Position rightPos)
		{
			return new Position(leftPos.X + rightPos.X, leftPos.Y + rightPos.Y);
		}

		public static Position operator *(int multiplier, Position rightPos)
		{
			return new Position(multiplier * rightPos.X, multiplier * rightPos.Y);
		}

		public static Position operator /(Position leftPos, int value)
		{
			return new Position(leftPos.X / value, leftPos.Y / value);
		}

		public static bool operator ==(Position leftPos, Position rightPos)
		{
			return leftPos.X == rightPos.X && leftPos.Y == rightPos.Y;
		}

		public static bool operator !=(Position leftPos, Position rightPos)
		{
			return !(leftPos == rightPos);
		}
	}
}