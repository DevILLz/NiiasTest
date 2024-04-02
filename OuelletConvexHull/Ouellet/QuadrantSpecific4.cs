using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ConvexHull.Ouellet
{
	public class QuadrantSpecific4 : Quadrant
	{
		// ************************************************************************
		public QuadrantSpecific4(IReadOnlyList<IReadOnlyList<Point>> listOfListOfPoint, int initialResultGuessSize) :
			base(listOfListOfPoint, initialResultGuessSize)
		{
		}

		// ******************************************************************
		protected override void SetQuadrantLimits()
		{
			Point firstPoint = this._listOfListOfPoint.First().First();

			double rightX = firstPoint.X;
			double rightY = firstPoint.Y;

			double bottomX = rightX;
			double bottomY = rightY;

			foreach (var enumOfPoints in _listOfListOfPoint)
			{
				foreach (var point in enumOfPoints)
				{
					if (point.X >= rightX)
					{
						if (point.X == rightX)
						{
							if (point.Y < rightY)
							{
								rightY = point.Y;
							}
						}
						else
						{
							rightX = point.X;
							rightY = point.Y;
						}
					}

					if (point.Y <= bottomY)
					{
						if (point.Y == bottomY)
						{
							if (point.X > bottomX)
							{
								bottomX = point.X;
							}
						}
						else
						{
							bottomX = point.X;
							bottomY = point.Y;
						}
					}
				}
			}

			FirstPoint = new Point(bottomX, bottomY);
			LastPoint = new Point(rightX, rightY);
			RootPoint = new Point(bottomX, rightY);
		}

		// ******************************************************************
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override bool IsGoodQuadrantForPoint(Point pt)
		{
			if (pt.X > this.RootPoint.X && pt.Y < this.RootPoint.Y)
			{
				return true;
			}

			return false;
		}

		// ******************************************************************
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool IsValueCannotBeConvexValueToAnotherOne(double x, double y, double xRef, double yRef)
		{
			if (x < xRef)
			{
				if (y >= yRef)
				{
					return true;
				}
			}
			else if (y > yRef)
			{
				if (x <= xRef)
				{
					return true;
				}
			}

			return false;
		}

		// ******************************************************************
		protected override int TryAdd(double x, double y)
		{
			int indexLow = 0;
			int indexMax = HullPoints.Count - 1;
			int indexHi = indexMax;

			while (indexLow != indexHi - 1)
			{
				int index = ((indexHi - indexLow) >> 1) + indexLow;

				if (x <= HullPoints[index].X && y >= HullPoints[index].Y)
				{
					return -1; // No calc needed
				}

				if (x > HullPoints[index].X)
				{
					indexLow = index;
					continue;
				}

				if (x < HullPoints[index].X)
				{
					indexHi = index;
					continue;
				}

				if (x == HullPoints[index].X)
				{
					if (y < HullPoints[index].Y)
					{
						indexLow = index;
					}
					else
					{
						return -1;
					}
				}

				break;
			}

			if (y >= HullPoints[indexHi].Y)
			{
				return -1; // Eliminated without slope calc
			}

			return indexLow;
		}

		// ******************************************************************
	}
}
