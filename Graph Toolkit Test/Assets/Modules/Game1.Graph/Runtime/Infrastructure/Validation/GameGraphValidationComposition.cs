using System;
using Game1.Graph.Runtime.Validation;

namespace Game1.Graph.Runtime.Infrastructure.Validation
{
	public sealed class GameGraphValidationComposition
	{
		public GameGraphValidationComposition(GameGraphNodeValidatorRegistry validatorRegistry)
		{
			ValidatorRegistry = validatorRegistry ?? throw new ArgumentNullException(nameof(validatorRegistry));
		}

		public GameGraphNodeValidatorRegistry ValidatorRegistry { get; }

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