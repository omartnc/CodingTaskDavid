using CodingTaskDavidSocketModels;
using System;
using System.Collections.Generic;
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
using System.Text.Json;
using System.Net.WebSockets;
using System.Threading;

namespace CodingTaskDavidWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<ListModel> listModel { get; set; } = new List<ListModel>();
        public async Task WsLoadData()
        {

            #region connect to ws

            using (ClientWebSocket clientWebSocket = new ClientWebSocket())
            {
                Uri serviceUri = new Uri("ws://localhost:5000/send");
                var cTs = new CancellationTokenSource();
                cTs.CancelAfter(TimeSpan.FromSeconds(120));
                try
                {
                    await clientWebSocket.ConnectAsync(serviceUri, cTs.Token);
                    var n = 0;
                    while (clientWebSocket.State == WebSocketState.Open)
                    {
                        await Task.Delay(1000);

                        ArraySegment<byte> byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes("send me data"));
                        await clientWebSocket.SendAsync(byteToSend, WebSocketMessageType.Text, true, cTs.Token);
                        var respnseBuffer = new byte[1024];
                        var offset = 0;
                        var packet = 1024;
                        var colorGreen = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                        var colorRed = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        var colorWhite = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        
                        while (true)
                        {
                            ArraySegment<byte> bytRecieved = new ArraySegment<byte>(respnseBuffer, offset, packet);
                            WebSocketReceiveResult response = await clientWebSocket.ReceiveAsync(bytRecieved, cTs.Token);
                            var responseMessage = Encoding.UTF8.GetString(respnseBuffer, offset, response.Count);
                            var responseWsList = JsonSerializer.Deserialize<List<ListModel>>(responseMessage);

                            int locationTop = 5; 
                            MainGrid.Children.Clear();
                            foreach (var responseWs in responseWsList)
                            {
                                var selectedColor = colorWhite;
                                var lastStock = listModel.LastOrDefault(x => x.stockName == responseWs.stockName);
                                if (lastStock!=null&& Convert.ToDecimal(lastStock.price) > Convert.ToDecimal(responseWs.price))
                                    selectedColor = colorRed;
                                if (lastStock != null && Convert.ToDecimal(lastStock.price) < Convert.ToDecimal(responseWs.price))
                                    selectedColor = colorGreen;
                                var lblStockName = new Label
                                {
                                    Content = responseWs.stockName,
                                    Tag = responseWs.stockName,
                                    Margin = new Thickness(5, locationTop, 0, 0), 
                                    Background = selectedColor
                                };
                                lblStockName.MouseDoubleClick += Row_DoubleClick;
                                MainGrid.Children.Add(lblStockName);
                                var lblDateTime = new Label
                                {
                                    Content = responseWs.dateTime,
                                    Tag = responseWs.stockName,
                                    Margin = new Thickness(200, locationTop, 0, 0),
                                    Background = selectedColor
                                };
                                lblDateTime.MouseDoubleClick += Row_DoubleClick;
                                MainGrid.Children.Add(lblDateTime);

                                var lblPrice = new Label
                                {
                                    Content = responseWs.price, 
                                    Tag=responseWs.stockName,
                                    Margin = new Thickness(400, locationTop, 0, 0),
                                    Background = selectedColor
                                };
                                lblPrice.MouseDoubleClick += Row_DoubleClick;
                                MainGrid.Children.Add(lblPrice);
                                
                                locationTop += 25;


                                listModel.Add(responseWs);
                            }
                            if (response.EndOfMessage)
                                break;
                        }

                    }
                }
                catch (WebSocketException e)
                {

                }

            }

            #endregion
        }
        public MainWindow()
        {
            InitializeComponent();
            _ = WsLoadData();



        }
        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label senderLbl = sender as Label;
            DGridStockListHis.ItemsSource = listModel.Where(x => x.stockName == senderLbl.Tag.ToString()).OrderByDescending(x=> Convert.ToDateTime(x.dateTime)).ToList();
            // execute some code
        }
    }
}
