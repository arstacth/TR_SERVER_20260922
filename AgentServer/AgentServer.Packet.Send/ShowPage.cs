using AgentServer.Holders;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using Serilog;

namespace AgentServer.Packet.Send
{
	public sealed class ShowPage : NetPacket
	{
		public ShowPage(Account User, byte type, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CLIENT_WEB_PAGE_URL_ACK);
			ns.Write(type);
			string value;
			switch (type)
			{
			default:
				Log.Warning("Unknown show page type {0}", type);
				value = string.Empty;
				break;
			case 0:
				value = ServerSettingHolder.ServerSettings.GateNoticeURL;
				break;
			case 1:
				value = (ServerSettingHolder.ServerSettings.cashFillUpURL ?? string.Empty).Replace("{userid}", User.UserID).Replace("{loginkey}", User.LoginKey).Replace("{country}", ServerSettingHolder.ServerSettings.countrycode);
				break;
			case 2:
			case 3:
				value = (ServerSettingHolder.ServerSettings.EveryDayEventURL ?? string.Empty).Replace("{userid}", User.UserID).Replace("{loginkey}", User.LoginKey).Replace("{country}", ServerSettingHolder.ServerSettings.countrycode);
				break;
			case 6:
				value = ServerSettingHolder.ServerSettings.QuitConfirmDialogURL ?? string.Empty;
				break;
			case 27:
				value = (ServerSettingHolder.ServerSettings.GuildMarkRegisterURL ?? string.Empty).Replace("{userid}", User.UserID).Replace("{loginkey}", User.LoginKey).Replace("{country}", ServerSettingHolder.ServerSettings.countrycode);
				break;
			}
			ns.WriteAnsiFixed_intSize(value ?? string.Empty);
			_ = last;
		}
	}
}
