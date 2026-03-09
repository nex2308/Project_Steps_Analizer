using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace projekt
{
    public partial class Form1 : Form
    {
        private DataTable dt;
        private DataCalculator dataCalculator;

        public class DataCalculator
        {
            private DataTable dataTable;

            public DataCalculator(DataTable dataTable)
            {
                this.dataTable = dataTable;
            }
            public int GetTotalDistance()
            {
                return (int)dataTable.AsEnumerable()
                    .Where(row => row.Field<object>("Dystans(km)") != DBNull.Value)
                    .Sum(row => row.Field<double>("Dystans(km)"));
            }
            public int GetTotalSteps()
            {
                int totalDistance = GetTotalDistance();
                return totalDistance * 1430;
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dt = new DataTable();
            DataColumn idColumn = new DataColumn("ID", typeof(int));
            idColumn.AutoIncrement = true;
            idColumn.AutoIncrementSeed = 1;
            idColumn.AutoIncrementStep = 1;
            dt.Columns.Add(idColumn);
            dt.Columns.Add(new DataColumn("Data", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("Dystans(km)", typeof(double)));

            Random rnd = new Random();

            for (int i = 0; i < 10; i++)
            {
                dt.Rows.Add(null, DateTime.Today.AddDays(+i), rnd.Next(1, 100));
            }

            dataCalculator = new DataCalculator(dt);

            chart1.Series[0].IsVisibleInLegend = false;
            dataGridView1.DataSource = dt;

            int distance = dataCalculator.GetTotalDistance();
            int steps = dataCalculator.GetTotalSteps();

            label1.Text ="Liczba kroków: "+ steps.ToString();
            label6.Text ="Liczba kilometrów: "+ distance.ToString();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Zamykanie aplikacji.");
            this.Close();
        }

        private void addRowButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Pobranie danych z pól tekstowych
                if (DateTime.TryParse(dateTimePicker1.Text, out DateTime parsedDate) && double.TryParse(textBox2.Text, out double parsedDystans))
                {
                    dt.Rows.Add(null, parsedDate, parsedDystans);
                }
                else
                {
                    MessageBox.Show("Nieprawidłowe dane. Upewnij się, że wprowadzono poprawną datę i dystans.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas dodawania nowego wiersza: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            dataCalculator = new DataCalculator(dt);

            int distance = dataCalculator.GetTotalDistance();
            int steps = dataCalculator.GetTotalSteps();

            label1.Text = "Liczba kroków: " + steps.ToString();
            label6.Text = "Liczba kilometrów: " + distance.ToString();
        }

        private void delete_Click(object sender, EventArgs e)
        {
            // Usuwanie rekordu o podanym numerze ID
            string input = textBox1.Text;

            if (int.TryParse(input, out int idToDelete))
            {
                DataRow rowToDelete = dt.AsEnumerable().FirstOrDefault(row => row.Field<int>("ID") == idToDelete);

                if (rowToDelete != null)
                {
                    dt.Rows.Remove(rowToDelete);
                }
                else
                {
                    MessageBox.Show($"Rekord o ID {idToDelete} nie istnieje.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Podano nieprawidłowy numer ID.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            dataCalculator = new DataCalculator(dt);

            int distance = dataCalculator.GetTotalDistance();
            int steps = dataCalculator.GetTotalSteps();

            label1.Text = "Liczba kroków: " + steps.ToString();
            label6.Text = "Liczba kilometrów: " + distance.ToString();
        }

        private void export_Click(object sender, EventArgs e)
        {
            // Eksport danych do pliku CSV
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Zapisz jako"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    // Nagłówki kolumn
                    var columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                    writer.WriteLine(string.Join(",", columnNames));

                    // Dane w wierszach
                    foreach (DataRow row in dt.Rows)
                    {
                        var fields = row.ItemArray.Select(field => field.ToString());
                        writer.WriteLine(string.Join(",", fields));
                    }
                }

                MessageBox.Show("Dane zostały zapisane do pliku CSV.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            dataCalculator = new DataCalculator(dt);

            int distance = dataCalculator.GetTotalDistance();
            int steps = dataCalculator.GetTotalSteps();

            label1.Text = "Liczba kroków: " + steps.ToString();
            label6.Text = "Liczba kilometrów: " + distance.ToString();
        }

        private void import_Click(object sender, EventArgs e)
        {
            // Import danych z pliku CSV
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Otwórz plik CSV"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                    {
                        string headerLine = reader.ReadLine();
                        string[] headers = headerLine.Split(',');

                        DataTable importTable = new DataTable();
                        foreach (string header in headers)
                        {
                            importTable.Columns.Add(new DataColumn(header));
                        }

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] values = line.Split(',');
                            importTable.Rows.Add(values);
                        }

                        dt.Clear(); // Clear the existing data
                        foreach (DataColumn column in importTable.Columns)
                        {
                            if (!dt.Columns.Contains(column.ColumnName))
                            {
                                dt.Columns.Add(new DataColumn(column.ColumnName));
                            }
                        }

                        foreach (DataRow row in importTable.Rows)
                        {
                            dt.Rows.Add(row.ItemArray);
                        }

                        MessageBox.Show("Dane zostały zaimportowane i zastąpiły istniejące dane.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd podczas importu danych: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                dataCalculator = new DataCalculator(dt);

                int distance = dataCalculator.GetTotalDistance();
                int steps = dataCalculator.GetTotalSteps();

                label1.Text = "Liczba kroków: " + steps.ToString();
                label6.Text = "Liczba kilometrów: " + distance.ToString();
            }
        }

        private void showChartButton_Click(object sender, EventArgs e)
        {
            // Wyświetlenie danych w postaci wykresu słupkowego
            chart1.Series.Clear();
            Series series = new Series("Dystans(km)")
            {
                ChartType = SeriesChartType.Column,
                XValueType = ChartValueType.DateTime
            };

            foreach (DataRow row in dt.Rows)
            {
                if (row["Data"] != DBNull.Value && row["Dystans(km)"] != DBNull.Value)
                {
                    series.Points.AddXY(Convert.ToDateTime(row["Data"]), Convert.ToDouble(row["Dystans(km)"]));
                }
            }

            chart1.Series.Add(series);
            chart1.Series[0].IsVisibleInLegend = false;
            chart1.ChartAreas[0].RecalculateAxesScale();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            dataCalculator = new DataCalculator(dt);

            int distance = dataCalculator.GetTotalDistance();
            int steps = dataCalculator.GetTotalSteps();

            label1.Text = "Liczba kroków: " + steps.ToString();
            label6.Text = "Liczba kilometrów: " + distance.ToString();
        }
    }
}
