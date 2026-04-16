using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
	public class Capsule : SplinePrimitive
	{
		public float height = 2f;
		public float radius = 1f;

		public override Spline.Type GetSplineType()
		{
			return Spline.Type.Bezier;
		}

		protected override void Generate()
		{
			base.Generate();
			m_closed = true;
			CreatePoints(6, SplinePoint.Type.SmoothMirrored);
			m_points[0].position = Vector3.right / 2f * radius + Vector3.up * height * 0.5f;
			m_points[0].SetTangentPosition(
				m_points[0].position + Vector3.down * 2 * (Mathf.Sqrt(2f) - 1f) / 3f * radius);
			m_points[1].position = Vector3.up / 2f * radius + Vector3.up * height * 0.5f;
			m_points[1].SetTangentPosition(m_points[1].position +
			                               Vector3.right * 2 * (Mathf.Sqrt(2f) - 1f) / 3f * radius);
			m_points[2].position = Vector3.left / 2f * radius + Vector3.up * height * 0.5f;
			m_points[2].SetTangentPosition(m_points[2].position + Vector3.up * 2 * (Mathf.Sqrt(2f) - 1f) / 3f * radius);
			m_points[3].position = Vector3.left / 2f * radius + Vector3.down * height * 0.5f;
			m_points[3].SetTangentPosition(m_points[3].position + Vector3.up * 2 * (Mathf.Sqrt(2f) - 1f) / 3f * radius);
			m_points[4].position = Vector3.down / 2f * radius + Vector3.down * height * 0.5f;
			m_points[4].SetTangentPosition(
				m_points[4].position + Vector3.left * 2 * (Mathf.Sqrt(2f) - 1f) / 3f * radius);
			m_points[5].position = Vector3.right / 2f * radius + Vector3.down * height * 0.5f;
			m_points[5].SetTangentPosition(
				m_points[5].position + Vector3.down * 2 * (Mathf.Sqrt(2f) - 1f) / 3f * radius);
		}
	}
}