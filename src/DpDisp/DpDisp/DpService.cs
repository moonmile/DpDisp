using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DpDisp
{
class DpService
{
const string BROADCAST_ADDR = "203.0.113.255";
const int BROADCAST_PORT = 54321;
byte[] CMD_REQUEST_RESPONSE = { 1 };
byte[] CMD_RESPONSE = { 15 };

IPEndPoint _ep = null;

public async Task<bool> Connect()
{
	// ブロードキャスト送信
	var client = new UdpClient();
	client.EnableBroadcast = true;
	int n = await client.SendAsync(
		CMD_REQUEST_RESPONSE, CMD_REQUEST_RESPONSE.Length,
		new IPEndPoint(IPAddress.Parse(BROADCAST_ADDR), BROADCAST_PORT));
	// _ep = new IPEndPoint(IPAddress.Any, 0);
	var recv = await client.ReceiveAsync();
	_ep = recv.RemoteEndPoint;
	client.Dispose();

	return true;
}
public async Task<ImageSource> GetImage()
{
	if (_ep == null) return null;

	var tcp = new TcpClient();
	await tcp.ConnectAsync(_ep.Address, _ep.Port);
	// xml 部分を取得する
	var s = "";
	while(true)
	{
		byte[] b = new byte[1];
		tcp.Client.Receive(b);
		if (b[0] == 0x0A) break;
		// Console.WriteLine("recv {0}: {1}", i++, Convert.ToChar( b[0] ) );
		s += Convert.ToChar(b[0]);
	}
	// Console.WriteLine(s);
	System.Diagnostics.Debug.WriteLine(s);
	// 画像部分を取得する
	int len = 0;
	var mem = new System.IO.MemoryStream();
	while (true)
	{
		byte[] b = new byte[1];
		int m = tcp.Client.Receive(b);
		if (m != 1) break;
		len++;
		mem.Write(b, 0, b.Length);
	}
	// Console.WriteLine("size: {0}", mem.Length);
	// tcp.Close();
	tcp.Dispose();

	mem.Position = 0;
	var image = mem.ToArray();

	// 仮のサイズを作る
	var bmp = new WriteableBitmap(100, 100);
	// 実際は1600x1200で取得
	bmp.SetSource(mem.AsRandomAccessStream());
	return bmp;
}
}
}
