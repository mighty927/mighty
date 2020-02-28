using NetMessaging.GameLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Checkers
{
	[Serializable]
	public class Core 
	{
		public bool IsEnemyMove { get; set; }
		public bool IsBeatProcessActive { get; set; }
		public bool GameEnd { get; set; }

		[Header("Data arrays: ")]
		public List<Square> AvailableSquares = new List<Square>();
		//public List<Square> AvailableSquaresForBlack = new List<Square>();

		private const int _boardSize = 8;

		private const int _checkerCount = 12;

		private readonly Square[,] _boardSquares;

		private readonly List<Square> _allAvailableSquares = new List<Square>();

		public Checker _currentChecker;

		public IDictionary<int, Square> _squaresData = new Dictionary<int, Square>();

		public IDictionary<int, Checker> _checkersData = new Dictionary<int, Checker>();

		private CheckerColor _currentMoveCollor;
		public CheckerColor CurrentMoveColor
		{
			get
			{
				return _currentMoveCollor;
			}
			set
			{
				_currentMoveCollor = value;
				BoardController.Instance.ChangeCurrentTurnImage();
			}
		}

		/// <summary>
		/// Create new Game instance
		/// </summary>
		public Core()
		{
			_boardSquares = new Square[_boardSize, _boardSize];
			_allAvailableSquares = new List<Square>();
			GameEnd = false;
		}

		/// <summary>
		/// Setup first turn player.
		/// </summary>
		public void FirstTurnPlayer()
		{
			CurrentMoveColor = CheckerColor.White;
		}

		/// <summary>
		/// Create board(squares and checkers).
		/// </summary>
		/// <param name="userColor"></param>
		public void PrepareBoard(UserColor userColor)
		{
			for (int i = 0; i < _boardSize; i++)
			{
				for (int j = 0; j < _boardSize; j++)
				{
					var position = new Position(i, j);
					var id = j * _boardSize + i;
					SquareColor squareColor = (i + j) % 2 == 0 ? SquareColor.White : SquareColor.Black;

					Square square = new Square(id, position, squareColor);
					_boardSquares[i, j] = square;
					_squaresData[square.Id] = square;
				}
			}

			int c = 0, index = 0;
			while (c < 2)
			{
				if (_boardSquares[index / _boardSize, index % _boardSize].Color == SquareColor.Black)
				{
					var checkerColor = userColor == UserColor.White ? CheckerColor.Black : CheckerColor.White;
					Checker checker = new Checker(index, _squaresData[index], checkerColor);
					_checkersData[index] = checker;
					_squaresData[index].SetChecker(checker);
					c++;
				}
				index++;
			}

			c = 0;
			index = _boardSize * _boardSize - 1;
			while (c < 2)
			{
				if (_boardSquares[index / _boardSize, index % _boardSize].Color == SquareColor.Black)
				{
					var checkerColor = userColor == UserColor.White ? CheckerColor.White : CheckerColor.Black;
					Checker checker = new Checker(index, _squaresData[index], checkerColor);
					checker.IsSuperChecker = true;
					_checkersData[index] = checker;
					_squaresData[index].SetChecker(checker);
					c++;
				}
				index--;
			}
			BoardController.Instance.OnBoardPrepared(_squaresData.Values.ToList(), _checkersData.Values.ToList());
		}

		/// <summary>
		/// When checker is chosen we need to find empty squares to move on.
		/// </summary>
		public void FindAvailableSquares(int checkerId)
		{

			if (GameEnd)
			{
				return;
			}

			//If user do not beat checkers yet 
			if (!IsBeatProcessActive || IsEnemyMove && BoardController.Instance.GameControllerComponent.Mode == GameMode.Mining)
			{
				_currentChecker = null;
				ClearAvailableSquares();
			}

			Checker choosenChecker = _checkersData[checkerId];
			List<Checker> beatCheckers = FindBeatCheckers();


			if (choosenChecker.Color == CurrentMoveColor && beatCheckers.Count == 0)
			{
				if (choosenChecker.IsSuperChecker)
				{
					FindAvailableSquaresForSuperTurn(choosenChecker);
				}
				else
				{
					foreach (var neighbourSquarePosition in choosenChecker.Square.Neighbours(false, _currentMoveCollor, Client.instance.lobbyInfo.GameMode))
					{
						if (IsOnBoard(neighbourSquarePosition))
						{
							Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];

							if (!neighbourSquare.HasChecker())
								_allAvailableSquares.Add(neighbourSquare);
						}
					}
				}
			}
			else if (choosenChecker.Color == CurrentMoveColor && beatCheckers.Count != 0 && beatCheckers.Contains(choosenChecker))
			{

				if (choosenChecker.IsSuperChecker)
				{
					FindAvailableSquaresForSuperBeat(choosenChecker);
				}
				else
				{
					foreach (var neighbourSquarePosition in choosenChecker.Square.Neighbours(true, _currentMoveCollor, Client.instance.lobbyInfo.GameMode))
					{
						if (IsOnBoard(neighbourSquarePosition))
						{
							Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
							if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
							{
								Square secondNextSquare = GetSecondNextSquare(choosenChecker.Square, neighbourSquare);
								if (secondNextSquare != null && !secondNextSquare.HasChecker())
									_allAvailableSquares.Add(secondNextSquare);
							}
						}
					}
				}
			}

			if ((BoardController.Instance.GameControllerComponent.Mode == GameMode.Mining &&
				choosenChecker.Color == CheckerColor.White) || BoardController.Instance.GameControllerComponent.Mode == GameMode.PVP)
			{
				if (_allAvailableSquares.Count == 0)
				{
					BoardController.Instance.ShakeChecker(choosenChecker);
				}
				else
				{
					if (!IsBeatProcessActive)
					{
						_currentChecker = choosenChecker;
					}

					BoardController.Instance.MarkSquares(_allAvailableSquares);
				}
			}
		}

		public void FindAvailableSquaresForSuperTurn(Checker choosenChecker)
		{
			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in choosenChecker.Square.NeighboursSuperRightTop(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];

						if (!neighbourSquare.HasChecker())
							_allAvailableSquares.Add(neighbourSquare);
						else
						{
							i = 8;
							break;
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{

				foreach (var neighbourSquarePosition in choosenChecker.Square.NeighboursSuperLeftTop(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];

						if (!neighbourSquare.HasChecker())
							_allAvailableSquares.Add(neighbourSquare);
						else
						{
							i = 8;
							break;
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in choosenChecker.Square.NeighboursSuperRightBottom(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];

						if (!neighbourSquare.HasChecker())
							_allAvailableSquares.Add(neighbourSquare);
						else
						{
							i = 8;
							break;
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{

				foreach (var neighbourSquarePosition in choosenChecker.Square.NeighboursSuperLeftBottom(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];

						if (!neighbourSquare.HasChecker())
							_allAvailableSquares.Add(neighbourSquare);
						else
						{
							i = 8;
							break;
						}
					}
				}
			}
		}

		public void FindAvailableSquaresForSuperBeat(Checker choosenChecker)
		{
			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in choosenChecker.Square.NeighboursSuperRightTop(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(choosenChecker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
									_allAvailableSquares.Add(item);
								else
								{
									i = 8;
									break;
								}
							}
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in choosenChecker.Square.NeighboursSuperLeftTop(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(choosenChecker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
									_allAvailableSquares.Add(item);
								else
								{
									i = 8;
									break;
								}
							}
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in choosenChecker.Square.NeighboursSuperRightBottom(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(choosenChecker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
									_allAvailableSquares.Add(item);
								else
								{
									i = 8;
									break;
								}
							}
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in choosenChecker.Square.NeighboursSuperLeftBottom(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(choosenChecker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
									_allAvailableSquares.Add(item);
								else
								{
									i = 8;
									break;
								}
							}
						}
					}
				}
			}
		}

		public bool FindAvailableSquareForSuperMultiBeat(Checker checker)
		{
			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in checker.Square.NeighboursSuperRightTop(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(checker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
								{
									return true;
								}
								else
								{
									break;
								}

							}


							i = 8;
							break;
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in checker.Square.NeighboursSuperLeftTop(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{

							var NextSquares = NextSquaresList(checker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
								{
									return true;
								}
								else
									break;

							}
						}


						i = 8;
						break;
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in checker.Square.NeighboursSuperRightBottom(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(checker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
								{
									return true;
								}
								else
									break;

							}


							i = 8;
							break;
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in checker.Square.NeighboursSuperLeftBottom(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(checker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
								{
									return true;
								}
								else
									break;

							}

							i = 8;
							break;
						}

					}
				}
			}

			return false;
		}

		public List<Checker> FindBeatCheckersForSuper(Checker checker)
		{
			List<Checker> beatCheckers = new List<Checker>();
			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in checker.Square.NeighboursSuperRightTop(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(checker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
								{
									beatCheckers.Add(checker);
								}
								else
								{
									break;
								}

							}

							i = 8;
							break;
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in checker.Square.NeighboursSuperLeftTop(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{

							var NextSquares = NextSquaresList(checker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
								{
									beatCheckers.Add(checker);
								}
								else
								{
									break;
								}

							}

							i = 8;
							break;
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in checker.Square.NeighboursSuperRightBottom(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(checker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
								{
									beatCheckers.Add(checker);
								}
								else
								{
									break;
								}

							}

							i = 8;
							break;
						}
					}
				}
			}

			for (int i = 1; i < 8; i++)
			{
				foreach (var neighbourSquarePosition in checker.Square.NeighboursSuperLeftBottom(i))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
						{
							var NextSquares = NextSquaresList(checker.Square, neighbourSquare);

							foreach (var item in NextSquares)
							{
								if (item != null && !item.HasChecker())
								{
									beatCheckers.Add(checker);
								}
								else
								{
									break;
								}

							}

							i = 8;
							break;
						}
					}
				}
			}
			return beatCheckers;
		}

		private List<Square> NextSquaresList(Square currSquare, Square nextSquare)
		{
			List<Square> NextPositionList = new List<Square>();

			for (int i = 1; i < 7; i++)
			{
				Position secondNextPosition = new Position(currSquare, nextSquare, i, false);

				if (IsOnBoard(secondNextPosition))
				{
					var square = _boardSquares[secondNextPosition.X, secondNextPosition.Y];

					//if (square.HasChecker() && i > 1)
					//	throw new Exception();

					NextPositionList.Add(square);
				}
				else
				{
					break;
				}

			}
			return NextPositionList;
		}

		/// <summary>
		/// Update lists with checkers by color.
		/// </summary>
		public void FindAvailableSquaresForColor()
		{
			if (GameEnd)
				return;

			//AvailableSquaresForBlack.Clear();
			AvailableSquares.Clear();
			//_currentChecker = null;

			foreach (var item in _checkersData.Values)
			{
				foreach (var neighbourSquarePosition in item.Square.Neighbours(false, _currentMoveCollor, Client.instance.lobbyInfo.GameMode))
				{
					if (IsOnBoard(neighbourSquarePosition))
					{
						Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
						if (!neighbourSquare.HasChecker())
						{
							AvailableSquares.Add(neighbourSquare);
						}
					}
				}
			}

			if (AvailableSquares.Count == 0)
			{
				int checkersForBeat = FindBeatCheckers().Count;
				if (checkersForBeat == 0)
				{
					//UiViewController.Instance.ShowPlayerNoMovesMessage();
					//AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Lose);
					//AdsController.Instance.ShowRewardBasedVideo();
					GameEnd = true;
				}
			}
		
		}

		/// <summary>
		/// Move checker to square that available.
		/// </summary>
		public void TryToMoveChecker(int squareId, int? checkerId = null)
		{
			Square square = _squaresData[squareId];
			Checker checker = null;
			if (checkerId.HasValue)
			{
				checker = _checkersData[checkerId.Value];
			}
			else
			{
				checker = _currentChecker;
			}
			if (_allAvailableSquares.Contains(square))
			{
				IsBeatProcessActive = false;

				//If between square where you want to place your checker exist enemy checker
				if (checker != null)
				{
					Square intermediateSquare = HasIntermediateSquare(checker.Square, square);

					if (Client.instance.lobbyInfo.GameMode == GameMode.PVP)
					{
						if (intermediateSquare != null)
						{
							Client.instance.connectedClient.SendTurn(checker, square, intermediateSquare);
						}
						else
						{
							Client.instance.connectedClient.SendTurn(checker, square);
						}
					}
					else
					{
						TryToMoveChekerAsync(checker, square, intermediateSquare);
					}

				}
			}
		}

		public void BeatCheckerAsync(Square square)
		{
			if (square.Checker != null)
			{
				int removeId = _squaresData[square.Id].Checker.Id;
				_checkersData[removeId].WasBeat();
				_checkersData.Remove(removeId);
				BoardController.Instance.OnRemoveChecker(_squaresData[square.Id].Checker);
				_boardSquares[square.Position.X, square.Position.Y].SetChecker(null);
				_squaresData[square.Id].SetChecker(null);
				IsBeatProcessActive = true;
				CheckGameEnd();
			}
		}

		public void TryToMoveChekerAsync(Checker checker, Square square, Square target = null)
		{
			if(target != null)
			{
				BeatCheckerAsync(target);
			}



			_boardSquares[checker.Square.Position.X, checker.Square.Position.Y].SetChecker(null);
			_squaresData[checker.Square.Id].SetChecker(null);
			checker.MoveTo(square);
			square.SetChecker(checker);
			BoardController.Instance.OnMoveChecker(checker);

			
			var checkerColor = Client.instance.UserCheckerColor;
			if (checkerColor == checker.Color)
				CheckForSuperChecker(checker.Id);
			else
				CheckForSuperCheckerBot(checker.Id);

			// Move checker to new square.
			var canBeatYet = FindBeatCheckersForSpecificChecker(checker.Id);
			if (canBeatYet)
			{
				FindAvailableSquares(checker.Id);
			}

			FindAvailableSquaresForColor();

			if (!IsBeatProcessActive || !canBeatYet)
			{
				_currentChecker = null;
				// Swich move to another player.

				ClearAvailableSquares();
				ChangeMoveColor(checker.Color);
			}
		}
 

		public bool IsSafeForMovingSquare(int squareId)
		{
			bool isSafe = false;
			Square square = _squaresData[squareId];

			if (_allAvailableSquares.Contains(square))
			{
				Square intermediateSquare = HasIntermediateSquare(_currentChecker.Square, square);
				if (intermediateSquare != null)
				{
					isSafe = true;
				}
			}

			return isSafe;
		}

		/// <summary>
		/// Is Checkers on board now.
		/// </summary>
		private bool IsOnBoard(Position position)
		{
			return position.X >= 0 && position.X < _boardSize && position.Y >= 0 && position.Y < _boardSize;
		}

		/// <summary>
		/// Change color of current player that move. So if White checkers has moved on the next movement for black checkers
		/// </summary>
		public void ChangeMoveColor(CheckerColor checkerColor)
		{
			CurrentMoveColor = checkerColor == CheckerColor.White ? CheckerColor.Black : CheckerColor.White;
		}

		/// <summary>
		/// Clear all available squares.
		/// </summary>
		public void ClearAvailableSquares()
		{
			BoardController.Instance.UnmarkSquares(_allAvailableSquares);
			_allAvailableSquares.Clear();
		}

		/// <summary>
		/// Find checkers for beat.
		/// </summary>
		private List<Checker> FindBeatCheckers()
		{
			List<Checker> beatCheckers = new List<Checker>();
			foreach (var checker in _checkersData.Values)
			{
				if (_currentChecker == null || checker == _currentChecker)
				{
					if (checker.Color == CurrentMoveColor)
					{

						if (checker.IsSuperChecker)
						{
							var listBeatSuper = FindBeatCheckersForSuper(checker);

							if(listBeatSuper.Count > 0)
							{
								beatCheckers.AddRange(listBeatSuper);
							}
						}
						else
						{
							foreach (var neighbourSquarePosition in checker.Square.Neighbours(true, _currentMoveCollor, Client.instance.lobbyInfo.GameMode))
							{
								if (IsOnBoard(neighbourSquarePosition))
								{
									Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
									if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
									{
										Square secondNextSquare = GetSecondNextSquare(checker.Square, neighbourSquare);
										if (secondNextSquare != null && !secondNextSquare.HasChecker())
											beatCheckers.Add(checker);
									}
								}
							}
						}
					}
				}
			}
			return beatCheckers;
		}

		/// <summary>
		/// If checker can beat others checkers.
		/// </summary>
		private bool FindBeatCheckersForSpecificChecker(int checkerId)
		{
			ClearAvailableSquares();

			Checker checker = _checkersData[checkerId];

			if (checker.Color == CurrentMoveColor)
			{

				if (checker.IsSuperChecker)
				{
					return FindAvailableSquareForSuperMultiBeat(checker);
				}
				else
				{
					foreach (var neighbourSquarePosition in checker.Square.Neighbours(true, _currentMoveCollor, Client.instance.lobbyInfo.GameMode))
					{
						if (IsOnBoard(neighbourSquarePosition))
						{
							Square neighbourSquare = _boardSquares[neighbourSquarePosition.X, neighbourSquarePosition.Y];
							if (neighbourSquare.HasChecker() && neighbourSquare.Checker.Color != CurrentMoveColor)
							{
								Square secondNextSquare = GetSecondNextSquare(checker.Square, neighbourSquare);
								if (secondNextSquare != null && !secondNextSquare.HasChecker())
									return true;
							}
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Get next square for move through the square.
		/// </summary>
		private Square GetSecondNextSquare(Square currSquare, Square nextSquare)
		{
			Position secondNextPosition = new Position(currSquare.Position - 2 * (currSquare.Position - nextSquare.Position));
			return IsOnBoard(secondNextPosition) ? _boardSquares[secondNextPosition.X, secondNextPosition.Y] : null;
		}

		/// <summary>
		/// Is has square between currSquare and nextSquare.
		/// </summary>
		private Square HasIntermediateSquare(Square currSquare, Square nextSquare)
		{
			if (currSquare.Checker.IsSuperChecker)
			{
				var legnth = nextSquare.Position - currSquare.Position;


				for (int i = 1; i < Math.Abs(legnth.X); i++)
				{
					Position secondNextPosition = new Position(nextSquare, currSquare, i, true);

					if (IsOnBoard(secondNextPosition))
					{
						var square = _boardSquares[secondNextPosition.X, secondNextPosition.Y];

						if (square.HasChecker())
						{
							return square;
						}
					}
					else
					{
						break;
					}

				}


				return _boardSquares[nextSquare.Position.X, nextSquare.Position.Y];
			}
			else
			{
				Position pos = nextSquare.Position - currSquare.Position;
				if (pos.Abs() != new Position(1, 1))
				{
					Position intermediatePosition = (currSquare.Position + nextSquare.Position) / 2;
					return _boardSquares[intermediatePosition.X, intermediatePosition.Y];
				}
			}


			return null;
		}

		/// <summary>
		/// Check for changing to super checker.
		/// </summary>
		public void CheckForSuperChecker(int id)
		{
			Checker checkerForCheck = _checkersData[id];
			if (checkerForCheck != null && !checkerForCheck.IsSuperChecker)
			{
				if (checkerForCheck.Square.Position.Y == 0)
				{
					if(Client.instance.lobbyInfo.GameMode == GameMode.PVP)
						Client.instance.connectedClient.SendSuperChecker(id);
					else
						BecomeSuperCheckAsync(id);
				}
			}
		}

		public void CheckForSuperCheckerBot(int id)
		{
			Checker checkerForCheck = _checkersData[id];
			if (checkerForCheck != null && !checkerForCheck.IsSuperChecker && Client.instance.lobbyInfo.GameMode == GameMode.Mining)
			{
				if (checkerForCheck.Square.Position.Y == 7 && _currentMoveCollor == CheckerColor.Black)
				{
					BecomeSuperCheckAsync(id);
				}
			}
		}

		public void BecomeSuperCheckAsync(int id)
		{
			Checker checkerForCheck = _checkersData[id];
			if (checkerForCheck != null && !checkerForCheck.IsSuperChecker)
			{
				AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.SuperChecker);
				checkerForCheck.BecomeSuperChecker();
				BoardController.Instance.SetSuperChecker(checkerForCheck);
			}
		}

		/// <summary>
		/// Check for game ending.
		/// </summary>
		private void CheckGameEnd()
		{
			List<Checker> whiteCheckers = new List<Checker>();
			List<Checker> blackCheckers = new List<Checker>();

			foreach (var checker in _checkersData.Values)
			{
				if (checker.Color == CheckerColor.White && !checker.IsBeat)
					whiteCheckers.Add(checker);
				else if (checker.Color == CheckerColor.Black && !checker.IsBeat)
					blackCheckers.Add(checker);
			}

			UiViewController.Instance.UpdateGameScore(whiteCheckers.Count, blackCheckers.Count);
			if (whiteCheckers.Count == 0)
			{
				if(Client.instance.UserCheckerColor != CheckerColor.White && Client.instance.lobbyInfo.GameMode == GameMode.PVP)
				{
					Client.instance.connectedClient.SendVictory(Client.instance.connectedClient.currentUser.Token);
					return;
				}

				if(Client.instance.lobbyInfo.GameMode == GameMode.Mining)
				{
					Client.instance.connectedClient.SendVictory("Компьютер");
					return;
				}
				
			}
			else if (blackCheckers.Count == 0)
			{
				if (Client.instance.UserCheckerColor != CheckerColor.Black)
				{
					Client.instance.connectedClient.SendVictory(Client.instance.connectedClient.currentUser.Token);
					return;
				}
			}
		}

		/// <summary>
		/// Get square instance by id.
		/// </summary>
		public Square GetSquare(int squareID)
		{
			return (_squaresData.ContainsKey(squareID)) ? _squaresData[squareID] : null;
		}

		/// <summary>
		/// Bot move action in coroutine.
		/// </summary>
		public IEnumerator BotMove()
		{
			if (GameEnd) yield break;
			IsEnemyMove = true;
			yield return new WaitForSeconds(1f);

			AudioController.Instance.PlayOneShotAudio(AudioController.AudioType.Move);
			if (CurrentMoveColor == CheckerColor.Black)
			{
				Dictionary<Checker, List<Square>> availableCheckers = new Dictionary<Checker, List<Square>>();
				//find all available AI checkers.
				foreach (var checker in _checkersData.Values)
				{
					if (checker.Color == CheckerColor.Black)
					{
						FindAvailableSquares(checker.Id);

						if (_allAvailableSquares.Count != 0)
							availableCheckers.Add(checker, new List<Square>(_allAvailableSquares));
					}
				}

				if (availableCheckers.Count != 0)
				{
					Random r = new Random();
					int randCheckerId = r.Next(0, availableCheckers.Count);
					Checker randChecker = availableCheckers.Keys.ElementAt(randCheckerId);
					int randSquare = r.Next(0, availableCheckers[randChecker].Count);
					int sqaureId = availableCheckers[randChecker][randSquare].Id;
					_currentChecker = randChecker;
					FindAvailableSquares(randChecker.Id);
					TryToMoveChecker(sqaureId, randChecker.Id);
				}
			}
			
			IsBeatProcessActive = false;
			IsEnemyMove = false;
			yield break;
		}

		public IEnumerator SecondPlayerMove()
		{
			if (GameEnd) yield break;
			IsEnemyMove = true;
			var moveColor = CurrentMoveColor;
			IsBeatProcessActive = false;

			yield return new WaitUntil(() => moveColor != CurrentMoveColor);

			IsBeatProcessActive = false;
			IsEnemyMove = false;
			yield break;
		}
	}
}
