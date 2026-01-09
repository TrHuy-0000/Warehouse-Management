using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeQTCSDL
{
    public partial class frmHangHoa : Form
    {
        private string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
        private bool isAdding = false;
        private bool isEditing = false;

        public frmHangHoa()
        {
            InitializeComponent();
            LoadMaNhomComboBox();
        }
        private class ComboboxItem
        {
            public string Value { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
        private void LoadMaNhomComboBox()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaNhom, TenNhom FROM NhomNhietDo";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    cboMaNhom.Items.Clear();
                    cboMaNhom.Items.Add(new ComboboxItem { Value = "", Text = "" });

                    while (reader.Read())
                    {
                        cboMaNhom.Items.Add(new ComboboxItem
                        {
                            Value = reader["MaNhom"].ToString(),
                            Text = reader["MaNhom"].ToString() + " - " + reader["TenNhom"].ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load danh sách mã nhóm: " + ex.Message);
                }
            }
        }

        private void DisableEdit()
        {
            txtMaHang.Enabled = false;
            txtTenHang.Enabled = false;
            txtDonVi.Enabled = false;
            txtLoai.Enabled = false;
            cboMaNhom.Enabled = false;
            txtBaoQuan.Enabled = false;
            txtMoTa.Enabled = false;
        }

        private void EnableEdit()
        {
            txtMaHang.Enabled = true;
            txtTenHang.Enabled = true;
            txtDonVi.Enabled = true;
            txtLoai.Enabled = true;
            cboMaNhom.Enabled = true;
            txtBaoQuan.Enabled = true;
            txtMoTa.Enabled = true;
        }

        private void ClearControls()
        {
            txtMaHang.Clear();
            txtTenHang.Clear();
            txtDonVi.Clear();
            txtLoai.Clear();
            cboMaNhom.SelectedIndex = -1;
            txtBaoQuan.Clear();
            txtMoTa.Clear();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT 
                        MaHang, 
                        TenHang, 
                        DonVi, 
                        Loai, 
                        MaNhom, 
                        CachBaoQuan, 
                        MoTa 
                    FROM HangHoa";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvHangHoa.AutoGenerateColumns = false;
                    dgvHangHoa.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
                }
            }
        }

        private void frmHangHoa_Load(object sender, EventArgs e)
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
            txtMaHang.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvHangHoa.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một hàng hóa để xóa.");
                return;
            }

            DataGridViewRow row = dgvHangHoa.CurrentRow;
            string maHang = GetSafeStringValue(row, "MaHang");

            if (string.IsNullOrEmpty(maHang))
            {
                MessageBox.Show("Không thể xóa dòng trống.");
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa hàng hóa {maHang}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_HangHoa_Xoa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaHang", maHang);

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Xóa hàng hóa thành công.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Xóa hàng hóa thất bại. Có thể hàng hóa đang được sử dụng trong lô hàng.");
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
            if (dgvHangHoa.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một hàng hóa để sửa.");
                return;
            }
            isAdding = false;
            isEditing = true;
            EnableEdit();
            txtMaHang.Enabled = false;
            DataGridViewRow row = dgvHangHoa.CurrentRow;

            txtMaHang.Text = GetSafeStringValue(row, "MaHang");
            txtTenHang.Text = GetSafeStringValue(row, "TenHang");
            txtDonVi.Text = GetSafeStringValue(row, "DonVi");
            txtLoai.Text = GetSafeStringValue(row, "Loai");
            txtBaoQuan.Text = GetSafeStringValue(row, "CachBaoQuan");
            txtMoTa.Text = GetSafeStringValue(row, "MoTa");
            string maNhom = GetSafeStringValue(row, "MaNhom");
            cboMaNhom.SelectedIndex = -1;

            if (!string.IsNullOrEmpty(maNhom))
            {
                for (int i = 0; i < cboMaNhom.Items.Count; i++)
                {
                    if (cboMaNhom.Items[i] is ComboboxItem item && item.Value == maNhom)
                    {
                        cboMaNhom.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaHang.Text) ||
                string.IsNullOrWhiteSpace(txtTenHang.Text) ||
                string.IsNullOrWhiteSpace(txtDonVi.Text))
            {
                MessageBox.Show("Mã Hàng, Tên Hàng và Đơn vị không được để trống.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string maNhom = "";
                    if (cboMaNhom.SelectedItem is ComboboxItem selectedItem)
                    {
                        maNhom = selectedItem.Value;
                    }

                    if (isAdding)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_HangHoa_Them", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaHang", txtMaHang.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenHang", txtTenHang.Text.Trim());
                            cmd.Parameters.AddWithValue("@Loai", string.IsNullOrWhiteSpace(txtLoai.Text) ? (object)DBNull.Value : txtLoai.Text.Trim());
                            cmd.Parameters.AddWithValue("@DonVi", txtDonVi.Text.Trim());
                            cmd.Parameters.AddWithValue("@MoTa", string.IsNullOrWhiteSpace(txtMoTa.Text) ? (object)DBNull.Value : txtMoTa.Text.Trim());
                            cmd.Parameters.AddWithValue("@CachBaoQuan", string.IsNullOrWhiteSpace(txtBaoQuan.Text) ? (object)DBNull.Value : txtBaoQuan.Text.Trim());
                            cmd.Parameters.AddWithValue("@MaNhom", string.IsNullOrEmpty(maNhom) ? (object)DBNull.Value : maNhom);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Thêm hàng hóa thành công.");
                            }
                        }
                    }
                    else if (isEditing)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_HangHoa_Sua", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaHang", txtMaHang.Text.Trim());
                            cmd.Parameters.AddWithValue("@TenHang", txtTenHang.Text.Trim());
                            cmd.Parameters.AddWithValue("@Loai", string.IsNullOrWhiteSpace(txtLoai.Text) ? (object)DBNull.Value : txtLoai.Text.Trim());
                            cmd.Parameters.AddWithValue("@DonVi", txtDonVi.Text.Trim());
                            cmd.Parameters.AddWithValue("@MoTa", string.IsNullOrWhiteSpace(txtMoTa.Text) ? (object)DBNull.Value : txtMoTa.Text.Trim());
                            cmd.Parameters.AddWithValue("@CachBaoQuan", string.IsNullOrWhiteSpace(txtBaoQuan.Text) ? (object)DBNull.Value : txtBaoQuan.Text.Trim());
                            cmd.Parameters.AddWithValue("@MaNhom", string.IsNullOrEmpty(maNhom) ? (object)DBNull.Value : maNhom);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Sửa hàng hóa thành công.");
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
            string maHang = txtTimKiem.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT 
                        MaHang, 
                        TenHang, 
                        DonVi, 
                        Loai, 
                        MaNhom, 
                        CachBaoQuan, 
                        MoTa 
                    FROM HangHoa";

                    if (!string.IsNullOrEmpty(maHang))
                    {
                        query += " WHERE MaHang LIKE @MaHang";
                    }

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    if (!string.IsNullOrEmpty(maHang))
                        da.SelectCommand.Parameters.AddWithValue("@MaHang", "%" + maHang + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvHangHoa.AutoGenerateColumns = false;
                    dgvHangHoa.DataSource = dt;

                    if (dt.Rows.Count == 0 && !string.IsNullOrEmpty(maHang))
                    {
                        MessageBox.Show("Không tìm thấy hàng hóa với mã: " + maHang);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
                }
            }
        }

        private void dgvHangHoa_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHangHoa.CurrentRow != null && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvHangHoa.CurrentRow;
                txtMaHang.Text = GetSafeStringValue(row, "MaHang");
                txtTenHang.Text = GetSafeStringValue(row, "TenHang");
                txtDonVi.Text = GetSafeStringValue(row, "DonVi");
                txtLoai.Text = GetSafeStringValue(row, "Loai");
                txtBaoQuan.Text = GetSafeStringValue(row, "CachBaoQuan");
                txtMoTa.Text = GetSafeStringValue(row, "MoTa");
                string maNhom = GetSafeStringValue(row, "MaNhom");
                cboMaNhom.SelectedIndex = -1;

                if (!string.IsNullOrEmpty(maNhom))
                {
                    for (int i = 0; i < cboMaNhom.Items.Count; i++)
                    {
                        if (cboMaNhom.Items[i] is ComboboxItem item && item.Value == maNhom)
                        {
                            cboMaNhom.SelectedIndex = i;
                            break;
                        }
                    }
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
