namespace WinFormsAppOdevv
{
    internal static class Program
    {

        [STAThread]
        static void Main()
        {
            try
            {

                ApplicationConfiguration.Initialize();
                DatabaseHelper.InitializeDatabase();

                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        Application.Run(new Form1());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uygulama başlatılırken bir hata oluştu: " + ex.ToString(), "Başlangıç Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
