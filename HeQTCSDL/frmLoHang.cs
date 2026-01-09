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
    public partial class frmLoHang : Form
    {
        private string connectionString = "Data Source=LAPTOP-UDIS316D\\SQLEXPRESS;Initial Catalog=QuanLyKho_TienLoi;Integrated Security=True";
        private bool isAdding = false;
        private bool isEditing = false;

        public frmLoHang()
        {
            InitializeComponent();
            LoadComboBoxData();
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
        private void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            foreach (SqlError error in e.Errors)
            {
                MessageBox.Show(error.Message, "Thông báo từ Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoadComboBoxData()
        {
            LoadMaHangComboBox();
            LoadMaNCCComboBox();
        }

        private void LoadMaHangComboBox()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaHang, TenHang FROM HangHoa";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cboMaHang.Items.Clear();
                    cboMaHang.Items.Add(new ComboboxItem { Value = "", Text = "" });

                    while (reader.Read())
                    {
                        cboMaHang.Items.Add(new ComboboxItem
                        {
                            Value = reader["MaHang"].ToString(),
                            Text = reader["MaHang"].ToString() + " - " + reader["TenHang"].ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load danh sách mã hàng: " + ex.Message);
                }
            }
        }

        private void LoadMaNCCComboBox()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MaNCC, TenNCC FROM NhaCungCap";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cboMaNCC.Items.Clear();
                    cboMaNCC.Items.Add(new ComboboxItem { Value = "", Text = "" });

                    while (reader.Read())
                    {
                        cboMaNCC.Items.Add(new ComboboxItem
                        {
                            Value = reader["MaNCC"].ToString(),
                            Text = reader["MaNCC"].ToString() + " - " + reader["TenNCC"].ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load danh sách mã NCC: " + ex.Message);
                }
            }
        }

        private void DisableEdit()
        {
            txtMaLo.Enabled = false;
            cboMaHang.Enabled = false;
            cboMaNCC.Enabled = false;
            txtSoLuong.Enabled = false;
            dtpNgaySX.Enabled = false;
            dtpHanSD.Enabled = false;
            dtpNgayNhapKho.Enabled = false;
        }

        private void EnableEdit()
        {
            txtMaLo.Enabled = true;
            cboMaHang.Enabled = true;
            cboMaNCC.Enabled = true;
            txtSoLuong.Enabled = true;
            dtpNgaySX.Enabled = true;
            dtpHanSD.Enabled = true;
            dtpNgayNhapKho.Enabled = true;
        }

        private void ClearControls()
        {
            txtMaLo.Clear();
            cboMaHang.SelectedIndex = -1;
            cboMaNCC.SelectedIndex = -1;
            txtSoLuong.Clear();
            dtpNgaySX.Value = DateTime.Now;
            dtpHanSD.Value = DateTime.Now.AddMonths(6);
            dtpNgayNhapKho.Value = DateTime.Now;
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT 
                        MaLo, 
                        MaHang, 
                        MaNCC, 
                        SoLuong, 
                        NgaySX, 
                        HanSD, 
                        NgayNhapKho 
                    FROM LoHang";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvLoHang.AutoGenerateColumns = false;
                    dgvLoHang.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
                }
            }
        }

        private void frmLoHang_Load(object sender, EventArgs e)
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
            txtMaLo.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvLoHang.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một lô hàng để xóa.");
                return;
            }

            DataGridViewRow row = dgvLoHang.CurrentRow;
            string maLo = GetSafeStringValue(row, "MaLo");

            if (string.IsNullOrEmpty(maLo))
            {
                MessageBox.Show("Không thể xóa dòng trống.");
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa lô hàng {maLo}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.InfoMessage += Connection_InfoMessage;

                        using (SqlCommand cmd = new SqlCommand("sp_LoHang_Xoa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaLo", maLo);

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Xóa lô hàng thành công.");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Xóa lô hàng thất bại. Có thể lô hàng đang được sử dụng hoặc còn tồn kho.");
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        XacDinhLoi(ex);
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
            if (dgvLoHang.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một lô hàng để sửa.");
                return;
            }

            isAdding = false;
            isEditing = true;
            EnableEdit();
            txtMaLo.Enabled = false;
            DataGridViewRow row = dgvLoHang.CurrentRow;

            txtMaLo.Text = GetSafeStringValue(row, "MaLo");
            txtSoLuong.Text = GetSafeStringValue(row, "SoLuong");
            if (row.Cells["NgaySX"].Value != DBNull.Value)
                dtpNgaySX.Value = Convert.ToDateTime(row.Cells["NgaySX"].Value);
            if (row.Cells["HanSD"].Value != DBNull.Value)
                dtpHanSD.Value = Convert.ToDateTime(row.Cells["HanSD"].Value);
            if (row.Cells["NgayNhapKho"].Value != DBNull.Value)
                dtpNgayNhapKho.Value = Convert.ToDateTime(row.Cells["NgayNhapKho"].Value);
            string maHang = GetSafeStringValue(row, "MaHang");
            cboMaHang.SelectedIndex = -1;
            if (!string.IsNullOrEmpty(maHang))
            {
                for (int i = 0; i < cboMaHang.Items.Count; i++)
                {
                    if (cboMaHang.Items[i] is ComboboxItem item && item.Value == maHang)
                    {
                        cboMaHang.SelectedIndex = i;
                        break;
                    }
                }
            }
            string maNCC = GetSafeStringValue(row, "MaNCC");
            cboMaNCC.SelectedIndex = -1;
            if (!string.IsNullOrEmpty(maNCC))
            {
                for (int i = 0; i < cboMaNCC.Items.Count; i++)
                {
                    if (cboMaNCC.Items[i] is ComboboxItem item && item.Value == maNCC)
                    {
                        cboMaNCC.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaLo.Text) ||
                string.IsNullOrWhiteSpace(txtSoLuong.Text))
            {
                MessageBox.Show("Mã Lô và Số Lượng không được để trống.");
                return;
            }

            if (cboMaHang.SelectedItem == null || cboMaNCC.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Mã Hàng và Nhà Cung Cấp.");
                return;
            }

            if (!int.TryParse(txtSoLuong.Text, out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên dương.");
                return;
            }

            if (dtpHanSD.Value < dtpNgaySX.Value)
            {
                MessageBox.Show("Hạn sử dụng phải lớn hơn hoặc bằng ngày sản xuất.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.InfoMessage += Connection_InfoMessage;

                    conn.Open();
                    string maHang = (cboMaHang.SelectedItem as ComboboxItem)?.Value;
                    string maNCC = (cboMaNCC.SelectedItem as ComboboxItem)?.Value;

                    if (string.IsNullOrEmpty(maHang) || string.IsNullOrEmpty(maNCC))
                    {
                        MessageBox.Show("Vui lòng chọn Mã Hàng và Nhà Cung Cấp hợp lệ.");
                        return;
                    }

                    if (isAdding)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_LoHang_Them", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaLo", txtMaLo.Text.Trim());
                            cmd.Parameters.AddWithValue("@MaHang", maHang);
                            cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmd.Parameters.AddWithValue("@NgaySX", dtpNgaySX.Value);
                            cmd.Parameters.AddWithValue("@HanSD", dtpHanSD.Value);
                            cmd.Parameters.AddWithValue("@NgayNhapKho", dtpNgayNhapKho.Value);
                            cmd.Parameters.AddWithValue("@MaNCC", maNCC);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Thêm lô hàng thành công.");
                            }
                            else
                            {
                                MessageBox.Show("Thêm lô hàng thất bại. Có thể mã lô đã tồn tại.");
                            }
                        }
                    }
                    else if (isEditing)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_LoHang_Sua", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaLo", txtMaLo.Text.Trim());
                            cmd.Parameters.AddWithValue("@MaHang", maHang);
                            cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmd.Parameters.AddWithValue("@NgaySX", dtpNgaySX.Value);
                            cmd.Parameters.AddWithValue("@HanSD", dtpHanSD.Value);
                            cmd.Parameters.AddWithValue("@NgayNhapKho", dtpNgayNhapKho.Value);
                            cmd.Parameters.AddWithValue("@MaNCC", maNCC);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Sửa lô hàng thành công.");
                            }
                            else
                            {
                                MessageBox.Show("Sửa lô hàng thất bại. Có thể lô hàng đã được xếp vào khu vực kho.");
                            }
                        }
                    }

                    LoadData();
                    DisableEdit();
                    isAdding = false;
                    isEditing = false;
                }
                catch (SqlException ex)
                {
                    XacDinhLoi(ex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        private void dgvLoHang_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLoHang.CurrentRow != null && !isAdding && !isEditing)
            {
                DataGridViewRow row = dgvLoHang.CurrentRow;
                txtMaLo.Text = GetSafeStringValue(row, "MaLo");
                txtSoLuong.Text = GetSafeStringValue(row, "SoLuong");
                if (row.Cells["NgaySX"].Value != DBNull.Value)
                    dtpNgaySX.Value = Convert.ToDateTime(row.Cells["NgaySX"].Value);
                if (row.Cells["HanSD"].Value != DBNull.Value)
                    dtpHanSD.Value = Convert.ToDateTime(row.Cells["HanSD"].Value);
                if (row.Cells["NgayNhapKho"].Value != DBNull.Value)
                    dtpNgayNhapKho.Value = Convert.ToDateTime(row.Cells["NgayNhapKho"].Value);
                string maHang = GetSafeStringValue(row, "MaHang");
                cboMaHang.SelectedIndex = -1;
                if (!string.IsNullOrEmpty(maHang))
                {
                    for (int i = 0; i < cboMaHang.Items.Count; i++)
                    {
                        if (cboMaHang.Items[i] is ComboboxItem item && item.Value == maHang)
                        {
                            cboMaHang.SelectedIndex = i;
                            break;
                        }
                    }
                }
                string maNCC = GetSafeStringValue(row, "MaNCC");
                cboMaNCC.SelectedIndex = -1;
                if (!string.IsNullOrEmpty(maNCC))
                {
                    for (int i = 0; i < cboMaNCC.Items.Count; i++)
                    {
                        if (cboMaNCC.Items[i] is ComboboxItem item && item.Value == maNCC)
                        {
                            cboMaNCC.SelectedIndex = i;
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
        private void XacDinhLoi(SqlException ex)
        {
            switch (ex.Number)
            {
                case 547:
                    MessageBox.Show("Không thể thực hiện thao tác vì có dữ liệu liên quan trong các bảng khác.", "Lỗi ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case 2627: 
                case 2601:
                    MessageBox.Show("Mã đã tồn tại trong hệ thống.", "Lỗi trùng mã", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case 515:
                    MessageBox.Show("Không được để trống các trường bắt buộc.", "Lỗi dữ liệu NULL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case 8114:
                    MessageBox.Show("Lỗi chuyển đổi kiểu dữ liệu.", "Lỗi kiểu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                default:
                    MessageBox.Show($"Lỗi SQL: {ex.Message}\nMã lỗi: {ex.Number}", "Lỗi database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }
     
        private void txtSoLuong_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}