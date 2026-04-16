using System;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
	[Serializable]
	public class SizeModifier : SplineSampleModifier
	{
		public SizeKey[] keys = new SizeKey[0];

		public SizeModifier()
		{
			keys = new SizeKey[0];
		}

		public override bool hasKeys => keys.Length > 0;

		public override List<Key> GetKeys()
		{
			return new List<Key>(keys);
		}

		public override void SetKeys(List<Key> input)
		{
			keys = new SizeKey[input.Count];
			for (var i = 0; i < input.Count; i++)
			{
				keys[i] = (SizeKey)input[i];
			}

			base.SetKeys(input);
		}

		public void AddKey(double f, double t)
		{
			ArrayUtility.Add(ref keys, new SizeKey(f, t));
		}

		public override void Apply(ref SplineSample result)
		{
			if (keys.Length == 0)
			{
				return;
			}

			base.Apply(ref result);
			for (var i = 0; i < keys.Length; i++)
			{
				result.size += keys[i].Evaluate(result.percent) * keys[i].size * blend;
			}
		}

		[Serializable]
		public class SizeKey : Key
		{
			public float size;

			public SizeKey(double f, double t) : base(f, t)
			{
			}
		}
	}
}