using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DpDisp.WPF
{
/// <summary>
/// MainWindow.xaml の相互作用ロジック
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
        if (dploop == false)
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
                        await Dispatcher.Invoke( async () => {
                            image1.Source = await _dp.GetImage();
                        });
                        await Task.Delay(500);
                    }
                });
                t.Start();
            }
            btnConnect.IsChecked = true;
            // btnConnect.Icon = new SymbolIcon(Symbol.Stop);
        }
        else
        {
            // 停止
            dploop = false;
            btnConnect.IsChecked = false;
            // btnConnect.Icon = new SymbolIcon(Symbol.Play);
        }
    }

    /// <summary>
    /// 画像保存
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AppBarSave_Click(object sender, RoutedEventArgs e)
    {
        var now = DateTime.Now;
        var filename = "dpt-" + now.ToString("yyyy-MM-dd HHmmss") + ".jpg";
        var wb = image1.Source as BitmapSource;

        var folder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\" + "DpDisp";
        if ( !System.IO.Directory.Exists(folder))
        {
            System.IO.Directory.CreateDirectory(folder );
        }
        var path = folder + "\\" + filename;

        FileStream filestream = new FileStream(path, FileMode.Create);
        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(wb));
        encoder.Save(filestream);
    }
}
}
