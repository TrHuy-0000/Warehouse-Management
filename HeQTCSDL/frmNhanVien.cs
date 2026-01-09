using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace HeQTCSDL
{
    public partial class frmNhanVien : Form
    {
        private string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
        private bool isAdding = false;
        private bool isEditing = false;

        public frmNhanVien()
        {
            InitializeComponent();
            LoadVaiTroComboBox();
        }
        private void LoadVaiTroComboBox()
        {
            cboVaiTro.Items.AddRange(new string[] {
                "Quản lý",
                "Nhân viên kho",
                "Nhân viên nhập hàng",
                "Nhân viên xuất hàng",
                "Kế toán"
            });
        }

        private void DisableEdit()
        {
            txtMaNV.Enabled = false;
            txtHoTen.Enabled = false;
            cboVaiTro.Enabled = false;
        }

        private void EnableEdit()
        {
            txtMaNV.Enabled = true;
            txtHoTen.Enabled = true;
            cboVaiTro.Enabled = true;
        }

        private void ClearControls()
        {
            txtMaNV.Clear();
            txtHoTen.Clear();
            cboVaiTro.SelectedIndex = -1;
            cboVaiTro.Text = "";
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaNV, TenNV, VaiTro FROM NhanVien";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvNhanVien.AutoGenerateColumns = false;
                    dgvNhanVien.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
                }
            }
        }

        private void frmNhanVien_Load(object sender, EventArgs e)
        {
            LoadData();
            DisableEdit();
            if (!UserSession.LaAdmin)
            {
                if (btnXoa != null)
                {
                    btnXoa.Enabled = false;
                }
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            isEditing = false;
            EnableEdit();
            ClearControls();
            txtMaNV.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên để xóa.");
                return;
            }

            DataGridViewRow row = dgvNhanVien.CurrentRow;
            string maNV = row.Cells["MaNV"].Value.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên {maNV}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhanVien_Xoa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNV", maNV);

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Xóa nhân viên thành công.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Xóa nhân viên thất bại. Có thể nhân viên đang được sử dụng trong phiếu nhập/xuất kho.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi: " + ex.Message);
                    }
                }
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên để sửa.");
                return;
            }

            isAdding = false;
            isEditing = true;
            EnableEdit();
            txtMaNV.Enabled = false;
            DataGridViewRow row = dgvNhanVien.CurrentRow;
            txtMaNV.Text = GetSafeStringValue(row, "MaNV");
            txtHoTen.Text = GetSafeStringValue(row, "TenNV");
            string vaiTro = GetSafeStringValue(row, "VaiTro");
            if (!string.IsNullOrEmpty(vaiTro))
            {
                int index = cboVaiTro.FindStringExact(vaiTro);
                if (index >= 0)
                    cboVaiTro.SelectedIndex = index;
                else
                    cboVaiTro.Text = vaiTro;
            }
            else
            {
                cboVaiTro.SelectedIndex = -1;
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNV.Text) ||
                string.IsNullOrWhiteSpace(txtHoTen.Text) ||
                string.IsNullOrWhiteSpace(cboVaiTro.Text))
            {
                MessageBox.Show("Mã NV, Họ Tên và Vai Trò không được để trống.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (isAdding)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhanVien_Them", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNV", txtMaNV.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenNV", txtHoTen.Text.Trim());
                            cmd.Parameters.AddWithValue("@VaiTro", cboVaiTro.Text.Trim());

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Thêm nhân viên thành công.");
                            }
                        }
                    }
                    else if (isEditing)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_NhanVien_Sua", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaNV", txtMaNV.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenNV", txtHoTen.Text.Trim());
                            cmd.Parameters.AddWithValue("@VaiTro", cboVaiTro.Text.Trim());

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Sửa nhân viên thành công.");
                            }
                        }
                    }

                    LoadData();
                    DisableEdit();
                    isAdding = false;
                    isEditing = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string maNV = txtTimKiem.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaNV, TenNV, VaiTro FROM NhanVien";

                    if (!string.IsNullOrEmpty(maNV))
                    {
                        query += " WHERE MaNV LIKE @MaNV";
                    }

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    if (!string.IsNullOrEmpty(maNV))
                        da.SelectCommand.Parameters.AddWithValue("@MaNV", "%" + maNV + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvNhanVien.AutoGenerateColumns = false;
                    dgvNhanVien.DataSource = dt;

                    if (dt.Rows.Count == 0 && !string.IsNullOrEmpty(maNV))
                    {
                        MessageBox.Show("Không tìm thấy nhân viên với mã: " + maNV);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
                }
            }
        }

        private void dgvNhanVien_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow != null && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvNhanVien.CurrentRow;
                txtMaNV.Text = GetSafeStringValue(row, "MaNV");
                txtHoTen.Text = GetSafeStringValue(row, "TenNV");
                string vaiTro = GetSafeStringValue(row, "VaiTro");
                if (!string.IsNullOrEmpty(vaiTro))
                {
                    int index = cboVaiTro.FindStringExact(vaiTro);
                    if (index >= 0)
                        cboVaiTro.SelectedIndex = index;
                    else
                        cboVaiTro.Text = vaiTro;
                }
                else
                {
                    cboVaiTro.SelectedIndex = -1;
                }
            }
        }
        private string GetSafeStringValue(DataGridViewRow row, string columnName)
        {
            if (row.Cells[columnName] != null && row.Cells[columnName].Value != null && row.Cells[columnName].Value != DBNull.Value)
                return row.Cells[columnName].Value.ToString();
            else
                return string.Empty;
        }
    }
}