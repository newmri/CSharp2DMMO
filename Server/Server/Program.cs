using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;

namespace Server
{
	/*
	 Recv(N)
	 GameLogic(1)
	 Send(1)
	 DB(1)
	*/
	class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
		{
			while (true)
			{
				GameLogic.Instance.Update();
				Thread.Sleep(0);
			}
		}

		static void DbTask()
		{
			while (true)
			{
				DbTransaction.Instance.Flush();
				Thread.Sleep(0);
			}
		}

		static void NetworkTask()
		{
			while (true)
			{
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();

				foreach (ClientSession session in sessions)
				{
					session.FlushSend();
				}

				Thread.Sleep(0);
			}
		}

		static void StartServerInfoTask()
		{
			var t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
			{
				using (SharedDbContext shared = new SharedDbContext())
				{
					ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
					if (serverDb != null)
					{
						serverDb.IpAddress = Ip;
						serverDb.Port = Port;
						serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
						shared.SaveChangesEx();
					}
					else
					{
						serverDb = new ServerDb()
						{
							Name = Program.Name,
							IpAddress = Program.Ip,
							Port = Program.Port,
							BusyScore = SessionManager.Instance.GetBusyScore()
						};

						shared.Servers.Add(serverDb);
						shared.SaveChangesEx();
					}
				}
			});

			t.Interval = 10 * 1000;
			t.Start();
		}

		public static string Name { get; } = "데포르쥬";
		public static int Port { get; } = 7777;
		public static string Ip { get; set; }

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			GameLogic.Instance.Push(() =>
			{
				GameRoom room = GameLogic.Instance.Add(1);
			});

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

			Ip = ipAddr.ToString();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			StartServerInfoTask();

			{
				Thread t = new Thread(DbTask);
				t.Name = "DbTask";
				t.Start();
			}

			{
				Thread t = new Thread(NetworkTask);
				t.Name = "NetworkTask";
				t.Start();
			}

			Thread.CurrentThread.Name = "GameLogicTask";
			GameLogicTask();
		}
	}
}
