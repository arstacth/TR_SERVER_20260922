namespace AgentServer.Holders
{
	public class ReLoginUserInfo
	{
		public string strID;

		public string strIP;

		public int Session;

		public int iServerNum;

		public ReLoginUserInfo()
		{
			Session = 0;
			iServerNum = 0;
		}
	}
}
