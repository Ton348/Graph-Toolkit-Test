using Prototype.Business.Runtime;

namespace Prototype.Business.Services
{
	public class ServerActionResult
	{
		public enum ErrorType
		{
			None,
			GameLogicError,
			NetworkError,
			Timeout
		}

		public bool Success { get; private set; }
		public ErrorType Type { get; private set; }
		public string ErrorCode { get; private set; }
		public string Message { get; private set; }
		public ProfileSnapshot ProfileSnapshot { get; private set; }

		public static ServerActionResult Ok(string message = null)
		{
			return new ServerActionResult
			{
				Success = true,
				Type = ErrorType.None,
				Message = message
			};
		}

		public static ServerActionResult Fail(string errorCode, string message = null)
		{
			return new ServerActionResult
			{
				Success = false,
				Type = ErrorType.GameLogicError,
				ErrorCode = errorCode,
				Message = message
			};
		}

		public static ServerActionResult SuccessResult(string message = null)
		{
			return Ok(message);
		}

		public static ServerActionResult SuccessResult(ProfileSnapshot snapshot, string message = null)
		{
			return new ServerActionResult
			{
				Success = true,
				Type = ErrorType.None,
				Message = message,
				ProfileSnapshot = snapshot
			};
		}

		public static ServerActionResult FailResult(string errorCode, string message = null)
		{
			return Fail(errorCode, message);
		}

		public static ServerActionResult FailResult(ErrorType type, string errorCode, string message = null)
		{
			return new ServerActionResult
			{
				Success = false,
				Type = type,
				ErrorCode = errorCode,
				Message = message
			};
		}
	}
}