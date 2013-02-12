using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace trymonotouchandroid
{
	[Activity (Label = "Try Montouch Android", MainLauncher = true, Icon = "@drawable/icon")]
	public class Home: ListActivity 
	{
		public const int NUMBER_OF_CLIENTS = 4;
		
		private Timer timer = new Timer();
		private Server server;
		private List<Client> clients;
		private List<String> items = new List<String>(NUMBER_OF_CLIENTS + 1);
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			items = new List<String>();
			
			server = new Server();
			clients = new List<Client>();
			
			for (int i = 0; i < NUMBER_OF_CLIENTS + 1; i++) {
				clients.Add(new Client(server, i));
				items.Add("");
			}
			
			
			timer.Interval = 1000;
			timer.Elapsed += new ElapsedEventHandler(onTimer);
			timer.Start();
			
			updateUi();
		}
		
		private void updateUi ()
		{
			items [0] = serverStatus ();
			for (int i = 0; i < NUMBER_OF_CLIENTS; i++) {
				items[i+1] = clientStatus(clients[i]);
			}
			
			RunOnUiThread(() => { 
				ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, items); 
				((BaseAdapter) ListAdapter).NotifyDataSetChanged(); 
			});
		}
		
		private String serverStatus ()
		{
			String result = "Server: ";
			
			switch (server.state) {
			case ServerState.Idle:
				result += "Idle";
				break;
			case ServerState.Serving:
				result += " Serving #" + server.currentClient.id + " " + server.ticks + "s";
				break;
			}
			
			return result;
		}
		
		private String clientStatus (Client client)
		{
			String result = "Client #" + client.id + ": ";
			
			switch (client.state) {
			case ClientState.Idle:
				result += "Idle " + client.ticks + "s";
				break;
			case ClientState.Queued:
				result += "Queued " + client.numberInQueue;
				break;
			case ClientState.Requesting:
				result += "Requesting";
				break;
			}
			
			return result;
		}
		
		private void onTimer (object sender, ElapsedEventArgs e)
		{
			server.run();
			for (int i = 0; i < NUMBER_OF_CLIENTS; i++) {
				clients[i].run ();
			}
			
			updateUi();
		}
	}
	
	public enum ServerState
	{
		Idle,
		Serving
	}
	
	public enum ClientState
	{
		Idle,
		Queued,
		Requesting
	}
	
	public class Server
	{
		public const int MIN_RANDOM = 1;
		public const int MAX_RANDOM = 5;
		
		public ServerState state = ServerState.Idle;
		public int ticks; 
		public Client currentClient;
		
		private Random random = new Random();
		private Queue<Client> queue = new Queue<Client>();
		
		public void connect (Client client)
		{
			queue.Enqueue(client);
			client.state = ClientState.Queued;
			updateNumbersInQueue();
		}
		
		public void run ()
		{
			if ((state == ServerState.Idle) && (queue.Count != 0)) {
				currentClient = queue.Dequeue ();
				currentClient.state = ClientState.Requesting;
				state = ServerState.Serving;
				ticks = getRandomTicks();
				updateNumbersInQueue();
			} else if (state == ServerState.Serving) {
				ticks--;
				if (ticks == 0) {
					currentClient.disconnect();
					state = ServerState.Idle;
				}
			}
		}
		
		public void updateNumbersInQueue()
		{
			IEnumerator en = queue.GetEnumerator();
			for (int i = 0; i < queue.Count; i++) {
				en.MoveNext();
				((Client) en.Current).numberInQueue = i;
			}
		}
		
		private int getRandomTicks()
		{
			return random.Next(MIN_RANDOM, MAX_RANDOM); 
		}
	}
	
	public class Client
	{
		public const int MIN_RANDOM = 5;
		public const int MAX_RANDOM = 15;
		
		public ClientState state = ClientState.Idle;
		public int ticks; 
		public int id;
		public int numberInQueue;
		
		private Random random = new Random();
		private Server server;
		
		public Client (Server server, int id)
		{
			this.server = server;
			this.id = id;
			ticks = getRandomTicks(); 
		}
		
		public void disconnect ()
		{
			state = ClientState.Idle;
			ticks = getRandomTicks(); 
		}
		
		public void run ()
		{
			if (state == ClientState.Idle) {
				ticks--;
				if (ticks == 0) {
					server.connect(this);
					state = ClientState.Queued;
				}
			}
		}
		
		private int getRandomTicks()
		{
			return random.Next(MIN_RANDOM, MAX_RANDOM); 
		}
	}
}


