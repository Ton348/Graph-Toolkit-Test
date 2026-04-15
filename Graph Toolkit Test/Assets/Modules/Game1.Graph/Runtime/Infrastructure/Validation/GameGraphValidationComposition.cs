using System;

namespace Game1.Graph.Runtime
{
	public sealed class GameGraphValidationComposition
	{
		public GameGraphNodeValidatorRegistry ValidatorRegistry { get; }

		public GameGraphValidationComposition(GameGraphNodeValidatorRegistry validatorRegistry)
		{
			ValidatorRegistry = validatorRegistry ?? throw new ArgumentNullException(nameof(validatorRegistry));
		}

		public void RegisterValidator(IGameGraphNodeValidator validator)
		{
			ValidatorRegistry.Register(validator);
		}

		public void RegisterValidator<TValidator>() where TValidator : IGameGraphNodeValidator, new()
		{
			ValidatorRegistry.Register<TValidator>();
		}

		public static GameGraphValidationComposition CreateDefault()
		{
			return new GameGraphValidationComposition(new GameGraphNodeValidatorRegistry());
		}
	}
}
