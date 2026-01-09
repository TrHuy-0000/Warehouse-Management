using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HeQTCSDL
{
    public partial class frmPhieuNK : Form
    {
        private DataTable dtChiTiet;
        private DataTable dtAllLoHang;

        public frmPhieuNK()
        {
            InitializeComponent();
            KhoiTaoDT();
        }

        private void KhoiTaoDT()
        {
            dgvChiTiet.AutoGenerateColumns = false;
            dgvChiTiet.Enabled = false;

            dtChiTiet = new DataTable();
            dtChiTiet.Columns.Add("MaLo", typeof(string));
            dtChiTiet.Columns.Add("SoLuong", typeof(int));

            dgvChiTiet.DataSource = dtChiTiet;
        }

        private void frmPhieuNK_Load(object sender, EventArgs e)
        {
            LoadComboBoxData();
            LoadLoHangData();
            dtpNgayLap.Value = DateTime.Now;
            dgvChiTiet.Enabled = false;
        }

        private void LoadComboBoxData()
        {
            try
            {
                string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmdKho = new SqlCommand("SELECT MaKho, TenKho FROM Kho", conn);
                    SqlDataAdapter daKho = new SqlDataAdapter(cmdKho);
                    DataTable dtKho = new DataTable();
                    daKho.Fill(dtKho);

                    cboMaKho.DataSource = dtKho;
                    cboMaKho.DisplayMember = "TenKho";
                    cboMaKho.ValueMember = "MaKho";

                    SqlCommand cmdNV = new SqlCommand("SELECT MaNV, TenNV FROM NhanVien", conn);
                    SqlDataAdapter daNV = new SqlDataAdapter(cmdNV);
                    DataTable dtNV = new DataTable();
                    daNV.Fill(dtNV);

                    cboMaNV.DataSource = dtNV;
                    cboMaNV.DisplayMember = "TenNV";
                    cboMaNV.ValueMember = "MaNV";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }

        private void LoadLoHangData()
        {
            try
            {
                string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmdLoHang = new SqlCommand("SELECT MaLo FROM LoHang", conn);
                    SqlDataAdapter daLoHang = new SqlDataAdapter(cmdLoHang);
                    dtAllLoHang = new DataTable();
                    daLoHang.Fill(dtAllLoHang);

                    if (dgvChiTiet.Columns["MaLo"] is DataGridViewComboBoxColumn colMaLo)
                    {
                        colMaLo.DataSource = dtAllLoHang.Copy();
                        colMaLo.DisplayMember = "MaLo";
                        colMaLo.ValueMember = "MaLo";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách lô hàng: " + ex.Message);
            }
        }

        private void btnThemLo_Click(object sender, EventArgs e)
        {
            dgvChiTiet.Enabled = true;
            DataRow newRow = dtChiTiet.NewRow();
            newRow["SoLuong"] = 1;
            dtChiTiet.Rows.Add(newRow);
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvChiTiet.CurrentRow != null && dgvChiTiet.CurrentRow.Index >= 0)
            {
                int rowIndex = dgvChiTiet.CurrentRow.Index;
                if (MessageBox.Show("Bạn có chắc muốn xóa lô hàng này?", "Xác nhận xóa",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dtChiTiet.Rows.RemoveAt(rowIndex);
                    if (dtChiTiet.Rows.Count == 0)
                    {
                        dgvChiTiet.Enabled = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một dòng để xóa!");
            }
        }

        private void btnLuuPNK_Click(object sender, EventArgs e)
        {
            if (KtraDuLieu())
            {
                try
                {
                    LuuVaoDB();
                    MessageBox.Show("Lưu phiếu nhập kho thành công!");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu phiếu nhập kho: " + ex.Message);
                }
            }
        }

        private bool KtraDuLieu()
        {
            if (string.IsNullOrEmpty(txtSoPNK.Text))
            {
                MessageBox.Show("Vui lòng nhập số PNK!");
                txtSoPNK.Focus();
                return false;
            }

            if (txtSoPNK.Text.Length > 10)
            {
                MessageBox.Show("Số PNK không được quá 10 ký tự!");
                txtSoPNK.Focus();
                return false;
            }

            if (cboMaKho.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn mã kho!");
                cboMaKho.Focus();
                return false;
            }

            if (dtChiTiet.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một lô hàng!");
                return false;
            }
            foreach (DataRow row in dtChiTiet.Rows)
            {
                if (string.IsNullOrEmpty(row["MaLo"].ToString()))
                {
                    MessageBox.Show("Vui lòng chọn mã lô cho tất cả các dòng!");
                    return false;
                }

                int soLuongNhap = Convert.ToInt32(row["SoLuong"]);
                if (soLuongNhap <= 0)
                {
                    MessageBox.Show("Số lượng phải lớn hơn 0!");
                    return false;
                }
            }
            if (!KiemTraSoLuongLoHang())
            {
                return false;
            }

            return true;
        }

        private bool KiemTraSoLuongLoHang()
        {
            try
            {
                string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var groupedDetails = dtChiTiet.AsEnumerable()
                        .GroupBy(row => row.Field<string>("MaLo"))
                        .Select(group => new
                        {
                            MaLo = group.Key,
                            TongSoLuongNhap = group.Sum(row => row.Field<int>("SoLuong"))
                        });

                    foreach (var detail in groupedDetails)
                    {
                        SqlCommand cmd = new SqlCommand("SELECT SoLuong FROM LoHang WHERE MaLo = @MaLo", conn);
                        cmd.Parameters.AddWithValue("@MaLo", detail.MaLo);
                        object result = cmd.ExecuteScalar();
                        int soLuongHienCo = result != null ? Convert.ToInt32(result) : 0;

                        if (detail.TongSoLuongNhap > soLuongHienCo)
                        {
                            MessageBox.Show($"Số lượng nhập của lô {detail.MaLo} là {detail.TongSoLuongNhap} " +
                                          $"vượt quá số lượng hiện có của lô ({soLuongHienCo})!");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kiểm tra số lượng: " + ex.Message);
                return false;
            }
        }

        private void LuuVaoDB()
        {
            string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    SqlCommand cmdPhieuNK = new SqlCommand("sp_PhieuNK_Them", conn, transaction);
                    cmdPhieuNK.CommandType = CommandType.StoredProcedure;
                    cmdPhieuNK.Parameters.AddWithValue("@SoPNK", txtSoPNK.Text.Trim());
                    cmdPhieuNK.Parameters.AddWithValue("@MaKho", cboMaKho.SelectedValue.ToString());
                    cmdPhieuNK.Parameters.AddWithValue("@NgayLap", dtpNgayLap.Value.Date);
                    cmdPhieuNK.Parameters.AddWithValue("@MaNV", cboMaNV.SelectedValue.ToString());
                    cmdPhieuNK.Parameters.AddWithValue("@GhiChu", txtGhiChu.Text);
                    cmdPhieuNK.ExecuteNonQuery();
                    var groupedDetails = dtChiTiet.AsEnumerable()
                        .GroupBy(row => row.Field<string>("MaLo"))
                        .Select(group => new
                        {
                            MaLo = group.Key,
                            TongSoLuong = group.Sum(row => row.Field<int>("SoLuong"))
                        });

                    foreach (var detail in groupedDetails)
                    {
                        SqlCommand cmdChiTiet = new SqlCommand("sp_ChiTietNK_Them", conn, transaction);
                        cmdChiTiet.CommandType = CommandType.StoredProcedure;
                        cmdChiTiet.Parameters.AddWithValue("@SoPNK", txtSoPNK.Text.Trim());
                        cmdChiTiet.Parameters.AddWithValue("@MaLo", detail.MaLo);
                        cmdChiTiet.Parameters.AddWithValue("@SoLuong", detail.TongSoLuong);
                        cmdChiTiet.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message);
                    throw;
                }
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn hủy?", "Xác nhận",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}