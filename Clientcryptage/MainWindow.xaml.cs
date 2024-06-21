using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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
                        prefixedText = "C|" + inputText;
                    }
                    else if (P.IsChecked == true)
                    {
                        prefixedText = "P|" + inputText;
                    }
                    else if (S.IsChecked == true)
                    {
                        prefixedText = "S|" + inputText;
                    }

                    string encryptedText = SendToServer(prefixedText);

                    if (encryptedText == null)
                    {
                        MessageBox.Show("Erreur : Serveur inaccessible.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        EncryptedTextBlock.Text = encryptedText;
                    }
                }
                else
                {
                    MessageBox.Show("Erreur dans la saisie du texte.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        private bool IsInputValid(string input)
        {
            // Utilisation de l'expression régulière pour valider la chaîne d'entrée
            Regex regex = new Regex("^[A-Z]+$");
            return regex.IsMatch(input);
        }

        private string SendToServer(string texte)
        {
            try
            {
                byte[] b_texte = Encoding.ASCII.GetBytes(texte);

                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    // Connexion Serveur
                    sock.Connect("localhost", 6666);

                    sock.Send(b_texte);

                    byte[] b_encrypte = new byte[256];
                    int bytesReceived = sock.Receive(b_encrypte);

                    // Fermeture de la communication
                    sock.Shutdown(SocketShutdown.Both);

                    string encrypte = Encoding.ASCII.GetString(b_encrypte, 0, bytesReceived);
                    return encrypte;
                }
            }
            catch (SocketException)
            {
                // Return null if server is inaccessible
                return null;
            }
        }
    }
}
