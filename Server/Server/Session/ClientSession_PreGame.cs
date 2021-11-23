using Google.Protobuf.Protocol;
using Server.DB;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public void HandleLogin(C_Login loginPacket)
        {
			Console.WriteLine($"UniqueId({loginPacket.UniqueId})");

			if (ServerState != PlayerServerState.ServerStateLogin)
				return;

			using (AppDbContext db = new AppDbContext())
			{
				AccountDb findAccount = db.Accounts.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

				if (findAccount == null)
				{
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
					db.Accounts.Add(newAccount);
					db.SaveChanges();
				}

				S_Login loginOk = new S_Login() { LoginOk = 1 };
				Send(loginOk);
			}
		}
    }
}
