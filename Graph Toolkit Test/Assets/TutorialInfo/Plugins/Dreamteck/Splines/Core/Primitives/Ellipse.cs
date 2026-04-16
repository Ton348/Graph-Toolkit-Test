using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
	public class Ellipse : SplinePrimitive
	{
		public float xRadius = 1f;
		public float yRadius = 1f;

		public override Spline.Type GetSplineType()
		{
			return Spline.Type.Bezier;
		}

		protected override void Generate()
		{
			base.Generate();
			m_closed = true;
			CreatePoints(4, SplinePoint.Type.SmoothMirrored);
			m_points[0].position = Vector3.up * yRadius;
			m_points[0].SetTangentPosition(m_points[0].position +
			                               Vector3.right * 2 * (Mathf.Sqrt(2f) - 1f) / 1.5f * xRadius);
			m_points[1].position = Vector3.left * xRadius;
			m_points[1].SetTangentPosition(m_points[1].position +
			                               Vector3.up * 2 * (Mathf.Sqrt(2f) - 1f) / 1.5f * yRadius);
			m_points[2].position = Vector3.down * yRadius;
			m_points[2].SetTangentPosition(m_points[2].position +
			                               Vector3.left * 2 * (Mathf.Sqrt(2f) - 1f) / 1.5f * xRadius);
			m_points[3].position = Vector3.right * xRadius;
			m_points[3].SetTangentPosition(m_points[3].position +
			                               Vector3.down * 2 * (Mathf.Sqrt(2f) - 1f) / 1.5f * yRadius);
		}
	}
}