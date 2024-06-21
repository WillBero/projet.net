using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clientcryptage
{ 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ClearTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string inputText = ClearTextBox.Text;
                if (string.IsNullOrEmpty(inputText))
                {
                    MessageBox.Show("Veuillez entrer une chaîne de caractères.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                inputText = inputText.ToUpper();

                if (IsInputValid(inputText))
                {

                    string prefixedText = "";


                    if (C.IsChecked == true)
                    {
                        prefixedText = "C|" + inputText.ToUpper();

                    }
                    else if (P.IsChecked == true)
                    {
                        prefixedText = "P|" + inputText.ToUpper();


                    }
                    else if (S.IsChecked == true)
                    {
                        prefixedText = "S|" + inputText.ToUpper();

                    }

                    string encryptedText = SendToServer(prefixedText);

                    EncryptedTextBlock.Text = encryptedText;
                }
                else { 
                EncryptedTextBlock.Text = "Erreur dans la saisie du texte";
                }

            }

        }

        private bool IsInputValid(string input)
        {
            // Utilisation de l'expression régulière pour valider la chaîne d'entrée
            Regex regex = new Regex("^[A-Z]+$");
            return regex.IsMatch(input);
        }

        static string SendToServer(string texte)
        {
            byte[] b_texte = new byte[256];
            b_texte = Encoding.ASCII.GetBytes(texte);

            Socket sock;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 4. Connexion Serveur
            sock.Connect("localhost", 6666);

            sock.Send(b_texte);

            byte[] b_encrypte = new byte[256];
            sock.Receive(b_encrypte);
            //6. Cloture de la communication
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
            string encrypte  = Encoding.ASCII.GetString(b_encrypte);
            return encrypte;

    
        }

    }
}
