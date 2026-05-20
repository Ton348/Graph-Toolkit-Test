using Prototype.Business.Runtime;
using TMPro;
using UnityEngine;

namespace Prototype.Business.World
{
	public class BusinessShelfVisual : MonoBehaviour
	{
		public BusinessWorldRuntime worldRuntime;
		public TMP_Text valueText;
		public Transform fillTransform;
		public Vector3 minScale = new(1f, 0.05f, 1f);
		public Vector3 maxScale = new(1f, 1f, 1f);

		private void OnEnable()
		{
			Refresh();
		}

		private void Refresh()
		{
			if (worldRuntime == null)
			{
				worldRuntime = GetComponentInParent<BusinessWorldRuntime>();
			}

			BusinessInstanceSnapshot state = worldRuntime != null ? worldRuntime.GetBusiness() : null;
			if (state == null)
			{
				return;
			}

			int capacity = 1;
			if (worldRuntime.bootstrap != null)
			{
				var calc = new BusinessCalculationService(worldRuntime.bootstrap.BusinessDefinitionsRepository, worldRuntime.bootstrap.GameDataRepository);
				capacity = Mathf.Max(1, calc.GetShelfCapacity(state));
			}
			float ratio = Mathf.Clamp01((float)Mathf.Max(0, state.shelfStock) / capacity);

			if (fillTransform != null)
			{
				fillTransform.localScale = Vector3.Lerp(minScale, maxScale, ratio);
			}

			if (valueText != null)
			{
				valueText.text = $"{Mathf.Max(0, state.shelfStock)}/{capacity}";
			}
		}
	}
}
