﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ConvexHull.Ouellet
{
	// ******************************************************************
	public class ConvexHull
	{
		// Quadrant: Q2 | Q1
		//	         -------
		//           Q3 | Q4

		private Quadrant _q1;
		private Quadrant _q2;
		private Quadrant _q3;
		private Quadrant _q4;

		private IReadOnlyList<IReadOnlyList<Point>> _listOfListOfPoint;
		private bool _shouldCloseTheGraph;

		// ******************************************************************
		public ConvexHull(IReadOnlyList<IReadOnlyList<Point>> listOfListOfPoint, bool shouldCloseTheGraph = true, int initialResultGuessSize = 0)
		{
			Init(listOfListOfPoint, shouldCloseTheGraph, initialResultGuessSize);
		}

		// ******************************************************************
		public ConvexHull(IReadOnlyList<Point> listOfPoint, bool shouldCloseTheGraph = true, int initialResultGuessSize = 0)
		{
			IReadOnlyList<Point>[] listOfListOfPoint = new IReadOnlyList<Point>[1];
			listOfListOfPoint[0] = listOfPoint;

			Init(listOfListOfPoint, shouldCloseTheGraph, initialResultGuessSize);
		}

		// ******************************************************************
		private void Init(IReadOnlyList<IReadOnlyList<Point>> listOfListOfPoint, bool shouldCloseTheGraph, int initialResultGuessSize)
		{
			_listOfListOfPoint = listOfListOfPoint;
			_shouldCloseTheGraph = shouldCloseTheGraph;

			if (initialResultGuessSize <= 0 && !IsZeroData())
			{
				int totalPointCount = 0;
				foreach (var enumOfPoint in listOfListOfPoint)
				{
					totalPointCount += enumOfPoint.Count;
				}
				initialResultGuessSize = Math.Min((int)Math.Sqrt(totalPointCount), 100);
			}

			_q1 = new QuadrantSpecific1(listOfListOfPoint, initialResultGuessSize);
			_q2 = new QuadrantSpecific2(listOfListOfPoint, initialResultGuessSize);
			_q3 = new QuadrantSpecific3(listOfListOfPoint, initialResultGuessSize);
			_q4 = new QuadrantSpecific4(listOfListOfPoint, initialResultGuessSize);
		}

		// ******************************************************************
		private int _count = -1;
		public int Count
		{
			get
			{
				if (_count == -1)
				{
					foreach (IReadOnlyList<Point> listOfPoint in this._listOfListOfPoint)
					{
						_count += listOfPoint.Count;
					}
				}

				return _count;
			}
		}

		// ******************************************************************
		/// <summary>
		/// 
		/// </summary>
		/// <param name="threadUsage">Using ConvexHullThreadUsage.All will only use all thread for the first pass (se quadrant limits) then use only 4 threads for pass 2 (which is the actual limit).</param>
		public void CalcConvexHull(ConvexHullThreadUsage threadUsage = ConvexHullThreadUsage.OneOrFour)
		{
			if (IsZeroData())
			{
				return;
			}

			if (threadUsage == ConvexHullThreadUsage.AutoSelect || threadUsage == ConvexHullThreadUsage.OneOrFour)
			{
				if (Environment.ProcessorCount == 1)
				{
					threadUsage = ConvexHullThreadUsage.OnlyOne;
				}
				// It's around 10 000 000 points on a 12 cores that some advantages really start to appear
				else if (threadUsage == ConvexHullThreadUsage.OneOrFour || Environment.ProcessorCount <= 4 || this.Count < 10000000)
				{
					threadUsage = ConvexHullThreadUsage.FixedFour;
				}
				else
				{
					threadUsage = ConvexHullThreadUsage.All;
				}
			}

			// There is no need to start more than 1 thread. It will not be usefull on a single core machine.
			if (threadUsage == ConvexHullThreadUsage.OnlyOne)
			{
				_q1.Calc();
				_q2.Calc();
				_q3.Calc();
				_q4.Calc();
			}
			else
			{
				bool isSkipSetQuadrantLimit = false;
				if (threadUsage == ConvexHullThreadUsage.All)
				{
					isSkipSetQuadrantLimit = true;

					SetQuadrantLimitsUsingAllThreads();
				}

				Quadrant[] quadrants = new Quadrant[4];
				quadrants[0] = _q1;
				quadrants[1] = _q2;
				quadrants[2] = _q3;
				quadrants[3] = _q4;

				Task[] tasks = new Task[4];
				for (int n = 0; n < tasks.Length; n++)
				{
					int nLocal = n; // Prevent Lambda internal closure error.
					tasks[n] = Task.Factory.StartNew(() => quadrants[nLocal].Calc(isSkipSetQuadrantLimit));
				}
				Task.WaitAll(tasks);
			}
		}

		private Limit _limit = null;
		// ******************************************************************
		// For usage of Parallel func, I highly suggest: Stephen Toub: Patterns of parallel programming ==> Just Awsome !!!
		// But its only my own fault if I'm not using it at its full potential...
		private void SetQuadrantLimitsUsingAllThreads()
		{
			Point pt = this._listOfListOfPoint.First().First();
			_limit = new Limit(pt);

			int coreCount = Environment.ProcessorCount;

			Task[] tasks = new Task[coreCount];
			for (int n = 0; n < tasks.Length; n++)
			{
				int nLocal = n; // Prevent Lambda internal closure error.
				tasks[n] = Task.Factory.StartNew(() =>
				{
					Limit limit = _limit.Copy();
					FindLimits(_listOfListOfPoint, nLocal, coreCount, limit);
					AggregateLimits(limit);
				});
			}
			Task.WaitAll(tasks);
			
			_q1.FirstPoint = _limit.Q1Right;
			_q1.LastPoint = _limit.Q1Top;
			_q2.FirstPoint = _limit.Q2Top;
			_q2.LastPoint = _limit.Q2Left;
			_q3.FirstPoint = _limit.Q3Left;
			_q3.LastPoint = _limit.Q3Bottom;
			_q4.FirstPoint = _limit.Q4Bottom;
			_q4.LastPoint = _limit.Q4Right;

			_q1.RootPoint = new Point(_q1.LastPoint.X, _q1.FirstPoint.Y);
			_q2.RootPoint = new Point(_q2.FirstPoint.X, _q2.LastPoint.Y);
			_q3.RootPoint = new Point(_q3.LastPoint.X, _q3.FirstPoint.Y);
			_q4.RootPoint = new Point(_q4.FirstPoint.X, _q4.LastPoint.Y);
		}

		// ******************************************************************
		private Limit FindLimits(IReadOnlyList<IReadOnlyList<Point>> listOfListOfPoint, int start, int offset, Limit limit)
		{
			foreach (var listOfPoint in listOfListOfPoint)
			{
				for (int index = start; index < listOfPoint.Count; index += offset)
				{
					Point pt = listOfPoint[index];

					double x = pt.X;
					double y = pt.Y;

					// Top
					if (y >= limit.Q2Top.Y)
					{
						if (y == limit.Q2Top.Y) // Special
						{
							if (y == limit.Q1Top.Y)
							{
								if (x < limit.Q2Top.X)
								{
									limit.Q2Top.X = x;
								}
								else if (x > limit.Q1Top.X)
								{
									limit.Q1Top.X = x;
								}
							}
							else
							{
								if (x < limit.Q2Top.X)
								{
									limit.Q1Top.X = limit.Q2Top.X;
									limit.Q1Top.Y = limit.Q2Top.Y;

									limit.Q2Top.X = x;
								}
								else if (x > limit.Q1Top.X)
								{
									limit.Q1Top.X = x;
									limit.Q1Top.Y = y;
								}
							}
						}
						else
						{
							limit.Q2Top.X = x;
							limit.Q2Top.Y = y;
						}
					}

					// Bottom
					if (y <= limit.Q3Bottom.Y)
					{
						if (y == limit.Q3Bottom.Y) // Special
						{
							if (y == limit.Q4Bottom.Y)
							{
								if (x < limit.Q3Bottom.X)
								{
									limit.Q3Bottom.X = x;
								}
								else if (x > limit.Q4Bottom.X)
								{
									limit.Q4Bottom.X = x;
								}
							}
							else
							{
								if (x < limit.Q3Bottom.X)
								{
									limit.Q4Bottom.X = limit.Q3Bottom.X;
									limit.Q4Bottom.Y = limit.Q3Bottom.Y;

									limit.Q3Bottom.X = x;
								}
								else if (x > limit.Q3Bottom.X)
								{
									limit.Q4Bottom.X = x;
									limit.Q4Bottom.Y = y;
								}
							}
						}
						else
						{
							limit.Q3Bottom.X = x;
							limit.Q3Bottom.Y = y;
						}
					}

					// Right
					if (x >= limit.Q4Right.X)
					{
						if (x == limit.Q4Right.X) // Special
						{
							if (x == limit.Q1Right.X)
							{
								if (y < limit.Q4Right.Y)
								{
									limit.Q4Right.Y = y;
								}
								else if (y > limit.Q1Right.Y)
								{
									limit.Q1Right.Y = y;
								}
							}
							else
							{
								if (y < limit.Q4Right.Y)
								{
									limit.Q1Right.X = limit.Q4Right.X;
									limit.Q1Right.Y = limit.Q4Right.Y;

									limit.Q4Right.Y = y;
								}
								else if (y > limit.Q1Right.Y)
								{
									limit.Q1Right.X = x;
									limit.Q1Right.Y = y;
								}
							}
						}
						else
						{
							limit.Q4Right.X = x;
							limit.Q4Right.Y = y;
						}
					}

					// Left
					if (x <= limit.Q3Left.X)
					{
						if (x == limit.Q3Left.X) // Special
						{
							if (x == limit.Q2Left.X)
							{
								if (y < limit.Q3Left.Y)
								{
									limit.Q3Left.Y = y;
								}
								else if (y > limit.Q2Left.Y)
								{
									limit.Q2Left.Y = y;
								}
							}
							else
							{
								if (y < limit.Q3Left.Y)
								{
									limit.Q2Left.X = limit.Q3Left.X;
									limit.Q2Left.Y = limit.Q3Left.Y;

									limit.Q3Left.Y = y;
								}
								else if (y > limit.Q2Left.Y)
								{
									limit.Q2Left.X = x;
									limit.Q2Left.Y = y;
								}
							}
						}
						else
						{
							limit.Q3Left.X = x;
							limit.Q3Left.Y = y;
						}
					}

					if (limit.Q2Left.X != limit.Q3Left.X)
					{
						limit.Q2Left.X = limit.Q3Left.X;
						limit.Q2Left.Y = limit.Q3Left.Y;
					}

					if (limit.Q1Right.X != limit.Q4Right.X)
					{
						limit.Q1Right.X = limit.Q4Right.X;
						limit.Q1Right.Y = limit.Q4Right.Y;
					}

					if (limit.Q1Top.Y != limit.Q2Top.Y)
					{
						limit.Q1Top.X = limit.Q2Top.X;
						limit.Q1Top.Y = limit.Q2Top.Y;
					}

					if (limit.Q4Bottom.Y != limit.Q3Bottom.Y)
					{
						limit.Q4Bottom.X = limit.Q3Bottom.X;
						limit.Q4Bottom.Y = limit.Q3Bottom.Y;
					}

				}
			}

			return limit;
		}

		// ******************************************************************
		private Limit FindLimits(Point pt, ParallelLoopState state, Limit limit)
		{
			double x = pt.X;
			double y = pt.Y;

			// Top
			if (y >= limit.Q2Top.Y)
			{
				if (y == limit.Q2Top.Y) // Special
				{
					if (y == limit.Q1Top.Y)
					{
						if (x < limit.Q2Top.X)
						{
							limit.Q2Top.X = x;
						}
						else if (x > limit.Q1Top.X)
						{
							limit.Q1Top.X = x;
						}
					}
					else
					{
						if (x < limit.Q2Top.X)
						{
							limit.Q1Top.X = limit.Q2Top.X;
							limit.Q1Top.Y = limit.Q2Top.Y;

							limit.Q2Top.X = x;
						}
						else if (x > limit.Q1Top.X)
						{
							limit.Q1Top.X = x;
							limit.Q1Top.Y = y;
						}
					}
				}
				else
				{
					limit.Q2Top.X = x;
					limit.Q2Top.Y = y;
				}
			}

			// Bottom
			if (y <= limit.Q3Bottom.Y)
			{
				if (y == limit.Q3Bottom.Y) // Special
				{
					if (y == limit.Q4Bottom.Y)
					{
						if (x < limit.Q3Bottom.X)
						{
							limit.Q3Bottom.X = x;
						}
						else if (x > limit.Q4Bottom.X)
						{
							limit.Q4Bottom.X = x;
						}
					}
					else
					{
						if (x < limit.Q3Bottom.X)
						{
							limit.Q4Bottom.X = limit.Q3Bottom.X;
							limit.Q4Bottom.Y = limit.Q3Bottom.Y;

							limit.Q3Bottom.X = x;
						}
						else if (x > limit.Q3Bottom.X)
						{
							limit.Q4Bottom.X = x;
							limit.Q4Bottom.Y = y;
						}
					}
				}
				else
				{
					limit.Q3Bottom.X = x;
					limit.Q3Bottom.Y = y;
				}
			}

			// Right
			if (x >= limit.Q4Right.X)
			{
				if (x == limit.Q4Right.X) // Special
				{
					if (x == limit.Q1Right.X)
					{
						if (y < limit.Q4Right.Y)
						{
							limit.Q4Right.Y = y;
						}
						else if (y > limit.Q1Right.Y)
						{
							limit.Q1Right.Y = y;
						}
					}
					else
					{
						if (y < limit.Q4Right.Y)
						{
							limit.Q1Right.X = limit.Q4Right.X;
							limit.Q1Right.Y = limit.Q4Right.Y;

							limit.Q4Right.Y = y;
						}
						else if (y > limit.Q1Right.Y)
						{
							limit.Q1Right.X = x;
							limit.Q1Right.Y = y;
						}
					}
				}
				else
				{
					limit.Q4Right.X = x;
					limit.Q4Right.Y = y;
				}
			}

			// Left
			if (x <= limit.Q3Left.X)
			{
				if (x == limit.Q3Left.X) // Special
				{
					if (x == limit.Q2Left.X)
					{
						if (y < limit.Q3Left.Y)
						{
							limit.Q3Left.Y = y;
						}
						else if (y > limit.Q2Left.Y)
						{
							limit.Q2Left.Y = y;
						}
					}
					else
					{
						if (y < limit.Q3Left.Y)
						{
							limit.Q2Left.X = limit.Q3Left.X;
							limit.Q2Left.Y = limit.Q3Left.Y;

							limit.Q3Left.Y = y;
						}
						else if (y > limit.Q2Left.Y)
						{
							limit.Q2Left.X = x;
							limit.Q2Left.Y = y;
						}
					}
				}
				else
				{
					limit.Q3Left.X = x;
					limit.Q3Left.Y = y;
				}
			}

			if (limit.Q2Left.X != limit.Q3Left.X)
			{
				limit.Q2Left.X = limit.Q3Left.X;
				limit.Q2Left.Y = limit.Q3Left.Y;
			}

			if (limit.Q1Right.X != limit.Q4Right.X)
			{
				limit.Q1Right.X = limit.Q4Right.X;
				limit.Q1Right.Y = limit.Q4Right.Y;
			}

			if (limit.Q1Top.Y != limit.Q2Top.Y)
			{
				limit.Q1Top.X = limit.Q2Top.X;
				limit.Q1Top.Y = limit.Q2Top.Y;
			}

			if (limit.Q4Bottom.Y != limit.Q3Bottom.Y)
			{
				limit.Q4Bottom.X = limit.Q3Bottom.X;
				limit.Q4Bottom.Y = limit.Q3Bottom.Y;
			}

			return limit;
		}

		// ******************************************************************
		private object _findLimitFinalLock = new object();

		private void AggregateLimits(Limit limit)
		{
			lock (_findLimitFinalLock)
			{
				if (limit.Q1Right.X >= _limit.Q1Right.X)
				{
					if (limit.Q1Right.X == _limit.Q1Right.X)
					{
						if (limit.Q1Right.Y > _limit.Q1Right.Y)
						{
							_limit.Q1Right = limit.Q1Right;
						}
					}
					else
					{
						_limit.Q1Right = limit.Q1Right;
					}
				}

				if (limit.Q4Right.X > _limit.Q4Right.X)
				{
					if (limit.Q4Right.X == _limit.Q4Right.X)
					{
						if (limit.Q4Right.Y < _limit.Q4Right.Y)
						{
							_limit.Q4Right = limit.Q4Right;
						}
					}
					else
					{
						_limit.Q4Right = limit.Q4Right;
					}
				}

				if (limit.Q2Left.X < _limit.Q2Left.X)
				{
					if (limit.Q2Left.X == _limit.Q2Left.X)
					{
						if (limit.Q2Left.Y > _limit.Q2Left.Y)
						{
							_limit.Q2Left = limit.Q2Left;
						}
					}
					else
					{
						_limit.Q2Left = limit.Q2Left;
					}
				}

				if (limit.Q3Left.X < _limit.Q3Left.X)
				{
					if (limit.Q3Left.X == _limit.Q3Left.X)
					{
						if (limit.Q3Left.Y > _limit.Q3Left.Y)
						{
							_limit.Q3Left = limit.Q3Left;
						}
					}
					else
					{
						_limit.Q3Left = limit.Q3Left;
					}
				}

				if (limit.Q1Top.Y > _limit.Q1Top.Y)
				{
					if (limit.Q1Top.Y == _limit.Q1Top.Y)
					{
						if (limit.Q1Top.X > _limit.Q1Top.X)
						{
							_limit.Q1Top = limit.Q1Top;
						}
					}
					else
					{
						_limit.Q1Top = limit.Q1Top;
					}
				}

				if (limit.Q2Top.Y > _limit.Q2Top.Y)
				{
					if (limit.Q2Top.Y == _limit.Q2Top.Y)
					{
						if (limit.Q2Top.X < _limit.Q2Top.X)
						{
							_limit.Q2Top = limit.Q2Top;
						}
					}
					else
					{
						_limit.Q2Top = limit.Q2Top;
					}
				}

				if (limit.Q3Bottom.Y < _limit.Q3Bottom.Y)
				{
					if (limit.Q3Bottom.Y == _limit.Q3Bottom.Y)
					{
						if (limit.Q3Bottom.X < _limit.Q3Bottom.X)
						{
							_limit.Q3Bottom = limit.Q3Bottom;
						}
					}
					else
					{
						_limit.Q3Bottom = limit.Q3Bottom;
					}
				}

				if (limit.Q4Bottom.Y < _limit.Q4Bottom.Y)
				{
					if (limit.Q4Bottom.Y == _limit.Q4Bottom.Y)
					{
						if (limit.Q4Bottom.X > _limit.Q4Bottom.X)
						{
							_limit.Q4Bottom = limit.Q4Bottom;
						}
					}
					else
					{
						_limit.Q4Bottom = limit.Q4Bottom;
					}
				}
			}
		}

		// ******************************************************************
		public Point[] GetResultsAsArrayOfPoint()
		{
			if (_listOfListOfPoint == null || !_listOfListOfPoint.Any() || !_listOfListOfPoint.First().Any())
			{
				return new Point[0];
			}

			int indexQ1Start;
			int indexQ2Start;
			int indexQ3Start;
			int indexQ4Start;
			int indexQ1End;
			int indexQ2End;
			int indexQ3End;
			int indexQ4End;

			Point lastPoint = new Point(double.NaN, double.NaN);

			indexQ1Start = 0;
			indexQ1End = _q1.HullPoints.Count - 1;
			lastPoint = _q1.HullPoints[indexQ1End];

			if (_q2.HullPoints.Count == 1)
			{
				if (_q2.FirstPoint.Equals(lastPoint))
				{
					indexQ2Start = 1;
					indexQ2End = 0;
				}
				else
				{
					indexQ2Start = 0;
					indexQ2End = 0;
					lastPoint = _q2.HullPoints[0];
				}
			}
			else
			{
				if (_q2.FirstPoint.Equals(lastPoint))
				{
					indexQ2Start = 1;
				}
				else
				{
					indexQ2Start = 0;
				}
				indexQ2End = _q2.HullPoints.Count - 1;
				lastPoint = _q2.HullPoints[indexQ2End];
			}

			if (_q3.HullPoints.Count == 1)
			{
				if (_q3.FirstPoint.Equals(lastPoint))
				{
					indexQ3Start = 1;
					indexQ3End = 0;
				}
				else
				{
					indexQ3Start = 0;
					indexQ3End = 0;
					lastPoint = _q3.HullPoints[0];
				}
			}
			else
			{
				if (_q3.FirstPoint.Equals(lastPoint))
				{
					indexQ3Start = 1;
				}
				else
				{
					indexQ3Start = 0;
				}
				indexQ3End = _q3.HullPoints.Count - 1;
				lastPoint = _q3.HullPoints[indexQ3End];
			}

			if (_q4.HullPoints.Count == 1)
			{
				if (_q4.FirstPoint.Equals(lastPoint))
				{
					indexQ4Start = 1;
					indexQ4End = 0;
				}
				else
				{
					indexQ4Start = 0;
					indexQ4End = 0;
					lastPoint = _q4.HullPoints[0];
				}
			}
			else
			{
				if (_q4.FirstPoint.Equals(lastPoint))
				{
					indexQ4Start = 1;
				}
				else
				{
					indexQ4Start = 0;
				}

				indexQ4End = _q4.HullPoints.Count - 1;
				lastPoint = _q4.HullPoints[indexQ4End];
			}

			if (_q1.HullPoints[indexQ1Start] == lastPoint)
			{
				indexQ1Start++;
			}

			int countOfFinalHullPoint = (indexQ1End - indexQ1Start) +
										(indexQ2End - indexQ2Start) +
										(indexQ3End - indexQ3Start) +
										(indexQ4End - indexQ4Start) + 4;

			if (countOfFinalHullPoint <= 1) // Case where there is only one point or many of only the same point. Auto closed if required.
			{
				return new Point[] { new Point(_q1.HullPoints[0].X, _q1.HullPoints[0].Y) };
			}

			if (_shouldCloseTheGraph)
			{
				countOfFinalHullPoint++;
			}

			Point[] results = new Point[countOfFinalHullPoint];

			int resIndex = 0;

			for (int n = indexQ1Start; n <= indexQ1End; n++)
			{
				results[resIndex] = new Point(_q1.HullPoints[n].X, _q1.HullPoints[n].Y);
				resIndex++;
			}

			for (int n = indexQ2Start; n <= indexQ2End; n++)
			{
				results[resIndex] = new Point(_q2.HullPoints[n].X, _q2.HullPoints[n].Y);
				resIndex++;
			}

			for (int n = indexQ3Start; n <= indexQ3End; n++)
			{
				results[resIndex] = new Point(_q3.HullPoints[n].X, _q3.HullPoints[n].Y);
				resIndex++;
			}

			for (int n = indexQ4Start; n <= indexQ4End; n++)
			{
				results[resIndex] = new Point(_q4.HullPoints[n].X, _q4.HullPoints[n].Y);
				resIndex++;
			}

			if (_shouldCloseTheGraph)
			{
				results[resIndex] = _q1.HullPoints[indexQ1Start];
			}

			return results;
		}

		// ******************************************************************
		private bool IsZeroData()
		{
			return _listOfListOfPoint == null || !_listOfListOfPoint.Any() || !_listOfListOfPoint.First().Any();
		}

		// ******************************************************************

	}
}

