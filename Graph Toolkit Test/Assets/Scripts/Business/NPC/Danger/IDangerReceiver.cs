namespace Prototype.Business.NPC.Danger
{
	public interface IDangerReceiver
	{
		void ReceiveDanger(in DangerEvent dangerEvent);
	}
}
