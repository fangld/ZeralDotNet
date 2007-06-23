using System;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using System.Diagnostics;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.TestLibBitTorrent.TestEncrypter
{
    public class DummyEncryptedConnection
	{
		private const string protocol_name = "BitTorrent protocol";

		DummyEncrypter encrypter;
		DummyRawConnection connection;
		public byte[] id;
		bool locally_initiated;
		public bool complete;
		bool closed;
		MemoryStream buffer;
		int next_len;
		FuncDelegate next_func;

		public DummyEncryptedConnection(DummyEncrypter encrypter, DummyRawConnection connection, byte[] id)
		{
			this.encrypter = encrypter;
			this.connection = connection;
			this.id = id;
			this.locally_initiated = (id != null);
			this.complete = false;
			this.closed = false;
			this.buffer = new MemoryStream();
			this.next_len = 1;
			this.next_func = new FuncDelegate(this.read_header_len);
			connection.write(new byte[] {(byte)protocol_name.Length} );
			connection.write(Encoding.ASCII.GetBytes(protocol_name));
			connection.write(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
			connection.write(encrypter.download_id);
			connection.write(encrypter.my_id);
		}

		public string get_ip()
		{
			return connection.get_ip();
		}

		public byte[] get_id()
		{
			return id;
		}

		public bool is_locally_initiated()
		{
			return locally_initiated;
		}
		
		public bool is_flushed()
		{
			return connection.is_flushed();
		}

		public NextFunction read_header_len(byte[] s)
		{
			if (s[0] != protocol_name.Length)
				return null;
			return new NextFunction(protocol_name.Length, new FuncDelegate(read_header));
		}

		public NextFunction read_header(byte[] s)
		{
			string pName = Encoding.ASCII.GetString(s, 0, protocol_name.Length);
			if (pName != protocol_name)
				return null;
			return new NextFunction(8, new FuncDelegate(read_reserved));
		}

		public NextFunction read_reserved(byte[] s)
		{
			return new NextFunction(20, new FuncDelegate(read_download_id));
		}

		public NextFunction read_download_id(byte[] s)
		{
			for (int i=0; i<20; i++)
				if (s[i] != encrypter.download_id[i])
					return null;
			return new NextFunction(20, new FuncDelegate(read_peer_id));
		}

		public NextFunction read_peer_id(byte[] s)
		{
			if (id == null)
				id = s;
			else
				for (int i=0; i<20; i++)
					if (s[i] != id[i])
						return null;
			complete = true;
			encrypter.connecter.connection_made(this);
			return new NextFunction(4, new FuncDelegate(read_len));
		}

		public NextFunction read_len(byte[] s)
		{
			uint n = BitConverter.ToUInt32(s, 0);
			int l = (int) EndianReverse(n);
			if (l > encrypter.max_len)
				return null;
			return new NextFunction(l, new FuncDelegate(read_message));
		}

		static uint EndianReverse(uint x) 
		{ 
			return ((x<<24) | ((x & 0xff00)<<8) | ((x & 0xff0000)>>8) | (x>>24));
		}


		public NextFunction read_message(byte[] s)
		{
			try
			{
				if (s.Length > 0)
					encrypter.connecter.got_message(this, s);
			}
				//TODO: catch (KeyboardInterrupt) {}
			catch //(Exception ex)
			{
				//TODO: Write error log
			}
			return new NextFunction(4, new FuncDelegate(read_len));
		}

		public void close()
		{
			if (!closed)
			{
				connection.close();
				sever();
			}
		}

		public void sever()
		{
			closed = true;
			encrypter.connections.Remove(connection);
			if (complete)
				encrypter.connecter.connection_lost(this);
		}

		public void send_message(byte message)
		{
			send_message(new byte[] {message});
		}

		public void send_message(byte[] message)
		{
			byte[] temp = BitConverter.GetBytes(message.Length);
			byte t;
			t = temp[0];
			temp[0] = temp[3];
			temp[3] = t;
			t = temp[1];
			temp[1] = temp[2];
			temp[2] = t;
			connection.write(temp);
			connection.write(message);
		}

		public void data_came_in(byte[] s)
		{
			while (true)
			{
				if (this.closed)
					return;
				int i = this.next_len - (int) this.buffer.Position;
				if (i > s.Length)
				{
					this.buffer.Write(s,0,s.Length);
					return;
				}
				this.buffer.Write(s, 0, i);
				byte[] t = new byte[s.Length - i];
				Buffer.BlockCopy(s, i, t, 0, s.Length - i);
				s = t;
				byte[] m = this.buffer.ToArray();
				this.buffer.Close();
				this.buffer = new MemoryStream();
				NextFunction x = this.next_func(m);
				if (x == null)
				{
					this.close();
					return;
				}
				this.next_len = x.Length;
				this.next_func = x.NextFunc;
			}
		}
	}

	public class DummyEncrypter
	{
		public DummyConnecter connecter;
		private DummyRawServer raw_server;
		public ListDictionary connections;
		public byte[] my_id;
		public int max_len;
		SchedulerDelegate schedulefunc;
		double keepalive_delay;
		public byte[] download_id;
		int max_initiate;

		public DummyEncrypter(DummyConnecter connecter, DummyRawServer raw_server, byte[] my_id, int max_len, SchedulerDelegate schedulefunc, double keepalive_delay, byte[] download_id, int max_initiate)
		{
			this.raw_server = raw_server;
			this.connecter = connecter;
			this.my_id = my_id;
			this.max_len = max_len;
			this.schedulefunc = schedulefunc;
			this.keepalive_delay = keepalive_delay;
			this.download_id = download_id;
			this.max_initiate = max_initiate;
			this.connections = new ListDictionary();
			schedulefunc(new TaskDelegate(send_keepalives), keepalive_delay, "Keep alive delay");
		}

		public void send_keepalives()
		{
            schedulefunc(new TaskDelegate(send_keepalives), keepalive_delay, "Keep alive delay");
			foreach (DummyEncryptedConnection c in connections.Values)
				if (c.complete)
					c.send_message(new byte[0]);
		}

		public void start_connection(IPEndPoint dns, byte[] id)
		{
			if (connections.Count >= max_initiate)
				return;
			bool bIdentical = true;
			for (int i = 0; i < id.Length; i++)
				if (id[i] != my_id[i])
				{
					bIdentical = false;
					break;
				}
			if (bIdentical)
				return;
			foreach (DummyEncryptedConnection v in connections.Values)
			{
				bIdentical = true;
				for (int i = 0; i < id.Length; i++)
					if (id[i] != v.id[i])
					{
						bIdentical = false;
						break;
					}
				if (bIdentical)
					return;
			}

			try
			{
				DummyRawConnection c = raw_server.start_connection(dns, null);
				connections[c] = new DummyEncryptedConnection(this, c, id);
			}
			catch (Exception ex)
			{
				int i = 0;
			}
		}        
		public void external_connection_made(DummyRawConnection connection)
		{
			connections[connection] = new DummyEncryptedConnection(this, connection, null);
		}

		public void connection_flushed(DummyRawConnection connection)
		{
			DummyEncryptedConnection c = (DummyEncryptedConnection) connections[connection];
			if (c.complete)
				connecter.connection_flushed(c);
		}

		public void connection_lost(DummyRawConnection connection)
		{
			((DummyEncryptedConnection)connections[connection]).sever();
		}
        
		public void data_came_in(DummyRawConnection connection, byte[] data)
		{
			((DummyEncryptedConnection)connections[connection]).data_came_in(data);
		}
	}

	public class DummyConnecter
	{
		public ArrayList log;
		public bool close_next;

		public DummyConnecter()
		{
			this.log = new ArrayList();
			this.close_next = false;
		}

		public void connection_made(DummyEncryptedConnection connection)
		{
			log.Add(new object[] {"made ", connection});
		}

		public void connection_lost(DummyEncryptedConnection connection)
		{
			log.Add(new object[] {"lost ", connection});
		}

		public void connection_flushed(DummyEncryptedConnection connection)
		{
			log.Add(new object[] {"flushed ", connection});
		}

		public void got_message(DummyEncryptedConnection connection, byte[] message)
		{
			log.Add(new object[] {"got ", connection, message});
			if (close_next)
				connection.close();
		}
	}

	public class DummyRawServer
	{
		public ArrayList connects;

		public DummyRawServer()
		{
			connects = new ArrayList();
		}

		public DummyRawConnection start_connection(IPEndPoint dns, object o)
		{
			DummyRawConnection c = new DummyRawConnection();
			connects.Add(new object[] {dns, c});
			return c;
		}
	}

	public class DummyRawConnection
	{
		public bool closed;
		public ArrayList data;
		public bool flushed;

		public DummyRawConnection()
		{
			closed = false;
			data = new ArrayList();
			flushed = true;
		}

		public string get_ip()
		{
			return "fake.ip";
		}

		public bool is_flushed()
		{
			return flushed;
		}

		public void write(byte[] data)
		{
			Debug.Assert(!closed);
			this.data.Add(data);
		}

		public void close()
		{
			Debug.Assert(!closed);
			closed = true;
		}

		public string pop()
		{
			int length = 0;
			foreach(byte[] b in data)
			{
				length += b.Length;
			}
			byte[] rv = new byte[length];
			length = 0;
			foreach(byte[] b in data)
			{
				Buffer.BlockCopy(b, 0, rv, length, b.Length);
				length += b.Length;
			}
			data.Clear();
			return Encoding.ASCII.GetString(rv);
		}
	}

    [TestFixture]
    public class TestEncrypter
    {
        private const string protocol_name = "BitTorrent protocol";

        public static void DummySchedule(TaskDelegate t, double a, string taskName)
        {
        }

        static ArrayList log;

        public static void ScheduleLog(TaskDelegate t, double a, string taskName)
        {
            log.Add(new object[] { t, a });
        }

        [Test]
        public void test_messages_in_and_out()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            string temp = c1.pop();
            byte[] bytes = new byte[48];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
                bytes[28 + i] = 1;
            e.data_came_in(c1, bytes);
            temp = c1.pop();
            bytes = new byte[20];
            for (int i = 0; i < 20; i++)
                bytes[i] = (byte)'b';
            e.data_came_in(c1, bytes);
            temp = c1.pop();
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            temp = ch.get_ip();
            ch.send_message(new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            temp = c1.pop();

            bytes = new byte[] { 0, 0, 0, 3, (byte)'d', (byte)'e', (byte)'f' };
            e.data_came_in(c1, bytes);
        }

        [Test]
        public void test_flushed()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            string temp = c1.pop();
            byte[] bytes = new byte[48];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
                bytes[28 + i] = 1;
            e.data_came_in(c1, bytes);
            temp = c1.pop();

            e.connection_flushed(c1);

            temp = c1.pop();
            bytes = new byte[20];
            for (int i = 0; i < 20; i++)
                bytes[i] = (byte)'b';
            e.data_came_in(c1, bytes);
            temp = c1.pop();
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Debug.Assert(rs.connects.Count == 0);
            Debug.Assert(!c1.closed);
            Debug.Assert(ch.is_flushed());

            e.connection_flushed(c1);
            temp = c1.pop();

            c1.flushed = false;
            Debug.Assert(!ch.is_flushed());
        }

        [Test]
        public void test_wrong_header_length()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            string temp = c1.pop();
            byte[] bytes = new byte[30];
            for (int i = 0; i < 30; i++)
                bytes[i] = 5;
            e.data_came_in(c1, bytes);
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(c1.closed);
        }

        [Test]
        public void test_wrong_header()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            string temp = c1.pop();
            byte[] bytes = new byte[20];
            bytes[0] = 19;
            for (int i = 0; i < 19; i++)
                bytes[i + 1] = (byte)'a';
            e.data_came_in(c1, bytes);
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(c1.closed);
        }

        [Test]
        public void test_wrong_download_id()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            string temp = c1.pop();
            byte[] bytes = new byte[48];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 1;
            for (int i = 0; i < 20; i++)
                bytes[28 + i] = 2;
            e.data_came_in(c1, bytes);
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(c1.closed);
        }

        [Test]
        public void test_wrong_other_id()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            e.start_connection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969), new byte[] { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 });
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(rs.connects.Count == 1);
            DummyRawConnection c1 = (DummyRawConnection)((object[])rs.connects[0])[1];
            rs.connects.Clear();
            string temp = c1.pop();
            Debug.Assert(!c1.closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.data_came_in(c1, bytes);
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(c1.closed);
        }

        [Test]
        public void text_over_max_len()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(rs.connects.Count == 0);
            Debug.Assert(!c1.closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.data_came_in(c1, bytes);
            Debug.Assert(c.log.Count == 1);

            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Debug.Assert(!c1.closed);

            e.data_came_in(c1, new byte[] { 1, 0, 0, 0 });
            Debug.Assert(c.log.Count == 1);
            Debug.Assert(c1.closed);
        }

        [Test]
        public void test_keepalive()
        {
            log = new ArrayList();

            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(ScheduleLog), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            Debug.Assert(log.Count == 1);
            Debug.Assert(((double)((object[])log[0])[1]) == 30);
            TaskDelegate kfunc = (TaskDelegate)((object[])log[0])[0];
            log.Clear();
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            string temp = c1.pop();
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(!c1.closed);
            kfunc();
            Debug.Assert(c1.pop() == "");
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(!c1.closed);
            log.Clear();

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.data_came_in(c1, bytes);
            Debug.Assert(c.log.Count == 1);
            c.log.Clear();
            Debug.Assert(c1.pop() == "");
            Debug.Assert(!c1.closed);

            kfunc();
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(!c1.closed);

        }

        [Test]
        public void test_swallow_keepalive()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0,rs.connects.Count);
            Assert.AreEqual(false, c1.closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.data_came_in(c1, bytes);
            Assert.AreEqual(1,c.log.Count);
            c.log.Clear();
            Assert.AreEqual(false,c1.closed);

            e.data_came_in(c1, new byte[] { 0, 0, 0, 0 });
            Assert.AreEqual(0,c.log.Count);
            Assert.AreEqual(false,c1.closed);

        }

        [Test]
        public void test_local_close()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            Assert.AreEqual(0,c.log.Count);
            Assert.AreEqual(0,rs.connects.Count);
            Assert.AreEqual(false, c1.closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.data_came_in(c1, bytes);
            Assert.AreEqual(1,c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false,c1.closed);

            ch.close();
            Assert.AreEqual(true,c1.closed);
        }

        [Test]
        public void test_local_close_in_message_receive()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            Assert.AreEqual(0,c.log.Count);
            Assert.AreEqual(0,rs.connects.Count);
            Assert.AreEqual(false, c1.closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.data_came_in(c1, bytes);
            Assert.AreEqual(1,c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false,c1.closed);

            c.close_next = true;
            e.data_came_in(c1, new byte[] { 0, 0, 0, 4, 1, 2, 3, 4 });
            Assert.AreEqual(true,c1.closed);
        }

        [Test]
        public void test_remote_close()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            Assert.AreEqual(0,c.log.Count);
            Assert.AreEqual(0,rs.connects.Count);
            Assert.AreEqual(false,c1.closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.data_came_in(c1, bytes);
            Assert.AreEqual(1,c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false,c1.closed);

            e.connection_lost(c1);
            Assert.AreEqual(false,c1.closed);
        }

        [Test]
        public void test_partial_data_in()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            Assert.AreEqual(0,c.log.Count);
            Assert.AreEqual(0,rs.connects.Count);
            Assert.AreEqual(false,c1.closed);

            byte[] bytes = new byte[24];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 4; i++)
                bytes[20 + i] = 0;
            e.data_came_in(c1, bytes);
            bytes = new byte[34];
            for (int i = 0; i < 4; i++)
                bytes[i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[4 + i] = 1;
            }
            for (int i = 0; i < 10; i++)
            {
                bytes[24 + i] = 2;
            }
            e.data_came_in(c1, bytes);
            bytes = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                bytes[i] = 2;
            }
            e.data_came_in(c1, bytes);
            Assert.AreEqual(1,c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false, c1.closed);

        }

        [Test]
        public void test_ignore_connect_of_extant()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();
            e.external_connection_made(c1);
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(rs.connects.Count == 0);
            Debug.Assert(!c1.closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(protocol_name), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (int i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 9;
            }
            e.data_came_in(c1, bytes);
            Assert.AreEqual(1,c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false,c1.closed);

            e.start_connection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969), new byte[] { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 });
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0,rs.connects.Count);
            Assert.AreEqual(false, c1.closed);
        }

        [Test]
        public void test_ignore_connect_to_self()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummyRawConnection c1 = new DummyRawConnection();

            e.start_connection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false,c1.closed);
        }
    }
}
