// Auto-generated from: Exporting Data from DataGridView Control to Excel File
// Source: saving-data-from-datagridview-to-xlsx-file-in-csharp

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Telerik.Documents.Common.Model;
using Telerik.Documents.Media;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Model;
using Telerik.Windows.Documents.Spreadsheet.Model.Printing;

namespace SavingDataFromDatagridviewToXlsx
{
    public partial class Form1 : Form
    {
        private DataGridView _grid;
        private Button _btnExport;

        public Form1()
        {
            InitializeComponent();

            this.Text = "DataGridView → XLSX (RadSpreadProcessing)";
            this.Width = 900;
            this.Height = 500;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            _btnExport = new Button
            {
                Text = "Export to XLSX",
                Dock = DockStyle.Top,
                Height = 40
            };
            _btnExport.Click += OnExportClick;

            this.Controls.Add(_grid);
            this.Controls.Add(_btnExport);

            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var table = new DataTable("Orders");
            table.Columns.Add("OrderID", typeof(int));
            table.Columns.Add("Customer", typeof(string));
            table.Columns.Add("OrderDate", typeof(DateTime));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("UnitPrice", typeof(decimal));
            table.Columns.Add("Discount", typeof(double));

            table.Rows.Add(1001, "Contoso Ltd.", new DateTime(2025, 11, 3), 12, 19.95m, 0.05);
            table.Rows.Add(1002, "Northwind Co.", new DateTime(2025, 11, 7), 5, 49.90m, 0.00);
            table.Rows.Add(1003, "AdventureWorks", new DateTime(2025, 12, 15), 25, 9.99m, 0.10);
            table.Rows.Add(1004, "Blue Yonder", new DateTime(2025, 12, 20), 8, 149.00m, 0.15);

            _grid.DataSource = table;
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            if (_grid.DataSource == null)
            {
                MessageBox.Show("No data to export.");
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "GridExport.xlsx",
                Title = "Save XLSX"
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    ExportGridToXlsx(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (MessageBox.Show("Export complete. Open file?", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open file:\n{ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void ExportGridToXlsx(string filePath)
        {
            var workbook = new Workbook();

            var worksheet = workbook.Worksheets.Add();
            worksheet.Name = "Export";

            var ps = worksheet.WorksheetPageSetup;
            ps.PageOrientation = PageOrientation.Portrait;
            ps.PaperType = PaperTypes.A4;
            ps.Margins = new PageMargins(20, 20, 20, 20);

            int colCount = _grid.Columns.Count;
            int rowCount = _grid.Rows.Count;

            for (int c = 0; c < colCount; c++)
            {
                var headerText = _grid.Columns[c].HeaderText;
                var headerCell = worksheet.Cells[0, c];
                headerCell.SetValue(headerText);

                var headerSel = worksheet.Cells[0, c];
                headerSel.SetIsBold(true);

                PatternFill solidPatternFill = new PatternFill(PatternType.Solid, Color.FromArgb(255, 46, 204, 113), Colors.Transparent);
                headerSel.SetFill(solidPatternFill);
                headerSel.SetVerticalAlignment(RadVerticalAlignment.Center);

                worksheet.Columns[c].SetWidth(new ColumnWidth(100, true));
            }

            for (int r = 0; r < rowCount; r++)
            {
                var gridRow = _grid.Rows[r];
                for (int c = 0; c < colCount; c++)
                {
                    var cell = worksheet.Cells[r + 1, c];
                    object value = gridRow.Cells[c].Value;

                    if (value is null || value == DBNull.Value)
                    {
                        cell.SetValue(string.Empty);
                        continue;
                    }

                    var type = value.GetType();

                    if (type == typeof(int))
                    {
                        cell.SetValue(Convert.ToInt32(value));
                        cell.SetFormat(new CellValueFormat("#,##0"));
                        cell.SetHorizontalAlignment(RadHorizontalAlignment.Right);
                    }
                    else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                    {
                        double d = Convert.ToDouble(value, CultureInfo.InvariantCulture);

                        string header = _grid.Columns[c].HeaderText?.ToLowerInvariant() ?? string.Empty;

                        if (header.Contains("price") || header.Contains("amount"))
                        {
                            cell.SetValue(d);
                            cell.SetFormat(new CellValueFormat("$#,##0.00"));
                        }
                        else if (header.Contains("discount") || header.Contains("percent"))
                        {
                            cell.SetValue(d);
                            cell.SetFormat(new CellValueFormat("0.00%"));
                        }
                        else
                        {
                            cell.SetValue(d);
                            cell.SetFormat(new CellValueFormat("#,##0.00"));
                        }

                        cell.SetHorizontalAlignment(RadHorizontalAlignment.Right);
                    }
                    else if (type == typeof(DateTime))
                    {
                        cell.SetValue((DateTime)value);
                        cell.SetFormat(new CellValueFormat("yyyy-mm-dd"));
                        cell.SetHorizontalAlignment(RadHorizontalAlignment.Center);
                    }
                    else
                    {
                        cell.SetValue(Convert.ToString(value, CultureInfo.CurrentCulture));
                    }
                }
            }

            var used = worksheet.UsedCellRange;
            if (used != null)
            {
                ThemableColor darkBlue = new ThemableColor(Color.FromArgb(255, 44, 62, 80));
                CellBorders darkBlueBorders = new CellBorders(
                                new CellBorder(CellBorderStyle.Medium, darkBlue),
                                new CellBorder(CellBorderStyle.Medium, darkBlue),
                                new CellBorder(CellBorderStyle.Medium, darkBlue),
                                new CellBorder(CellBorderStyle.Medium, darkBlue),
                                new CellBorder(CellBorderStyle.Thin, darkBlue),
                                new CellBorder(CellBorderStyle.Thin, darkBlue),
                                new CellBorder(CellBorderStyle.None, darkBlue),
                                new CellBorder(CellBorderStyle.None, darkBlue));

                worksheet.Cells[used.FromIndex.RowIndex, used.FromIndex.ColumnIndex, used.ToIndex.RowIndex, used.ToIndex.ColumnIndex]
                         .SetBorders(darkBlueBorders);
            }

            worksheet.ViewState.FreezePanes(1, 0);

            var xlsx = new XlsxFormatProvider();
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                xlsx.Export(workbook, fs, TimeSpan.FromSeconds(15));
            }
        }
    }
}
