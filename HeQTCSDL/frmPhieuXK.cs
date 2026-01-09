using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HeQTCSDL
{
    public partial class frmPhieuXK : Form
    {
        private DataTable dtChiTiet;
        private DataTable dtAllLoHang;

        public frmPhieuXK()
        {
            InitializeComponent();
            if (!UserSession.LaAdmin && !UserSession.LaXuatKho)
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng Xuất kho!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }
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

        private void frmPhieuXK_Load(object sender, EventArgs e)
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
                    SqlCommand cmdCuaHang = new SqlCommand("SELECT MaCuaHang, TenCuaHang FROM CuaHang", conn);
                    SqlDataAdapter daCuaHang = new SqlDataAdapter(cmdCuaHang);
                    DataTable dtCuaHang = new DataTable();
                    daCuaHang.Fill(dtCuaHang);
                    cboMaCH.DataSource = dtCuaHang;
                    cboMaCH.DisplayMember = "TenCuaHang";
                    cboMaCH.ValueMember = "MaCuaHang";
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

        private void btnLuuPXK_Click(object sender, EventArgs e)
        {
            if (KtraDuLieu())
            {
                try
                {
                    LuuVaoDB();
                    MessageBox.Show("Lưu phiếu xuất kho thành công!");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu phiếu xuất kho: " + ex.Message);
                }
            }
        }

        private bool KtraDuLieu()
        {
            if (string.IsNullOrEmpty(txtSoPXK.Text))
            {
                MessageBox.Show("Vui lòng nhập số PXK!");
                txtSoPXK.Focus();
                return false;
            }

            if (txtSoPXK.Text.Length > 10)
            {
                MessageBox.Show("Số PXK không được quá 10 ký tự!");
                txtSoPXK.Focus();
                return false;
            }

            if (cboMaKho.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn mã kho!");
                cboMaKho.Focus();
                return false;
            }

            if (cboMaCH.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn mã cửa hàng!");
                cboMaCH.Focus();
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

                int soLuongXuat = Convert.ToInt32(row["SoLuong"]);
                if (soLuongXuat <= 0)
                {
                    MessageBox.Show("Số lượng phải lớn hơn 0!");
                    return false;
                }
            }
            if (!KiemTraSoLuongConLai())
            {
                return false;
            }

            return true;
        }
        private bool KiemTraSoLuongConLai()
        {
            try
            {
                string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
                string maKho = cboMaKho.SelectedValue?.ToString();

                if (string.IsNullOrEmpty(maKho))
                {
                    MessageBox.Show("Vui lòng chọn kho!");
                    return false;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var groupedDetails = dtChiTiet.AsEnumerable()
                        .GroupBy(row => row.Field<string>("MaLo"))
                        .Select(group => new
                        {
                            MaLo = group.Key,
                            TongSoLuongXuat = group.Sum(row => row.Field<int>("SoLuong"))
                        });

                    foreach (var detail in groupedDetails)
                    {
                        SqlCommand cmd = new SqlCommand("sp_CheckSoLuongConLaiXuat", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaLo", detail.MaLo);
                        cmd.Parameters.AddWithValue("@MaKho", maKho);

                        object result = cmd.ExecuteScalar();
                        int soLuongConLai = result != null ? Convert.ToInt32(result) : 0;

                        if (detail.TongSoLuongXuat > soLuongConLai)
                        {
                            MessageBox.Show($"Số lượng xuất của lô {detail.MaLo} là {detail.TongSoLuongXuat} " +
                                          $"vượt quá số lượng tồn kho trong kho {maKho} ({soLuongConLai})!");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kiểm tra số lượng tồn kho: " + ex.Message);
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
                    SqlCommand cmdPhieuXK = new SqlCommand("sp_PhieuXK_Them", conn, transaction);
                    cmdPhieuXK.CommandType = CommandType.StoredProcedure;
                    cmdPhieuXK.Parameters.AddWithValue("@SoPXK", txtSoPXK.Text.Trim());
                    cmdPhieuXK.Parameters.AddWithValue("@MaKho", cboMaKho.SelectedValue.ToString());
                    cmdPhieuXK.Parameters.AddWithValue("@MaCuaHang", cboMaCH.SelectedValue.ToString());
                    cmdPhieuXK.Parameters.AddWithValue("@NgayLap", dtpNgayLap.Value.Date);
                    cmdPhieuXK.Parameters.AddWithValue("@MaNV", cboMaNV.SelectedValue.ToString());
                    cmdPhieuXK.ExecuteNonQuery();
                    foreach (DataRow row in dtChiTiet.Rows)
                    {
                        string maLo = row["MaLo"].ToString();
                        int soLuong = Convert.ToInt32(row["SoLuong"]);

                        if (!string.IsNullOrEmpty(maLo))
                        {
                            SqlCommand cmdChiTiet = new SqlCommand("sp_ChiTietXK_Them", conn, transaction);
                            cmdChiTiet.CommandType = CommandType.StoredProcedure;
                            cmdChiTiet.Parameters.AddWithValue("@SoPXK", txtSoPXK.Text.Trim());
                            cmdChiTiet.Parameters.AddWithValue("@MaLo", maLo);
                            cmdChiTiet.Parameters.AddWithValue("@MaCuaHang", cboMaCH.SelectedValue.ToString());
                            cmdChiTiet.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmdChiTiet.ExecuteNonQuery();
                        }
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