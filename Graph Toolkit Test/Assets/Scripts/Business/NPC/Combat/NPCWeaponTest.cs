using System;
using System.Reflection;
using UnityEngine;

namespace Prototype.Business.NPC.Combat
{
	public sealed class NPCWeaponTest : MonoBehaviour
	{
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private Transform muzzlePoint;
		[SerializeField] private int damage = 1;

		public void Fire()
		{
			if (bulletPrefab == null || muzzlePoint == null)
			{
				return;
			}

			GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
			Component bulletBehaviour = bullet.GetComponent("BulletTest");
			if (bulletBehaviour == null)
			{
				return;
			}

			Type bulletType = Type.GetType("Prototype.Player.BulletTest, Assembly-CSharp");
			Type ownerTypeType = Type.GetType("Prototype.Player.BulletOwnerType, Assembly-CSharp");
			if (bulletType == null || ownerTypeType == null)
			{
				return;
			}

			MethodInfo initialize = bulletType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
			if (initialize == null)
			{
				return;
			}

			object ownerEnum = Enum.Parse(ownerTypeType, "Npc");
			if (bulletType.IsInstanceOfType(bulletBehaviour))
			{
				initialize.Invoke(bulletBehaviour, new object[] { damage, ownerEnum, transform });
			}
		}
	}
}
