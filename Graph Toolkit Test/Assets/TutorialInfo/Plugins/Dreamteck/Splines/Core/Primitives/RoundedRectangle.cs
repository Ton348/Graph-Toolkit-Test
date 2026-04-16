using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
	public class RoundedRectangle : SplinePrimitive
	{
		public Vector2 size = Vector2.one;
		public float xRadius = 0.25f;
		public float yRadius = 0.25f;

		public override Spline.Type GetSplineType()
		{
			return Spline.Type.Bezier;
		}

		protected override void Generate()
		{
			base.Generate();
			m_closed = true;
			CreatePoints(8, SplinePoint.Type.Broken);
			Vector2 edgeSize = size - new Vector2(xRadius, yRadius) * 2f;
			m_points[0].SetPosition(Vector3.up / 2f * edgeSize.y + Vector3.left / 2f * size.x);
			m_points[1].SetPosition(Vector3.up / 2f * size.y + Vector3.left / 2f * edgeSize.x);
			m_points[2].SetPosition(Vector3.up / 2f * size.y + Vector3.right / 2f * edgeSize.x);
			m_points[3].SetPosition(Vector3.up / 2f * edgeSize.y + Vector3.right / 2f * size.x);
			m_points[4].SetPosition(Vector3.down / 2f * edgeSize.y + Vector3.right / 2f * size.x);
			m_points[5].SetPosition(Vector3.down / 2f * size.y + Vector3.right / 2f * edgeSize.x);
			m_points[6].SetPosition(Vector3.down / 2f * size.y + Vector3.left / 2f * edgeSize.x);
			m_points[7].SetPosition(Vector3.down / 2f * edgeSize.y + Vector3.left / 2f * size.x);

			float xRad = 2f * (Mathf.Sqrt(2f) - 1f) / 3f * xRadius * 2f;
			float yRad = 2f * (Mathf.Sqrt(2f) - 1f) / 3f * yRadius * 2f;
			m_points[0].SetTangent2Position(m_points[0].position + Vector3.up * yRad);
			m_points[1].SetTangentPosition(m_points[1].position + Vector3.left * xRad);
			m_points[2].SetTangent2Position(m_points[2].position + Vector3.right * xRad);
			m_points[3].SetTangentPosition(m_points[3].position + Vector3.up * yRad);
			m_points[4].SetTangent2Position(m_points[4].position + Vector3.down * yRad);
			m_points[5].SetTangentPosition(m_points[5].position + Vector3.right * xRad);
			m_points[6].SetTangent2Position(m_points[6].position + Vector3.left * xRad);
			m_points[7].SetTangentPosition(m_points[7].position + Vector3.down * yRad);
		}
	}
}