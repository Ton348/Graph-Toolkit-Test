using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
	public class Rectangle : SplinePrimitive
	{
		public Vector2 size = Vector2.one;

		public override Spline.Type GetSplineType()
		{
			return Spline.Type.Linear;
		}

		protected override void Generate()
		{
			base.Generate();
			m_closed = true;
			CreatePoints(4, SplinePoint.Type.SmoothMirrored);
			m_points[0].position = m_points[0].tangent = Vector3.up / 2f * size.y + Vector3.left / 2f * size.x;
			m_points[1].position = m_points[1].tangent = Vector3.up / 2f * size.y + Vector3.right / 2f * size.x;
			m_points[2].position = m_points[2].tangent = Vector3.down / 2f * size.y + Vector3.right / 2f * size.x;
			m_points[3].position = m_points[3].tangent = Vector3.down / 2f * size.y + Vector3.left / 2f * size.x;
		}
	}
}