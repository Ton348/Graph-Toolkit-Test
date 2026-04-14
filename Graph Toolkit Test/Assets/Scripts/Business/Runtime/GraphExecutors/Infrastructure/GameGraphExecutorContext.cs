using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

internal static class GameGraphExecutorContext
{
	public static bool TryGetBootstrap(GraphExecutionContext context, out GameBootstrap bootstrap)
	{
		bootstrap = null;
		return context != null && context.TryGet(GraphContextKeys.RuntimeBootstrap, out bootstrap) && bootstrap != null;
	}

	public static async UniTask<ServerActionResult> ExecuteServerAsync(GraphExecutionContext context, UniTask<ServerActionResult> action)
	{
		ServerActionResult result = await action;
		context?.Set(GraphContextKeys.ServerLastResult, result);

		if (result?.ProfileSnapshot != null && TryGetBootstrap(context, out GameBootstrap bootstrap) && bootstrap.ProfileSyncService != null)
		{
			bootstrap.ProfileSyncService.ApplySnapshot(result.ProfileSnapshot);
		}

		return result;
	}

	public static async UniTask<ServerActionResult> ExecuteServerAsync(GraphExecutionContext context, System.Threading.Tasks.Task<ServerActionResult> action)
	{
		ServerActionResult result = await action;
		context?.Set(GraphContextKeys.ServerLastResult, result);

		if (result?.ProfileSnapshot != null && TryGetBootstrap(context, out GameBootstrap bootstrap) && bootstrap.ProfileSyncService != null)
		{
			bootstrap.ProfileSyncService.ApplySnapshot(result.ProfileSnapshot);
		}

		return result;
	}
}
