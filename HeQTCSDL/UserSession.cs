public static class UserSession
{
    public static string TenDangNhap { get; set; }
    public static string VaiTro { get; set; }
    public static string MaNV { get; set; }
    public static string TenNV { get; set; }
    public static bool LaAdmin => VaiTro == "Admin";
    public static bool LaNhapKho => VaiTro == "Nhập kho";
    public static bool LaXuatKho => VaiTro == "Xuất kho";

    public static void Reset()
    {
        TenDangNhap = null;
        VaiTro = null;
        MaNV = null;
        TenNV = null;
    }
}