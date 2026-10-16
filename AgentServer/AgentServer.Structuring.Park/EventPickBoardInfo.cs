using System;

namespace AgentServer.Structuring.Park
{
	public class EventPickBoardInfo
	{
		public DateTime StartDateTime;

		public DateTime EndDateTime;

		public int ConstructType;

		public int StepUpturnType;

		public int StepResetType;

		public byte LastStep;
	}
}
