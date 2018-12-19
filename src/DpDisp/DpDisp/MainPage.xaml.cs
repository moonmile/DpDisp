using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace DpDisp
{
/// <summary>
/// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
/// </summary>
public sealed partial class MainPage : Page
{
public MainPage()
{
	this.InitializeComponent();
}

DpService _dp;
bool dploop = false;
/// <summary>
/// DPと接続
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
private async void AppBarConnect_Click(object sender, RoutedEventArgs e)
{
	if ( dploop == false )
	{
		// 接続
		if (_dp == null)
		{
			_dp = new DpService();
			if (await _dp.Connect() == false)
			{
				_dp = null;
				return;
			}
		}
		// 定期的に実行
		if (dploop == false)
		{
			var t = new Task(async () =>
					{
						dploop = true;
						while (dploop)
						{
						        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
								image1.Source = await _dp.GetImage();
							});
						        await Task.Delay(500);
						}
					});
			t.Start();
		}
		btnConnect.Icon = new SymbolIcon(Symbol.Stop);
	}
	else
	{
		// 停止
		dploop = false;
		btnConnect.Icon = new SymbolIcon(Symbol.Play);
	}
}

/// <summary>
/// 画像保存
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
private async void AppBarSave_Click(object sender, RoutedEventArgs e)
{
	var now = DateTime.Now;
	var filename = "dpt-" + now.ToString("yyyy-MM-dd HHmmss") + ".jpg";

	var wb = image1.Source as WriteableBitmap;
	SoftwareBitmap outputBitmap = SoftwareBitmap.CreateCopyFromBuffer( wb.PixelBuffer, BitmapPixelFormat.Bgra8, wb.PixelWidth, wb.PixelHeight );

	StorageFolder storageFolder = KnownFolders.PicturesLibrary;
	StorageFolder folder;
	try
	{
		folder = await storageFolder.GetFolderAsync("DpDisp");
	}
	catch
	{
		await storageFolder.CreateFolderAsync("DpDisp");
		folder = await storageFolder.GetFolderAsync("DpDisp");
	}

	StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
	SaveSoftwareBitmapToFile(outputBitmap, file);
}


private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
{
	using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
	{
		// Create an encoder with the desired format
		BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

		// Set the software bitmap
		encoder.SetSoftwareBitmap(softwareBitmap);

		// Set additional encoding parameters, if needed
		encoder.BitmapTransform.ScaledWidth = 1200;
		encoder.BitmapTransform.ScaledHeight = 1600;
		// encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
		encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
		encoder.IsThumbnailGenerated = true;

		try
		{
			await encoder.FlushAsync();
		}
		catch (Exception err)
		{
			switch (err.HResult)
			{
			case unchecked ((int)0x88982F81): //WINCODEC_ERR_UNSUPPORTEDOPERATION
				// If the encoder does not support writing a thumbnail, then try again
				// but disable thumbnail generation.
				encoder.IsThumbnailGenerated = false;
				break;
			default:
				throw err;
			}
		}

		if (encoder.IsThumbnailGenerated == false)
		{
			await encoder.FlushAsync();
		}


	}
}
}
}
