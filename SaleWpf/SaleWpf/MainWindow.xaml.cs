using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using System.Net.Http.Formatting;
using static SaleWpf.Models;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Forms.DataVisualization.Charting;

namespace SaleWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Client> Clients = new List<Client>();
        List<Telephone> Telephones = new List<Telephone>();
        List<Sale> Sales = new List<Sale>();
        static HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }
        private async void btnGet_Click(object sender, RoutedEventArgs e)
        {
            string URI = $"https://localhost:7100/api/Sale";
            Dictionary<string, string> parameters = new Dictionary<string, string> { { "param1", tbDateStart.Text }, { "param2", tbDateEnd.Text } };
            FormUrlEncodedContent encodedContent = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await client.PostAsync(URI, encodedContent);
            response.EnsureSuccessStatusCode();
            string json = response.Content.ReadAsStringAsync().Result;
            List<Sale> sales = JsonConvert.DeserializeObject<List<Sale>>(json);
            lv.ItemsSource = sales;
        }
        private void CmbSourceDiagramm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dictionary<int, Action> sourceDiagramTypes = new Dictionary<int, Action>
            {
                [0] = () => CreateLineDiagram(),
                [1] = () => CreateCircleDiagram(),
            };

            ChartArea chartArea = new ChartArea("Sales");

            ChrtSales.Series.Clear();
            ChrtSales.ChartAreas.Clear();

            ChrtSales.ChartAreas.Add(chartArea);

            sourceDiagramTypes[CmbSourceDiagramm.SelectedIndex]?.Invoke();
        }

        private void CreateLineDiagram()
        {
            Series series = new Series()
            {
                IsValueShownAsLabel = true,
            };
            series.ChartType = SeriesChartType.Line;

            foreach (var sale in Sales.GroupBy(s => s.dateSale))
            {
                series.Points.AddXY(sale.Key, sale.Sum(s => s.telephones.Sum(tel => tel.cost * tel.count)));
            }

            ChrtSales.Series.Add(series);
        }

        private void CreateCircleDiagram()
        {
            Series series = new Series()
            {
                IsValueShownAsLabel = true,
            };
            series.ChartType = SeriesChartType.Pie;

            foreach (var sale in Sales.SelectMany(s => s.telephones).GroupBy(s => s.manufacturer))
            {
                series.Points.AddXY(sale.Key, sale.Sum(s => s.cost * s.count));
            }

            ChrtSales.Series.Add(series);
        }
    }
}
