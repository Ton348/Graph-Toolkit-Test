namespace Prototype.Business.NPC.Workers
{
	public enum WorkerNPCState
	{
		Spawning,
		MovingToEntrance,
		MovingToStorage,
		MovingToShelves,
		MovingToCashRegister,
		CarryingGoods,
		RestockingShelves,
		WaitingStock,
		WaitingCustomer,
		ServingCustomer,
		Despawning
	}
}
