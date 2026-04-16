using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
	public static class SplineUtility
	{
		public enum MergeSide
		{
			Start,
			End
		}

		/// <summary>
		///     Merges two spline objects into one. The result will be merged into <paramref name="baseSpline" />
		/// </summary>
		/// <param name="baseSpline">The base spline object that</param>
		/// <param name="addedSpline">The object that will be merged into the base spline</param>
		/// <param name="side">Which side of the base spline to append to - beginning or end?</param>
		/// <param name="mergeEndpoints">Should the end points of the splines be merged or should they be bridged?</param>
		/// <param name="destroyAddedSpline">If true, the added spline's game object will be destroyed after merge</param>
		public static void Merge(
			SplineComputer baseSpline,
			SplineComputer addedSpline,
			MergeSide side,
			bool mergeEndpoints = false,
			bool destroyAddedSpline = false)
		{
			SplinePoint[] mergedPoints = addedSpline.GetPoints();
			SplinePoint[] basePoints = baseSpline.GetPoints();
			var pointsList = new List<SplinePoint>();
			SplinePoint[] points;
			if (!mergeEndpoints)
			{
				points = new SplinePoint[mergedPoints.Length + basePoints.Length];
			}
			else
			{
				points = new SplinePoint[mergedPoints.Length + basePoints.Length - 1];
			}

			if (side == MergeSide.End)
			{
				if (side == MergeSide.Start)
				{
					for (var i = 0; i < basePoints.Length; i++)
					{
						pointsList.Add(basePoints[i]);
					}

					for (int i = mergeEndpoints ? 1 : 0; i < mergedPoints.Length; i++)
					{
						pointsList.Add(mergedPoints[i]);
					}
				}
				else
				{
					for (var i = 0; i < basePoints.Length; i++)
					{
						pointsList.Add(basePoints[i]);
					}

					for (var i = 0; i < mergedPoints.Length - (mergeEndpoints ? 1 : 0); i++)
					{
						pointsList.Add(mergedPoints[mergedPoints.Length - 1 - i]);
					}
				}
			}
			else
			{
				if (side == MergeSide.Start)
				{
					for (var i = 0; i < mergedPoints.Length - (mergeEndpoints ? 1 : 0); i++)
					{
						pointsList.Add(mergedPoints[mergedPoints.Length - 1 - i]);
					}

					for (var i = 0; i < basePoints.Length; i++)
					{
						pointsList.Add(basePoints[i]);
					}
				}
				else
				{
					for (int i = mergeEndpoints ? 1 : 0; i < mergedPoints.Length; i++)
					{
						pointsList.Add(mergedPoints[i]);
					}

					for (var i = 0; i < basePoints.Length; i++)
					{
						pointsList.Add(basePoints[i]);
					}
				}
			}

			points = pointsList.ToArray();
			double mergedPercent = (double)(mergedPoints.Length - 1) / (points.Length - 1);
			var from = 0.0;
			var to = 1.0;
			if (side == MergeSide.End)
			{
				from = 1.0 - mergedPercent;
				to = 1.0;
			}
			else
			{
				from = 0.0;
				to = mergedPercent;
			}


			var mergedNodes = new List<Node>();
			var mergedIndices = new List<int>();

			for (var i = 0; i < addedSpline.pointCount; i++)
			{
				Node node = addedSpline.GetNode(i);
				if (node != null)
				{
					mergedNodes.Add(node);
					mergedIndices.Add(i);
					addedSpline.DisconnectNode(i);
					i--;
				}
			}

			SplineUser[] subs = addedSpline.GetSubscribers();
			for (var i = 0; i < subs.Length; i++)
			{
				addedSpline.Unsubscribe(subs[i]);
				subs[i].spline = baseSpline;
				subs[i].clipFrom = Dmath.Lerp(from, to, subs[i].clipFrom);
				subs[i].clipTo = Dmath.Lerp(from, to, subs[i].clipTo);
			}

			baseSpline.SetPoints(points);

			if (side == MergeSide.Start)
			{
				baseSpline.ShiftNodes(0, baseSpline.pointCount - 1, addedSpline.pointCount);
				for (var i = 0; i < mergedNodes.Count; i++)
				{
					baseSpline.ConnectNode(mergedNodes[i], mergedIndices[i]);
				}
			}
			else
			{
				for (var i = 0; i < mergedNodes.Count; i++)
				{
					int connectIndex = mergedIndices[i] + basePoints.Length;
					if (mergeEndpoints)
					{
						connectIndex--;
					}

					baseSpline.ConnectNode(mergedNodes[i], connectIndex);
				}
			}

			if (destroyAddedSpline)
			{
				Object.Destroy(addedSpline.gameObject);
			}
		}
	}
}