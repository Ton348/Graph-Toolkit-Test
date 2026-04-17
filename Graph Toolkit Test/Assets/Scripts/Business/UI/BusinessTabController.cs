using UnityEngine;
using UnityEngine.UI;

namespace Prototype.Business.UI
{
	public class BusinessTabController : MonoBehaviour
	{
		public Button overviewButton;
		public Button equipmentButton;
		public Button logisticsButton;
		public Button staffButton;
		public Button pricesButton;

		public GameObject overviewTab;
		public GameObject equipmentTab;
		public GameObject logisticsTab;
		public GameObject staffTab;
		public GameObject pricesTab;

		public int startTabIndex;

		private void Awake()
		{
			Hook(overviewButton, 0);
			Hook(equipmentButton, 1);
			Hook(logisticsButton, 2);
			Hook(staffButton, 3);
			Hook(pricesButton, 4);

			ShowTab(startTabIndex);
		}

		private void Hook(Button button, int index)
		{
			if (button == null)
			{
				return;
			}

			button.onClick.AddListener(() => ShowTab(index));
		}

		public void ShowTab(int index)
		{
			SetActive(overviewTab, index == 0);
			SetActive(equipmentTab, index == 1);
			SetActive(logisticsTab, index == 2);
			SetActive(staffTab, index == 3);
			SetActive(pricesTab, index == 4);
		}

		private static void SetActive(GameObject go, bool active)
		{
			if (go == null)
			{
				return;
			}

			go.SetActive(active);
		}
	}
}