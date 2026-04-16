using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GraphCore.Runtime;

internal static class GameGraphExecutorContext
{
	public static bool TryGetBootstrap(GraphExecutionContext context, out GameBootstrap bootstrap)
	{
		bootstrap = null;
		return context != null && context.TryGet(GraphContextKeys.runtimeBootstrap, out bootstrap) && bootstrap != null;
	}

	public static async UniTask<ServerActionResult> ExecuteServerAsync(
		GraphExecutionContext context,
		UniTask<ServerActionResult> action)
	{
		ServerActionResult result = await action;
		context?.Set(GraphContextKeys.serverLastResult, result);

		if (result?.ProfileSnapshot != null && TryGetBootstrap(context, out GameBootstrap bootstrap) &&
		    bootstrap.ProfileSyncService != null)
		{
			bootstrap.ProfileSyncService.ApplySnapshot(result.ProfileSnapshot);
		}

		return result;
	}

	public static async UniTask<ServerActionResult> ExecuteServerAsync(
		GraphExecutionContext context,
		Task<ServerActionResult> action)
	{
		ServerActionResult result = await action;
		context?.Set(GraphContextKeys.serverLastResult, result);

		if (result?.ProfileSnapshot != null && TryGetBootstrap(context, out GameBootstrap bootstrap) &&
		    bootstrap.ProfileSyncService != null)
		{
			bootstrap.ProfileSyncService.ApplySnapshot(result.ProfileSnapshot);
		}

		return result;
	}
}