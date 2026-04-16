using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[AddComponentMenu("Dreamteck/Splines/Users/Spline Mesh")]
	public partial class SplineMesh : MeshGenerator
	{
		//Mesh data
		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("channels")]
		private List<Channel> m_channels = new();

		private readonly List<TsMesh> m_combineMeshes = new();
		private SplineSample m_lastResult;
		private Matrix4x4 m_normalMatrix;
		private bool m_useLastResult;

		private Matrix4x4 m_vertexMatrix;

		protected override string meshName => "Custom Mesh";

		protected override void Awake()
		{
			base.Awake();
#if UNITY_EDITOR
			for (var i = 0; i < m_channels.Count; i++)
			{
				for (var j = 0; j < m_channels[i].GetMeshCount(); j++)
				{
					m_channels[i].GetMesh(j).Refresh();
				}
			}
#endif
		}

		protected override void Reset()
		{
			base.Reset();
			AddChannel("Channel 1");
		}

		public void RemoveChannel(int index)
		{
			m_channels.RemoveAt(index);
			Rebuild();
		}

		public void SwapChannels(int a, int b)
		{
			if (a < 0 || a >= m_channels.Count || b < 0 || b >= m_channels.Count)
			{
				return;
			}

			Channel temp = m_channels[b];
			m_channels[b] = m_channels[a];
			m_channels[a] = temp;
			Rebuild();
		}

		public Channel AddChannel(Mesh inputMesh, string name)
		{
			var channel = new Channel(name, inputMesh, this);
			m_channels.Add(channel);
			return channel;
		}

		public Channel AddChannel(string name)
		{
			var channel = new Channel(name, this);
			m_channels.Add(channel);
			return channel;
		}

		public int GetChannelCount()
		{
			return m_channels.Count;
		}

		public Channel GetChannel(int index)
		{
			return m_channels[index];
		}


		protected override void BuildMesh()
		{
			base.BuildMesh();
			Generate();
		}

		private void Generate()
		{
			var meshCount = 0;
			for (var i = 0; i < m_channels.Count; i++)
			{
				if (m_channels[i].GetMeshCount() == 0)
				{
					continue;
				}

				if (m_channels[i].autoCount)
				{
					var avgBounds = 0f;
					for (var j = 0; j < m_channels[i].GetMeshCount(); j++)
					{
						avgBounds += m_channels[i].GetMesh(j).bounds.size.z;
					}

					if (m_channels[i].GetMeshCount() > 1)
					{
						avgBounds /= m_channels[i].GetMeshCount();
					}

					if (avgBounds > 0f)
					{
						float length = CalculateLength(m_channels[i].clipFrom, m_channels[i].clipTo, false);
						int newCount = Mathf.RoundToInt(length / avgBounds);
						if (newCount < 1)
						{
							newCount = 1;
						}

						m_channels[i].count = newCount;
					}
				}

				meshCount += m_channels[i].count;
			}

			if (meshCount == 0)
			{
				tsMesh.Clear();
				return;
			}

			if (m_combineMeshes.Count < meshCount)
			{
				m_combineMeshes.AddRange(new TsMesh[meshCount - m_combineMeshes.Count]);
			}
			else if (m_combineMeshes.Count > meshCount)
			{
				m_combineMeshes.RemoveRange(m_combineMeshes.Count - 1 - (m_combineMeshes.Count - meshCount),
					m_combineMeshes.Count - meshCount);
			}

			var combineMeshIndex = 0;
			for (var i = 0; i < m_channels.Count; i++)
			{
				if (m_channels[i].GetMeshCount() == 0)
				{
					continue;
				}

				m_channels[i].ResetIteration();
				m_useLastResult = false;
				double step = 1.0 / m_channels[i].count;
				double space = step * m_channels[i].spacing * 0.5;

				switch (m_channels[i].type)
				{
					case Channel.Type.Extrude:
						for (var j = 0; j < m_channels[i].count; j++)
						{
							double from = Dmath.Lerp(m_channels[i].clipFrom, m_channels[i].clipTo, j * step + space);
							double to = Dmath.Lerp(m_channels[i].clipFrom, m_channels[i].clipTo,
								j * step + step - space);
							if (m_combineMeshes[combineMeshIndex] == null)
							{
								m_combineMeshes[combineMeshIndex] = new TsMesh();
							}

							Extrude(m_channels[i], m_combineMeshes[combineMeshIndex], from, to);
							combineMeshIndex++;
						}

						if (space == 0f)
						{
							m_useLastResult = true;
						}

						break;
					case Channel.Type.Place:
						for (var j = 0; j < m_channels[i].count; j++)
						{
							if (m_combineMeshes[combineMeshIndex] == null)
							{
								m_combineMeshes[combineMeshIndex] = new TsMesh();
							}

							Place(m_channels[i], m_combineMeshes[combineMeshIndex],
								Dmath.Lerp(m_channels[i].clipFrom, m_channels[i].clipTo,
									(double)j / Mathf.Max(m_channels[i].count - 1, 1)));
							combineMeshIndex++;
						}

						break;
				}
			}

			tsMesh.Combine(m_combineMeshes);
		}

		private void Place(Channel channel, TsMesh target, double percent)
		{
			Channel.MeshDefinition definition = channel.NextMesh();
			if (target == null)
			{
				target = new TsMesh();
			}

			definition.Write(target, channel.overrideMaterialID ? channel.targetMaterialID : -1);
			Vector2 channelOffset = channel.NextRandomOffset();
			Quaternion channelRotation = channel.NextRandomQuaternion();

			(Vector2, Quaternion, Vector3) customValues = channel.GetCustomPlaceValues(percent);

			Vector2 finalOffset = channelOffset + customValues.Item1 + new Vector2(offset.x, offset.y);
			Quaternion finalRotation =
				channelRotation * Quaternion.AngleAxis(rotation, Vector3.forward) * customValues.Item2;
			Vector3 finalScale = channel.NextPlaceScale();

			Evaluate(percent, ref m_evalResult);
			Vector3 originalNormal = m_evalResult.up;
			Vector3 originalRight = m_evalResult.right;
			Vector3 originalDirection = m_evalResult.forward;
			if (channel.overrideNormal)
			{
				m_evalResult.forward = Vector3.Cross(m_evalResult.right, channel.customNormal);
				m_evalResult.up = channel.customNormal;
			}

			if (!channel.scaleModifier.useClippedPercent)
			{
				UnclipPercent(ref m_evalResult.percent);
			}

			Vector3 scaleMod = channel.scaleModifier.GetScale(m_evalResult);
			finalScale.x *= customValues.Item3.x * scaleMod.x;
			finalScale.y *= customValues.Item3.y * scaleMod.y;
			finalScale.z *= customValues.Item3.z * scaleMod.z;

			if (!channel.scaleModifier.useClippedPercent)
			{
				ClipPercent(ref m_evalResult.percent);
			}

			float resultSize = GetBaseSize(m_evalResult);
			m_vertexMatrix.SetTRS(
				m_evalResult.position + originalRight * (finalOffset.x * resultSize) +
				originalNormal * (finalOffset.y * resultSize) + originalDirection * offset.z, //Position
				m_evalResult.rotation * finalRotation, //Rotation
				finalScale * resultSize); //Scale
			m_normalMatrix = m_vertexMatrix.inverse.transpose;

			for (var i = 0; i < target.vertexCount; i++)
			{
				target.vertices[i] = m_vertexMatrix.MultiplyPoint3x4(definition.vertices[i]);
				target.normals[i] = m_normalMatrix.MultiplyVector(definition.normals[i]);
			}

			for (var i = 0; i < Mathf.Min(target.colors.Length, definition.colors.Length); i++)
			{
				target.colors[i] = definition.colors[i] * m_evalResult.color * color;
			}
		}

		private void Extrude(Channel channel, TsMesh target, double from, double to)
		{
			Channel.MeshDefinition definition = channel.NextMesh();
			if (target == null)
			{
				target = new TsMesh();
			}

			definition.Write(target, channel.overrideMaterialID ? channel.targetMaterialID : -1);
			Vector2 uv = Vector2.zero;
			Vector3 trsVector = Vector3.zero;

			Vector3 channelOffset = channel.NextRandomOffset();
			Vector3 channelScale = channel.NextRandomScale();
			float channelRotation = channel.NextRandomAngle();

			for (var i = 0; i < definition.vertexGroups.Count; i++)
			{
				if (m_useLastResult && i == definition.vertexGroups.Count)
				{
					m_evalResult = m_lastResult;
				}
				else
				{
					Evaluate(Dmath.Lerp(from, to, definition.vertexGroups[i].percent), ref m_evalResult);
				}

				Vector3 originalNormal = m_evalResult.up;
				Vector3 originalRight = m_evalResult.right;
				Vector3 originalDirection = m_evalResult.forward;
				if (channel.overrideNormal)
				{
					m_evalResult.forward = Vector3.Cross(m_evalResult.right, channel.customNormal);
					m_evalResult.up = channel.customNormal;
				}

				(Vector2, float, Vector3) customValues = channel.GetCustomExtrudeValues(m_evalResult.percent);
				Vector3 finalOffset = offset + channelOffset + (Vector3)customValues.Item1;
				float finalRotation = rotation + channelRotation + customValues.Item2;
				Vector3 finalScale = channelScale;
				if (!channel.scaleModifier.useClippedPercent)
				{
					UnclipPercent(ref m_evalResult.percent);
				}

				Vector2 scaleMod = channel.scaleModifier.GetScale(m_evalResult);
				if (!channel.scaleModifier.useClippedPercent)
				{
					ClipPercent(ref m_evalResult.percent);
				}

				finalScale.x *= customValues.Item3.x * scaleMod.x;
				finalScale.y *= customValues.Item3.y * scaleMod.y;
				finalScale.z = 1f;
				float resultSize = m_evalResult.size;
				m_vertexMatrix.SetTRS(
					m_evalResult.position + originalRight * (finalOffset.x * resultSize) +
					originalNormal * (finalOffset.y * resultSize) + originalDirection * offset.z, //Position
					m_evalResult.rotation * Quaternion.AngleAxis(finalRotation, Vector3.forward), //Rotation
					finalScale * resultSize); //Scale
				m_normalMatrix = m_vertexMatrix.inverse.transpose;
				if (i == 0)
				{
					m_lastResult = m_evalResult;
				}

				for (var n = 0; n < definition.vertexGroups[i].ids.Length; n++)
				{
					int index = definition.vertexGroups[i].ids[n];
					trsVector = definition.vertices[index];
					trsVector.z = 0f;
					target.vertices[index] = m_vertexMatrix.MultiplyPoint3x4(trsVector);
					trsVector = definition.normals[index];
					target.normals[index] = m_normalMatrix.MultiplyVector(trsVector);
					target.colors[index] = target.colors[index] * m_evalResult.color * color;
					if (target.uv.Length > index)
					{
						uv = target.uv[index];
						switch (channel.overrideUVs)
						{
							case Channel.Uvoverride.ClampU: uv.x = (float)m_evalResult.percent; break;
							case Channel.Uvoverride.ClampV: uv.y = (float)m_evalResult.percent; break;
							case Channel.Uvoverride.UniformU: uv.x = CalculateLength(0.0, m_evalResult.percent); break;
							case Channel.Uvoverride.UniformV: uv.y = CalculateLength(0.0, m_evalResult.percent); break;
						}

						target.uv[index] = new Vector2(uv.x * uvScale.x * channel.uvScale.x,
							uv.y * uvScale.y * channel.uvScale.y);
						target.uv[index] += uvOffset + channel.uvOffset;
					}
				}
			}
		}
	}
}