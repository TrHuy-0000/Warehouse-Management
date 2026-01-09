using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace HeQTCSDL
{
    public partial class frmBaoCao : Form
    {
        public frmBaoCao()
        {
            InitializeComponent();
        }

        private void btnXemBaoCao_Click(object sender, EventArgs e)
        {
            if (rdoTonKho.Checked)
            {
                LoadBaoCaoTonKho();
            }
            else if (rdoNhap.Checked)
            {
                LoadBaoCaoNhap();
            }
            else if (rdoXuat.Checked)
            {
                LoadBaoCaoXuat();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn loại báo cáo!");
            }
        }

        private void LoadBaoCaoTonKho()
        {
            try
            {
                string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("sp_BaoCaoTonKho", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvBaoCao.DataSource = dt;
                    if (dgvBaoCao.Columns.Count > 0)
                    {
                        dgvBaoCao.Columns[0].HeaderText = "Mã Lô";
                        dgvBaoCao.Columns[1].HeaderText = "Mã Hàng";
                        dgvBaoCao.Columns[2].HeaderText = "Mã Kho";
                        dgvBaoCao.Columns[3].HeaderText = "Số Lượng Tồn";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải báo cáo tồn kho: " + ex.Message);
            }
        }

        private void LoadBaoCaoNhap()
        {
            try
            {
                string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            SoPNK,
                            MaLo,
                            SUM(SoLuong) as SoLuong
                        FROM CT_PhieuNhapKho 
                        GROUP BY SoPNK, MaLo
                        ORDER BY SoPNK";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvBaoCao.DataSource = dt;
                    if (dgvBaoCao.Columns.Count > 0)
                    {
                        dgvBaoCao.Columns[0].HeaderText = "Số Phiếu Nhập";
                        dgvBaoCao.Columns[1].HeaderText = "Mã Lô";
                        dgvBaoCao.Columns[2].HeaderText = "Số Lượng";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải báo cáo nhập: " + ex.Message);
            }
        }

        private void LoadBaoCaoXuat()
        {
            try
            {
                string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            SoPXK,
                            MaLo,
                            MaCuaHang,
                            SUM(SoLuong) as SoLuong
                        FROM CT_PhieuXuatKho 
                        GROUP BY SoPXK, MaLo, MaCuaHang
                        ORDER BY SoPXK";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvBaoCao.DataSource = dt;
                    if (dgvBaoCao.Columns.Count > 0)
                    {
                        dgvBaoCao.Columns[0].HeaderText = "Số Phiếu Xuất";
                        dgvBaoCao.Columns[1].HeaderText = "Mã Lô";
                        dgvBaoCao.Columns[2].HeaderText = "Mã Cửa Hàng";
                        dgvBaoCao.Columns[3].HeaderText = "Số Lượng";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải báo cáo xuất: " + ex.Message);
            }
        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (dgvBaoCao.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất file!");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            saveFileDialog.Title = "Xuất báo cáo";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        for (int i = 0; i < dgvBaoCao.Columns.Count; i++)
                        {
                            writer.Write(dgvBaoCao.Columns[i].HeaderText);
                            if (i < dgvBaoCao.Columns.Count - 1)
                                writer.Write(",");
                        }
                        writer.WriteLine();
                        foreach (DataGridViewRow row in dgvBaoCao.Rows)
                        {
                            for (int i = 0; i < dgvBaoCao.Columns.Count; i++)
                            {
                                if (row.Cells[i].Value != null)
                                {
                                    string value = row.Cells[i].Value.ToString();
                                    if (value.Contains(","))
                                        value = "\"" + value + "\"";
                                    writer.Write(value);
                                }
                                if (i < dgvBaoCao.Columns.Count - 1)
                                    writer.Write(",");
                            }
                            writer.WriteLine();
                        }
                    }

                    MessageBox.Show("Xuất file thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message);
                }
            }
        }
    }
}

