namespace WinFormsAppOdevv
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            if (passwordTextBox.Text != confirmPasswordTextBox.Text)
            {
                MessageBox.Show("Parolalar uyuşmuyor!");
                return;
            }

            bool success = DatabaseHelper.RegisterUser(usernameTextBox.Text, passwordTextBox.Text);

            if (success)
            {
                MessageBox.Show("Kayıt başarılı!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Bu kullanıcı adı zaten mevcut!");
            }
        }
    }
} 