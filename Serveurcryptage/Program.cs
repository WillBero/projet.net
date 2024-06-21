using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main()
    {
        // Création d'un socket IPv4 de type TCP
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            // Création de l'end point pour écouter sur toutes les interfaces et le port 6666
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 6666);

            // Liaison du socket à l'end point
            socket.Bind(endPoint);

            // Ouverture du service en écoute
            socket.Listen(5); // Maximum 5 connexions en file d'attente

            Console.WriteLine("Attente Client");

            // Attente des connexions entrantes
            while (true)
            {
                try
                {
                    // Accepter la connexion entrante
                    Socket clientSocket = socket.Accept();

                    // Créer un thread dédié pour gérer la communication avec ce client
                    ThreadPool.QueueUserWorkItem(Communication, clientSocket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'acceptation de la connexion : {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la liaison du socket : {ex.Message}");
        }
    }

    // Procédure qui traite la connexion du client
    static void Communication(object clientSocketObject)
    {
        Socket clientSocket = (Socket)clientSocketObject;

        try
        {
            // Récupération de l'adresse du client et du port
            IPEndPoint clientEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
            string clientAddress = clientEndPoint.Address.ToString();
            int clientPort = clientEndPoint.Port;

            // Réception des données du client
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);

            // Conversion des données reçues en chaîne ASCII
            string receivedText = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"[{DateTime.Now}] Requête de {clientAddress}:{clientPort} - Chaîne reçue : {receivedText}");

            // Récupération du texte crypté
            string response = ProcessRequest(receivedText);
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);

            Console.WriteLine(response);

            // Envoi de la réponse au client
            clientSocket.Send(responseBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la communication avec le client : {ex.Message}");
        }
        finally
        {
            // Fermeture propre de la connexion avec le client
            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            clientSocket.Close();
        }
    }

    // Procédure qui traite la chaîne de caractères reçue pour définir le type de cryptage
    static string ProcessRequest(string request)
    {
        try
        {
            // Séparation de la chaîne en utilisant '|' comme délimiteur
            string[] parts = request.Split('|');
            if (parts.Length != 2)
            {
                return "Invalid input format";
            }

            // Le premier caractère indique le type de chiffrement, le second est le texte à chiffrer
            char method = parts[0][0];
            string textToEncrypt = parts[1];

            switch (method)
            {
                case 'C':
                    return Cesar(textToEncrypt); // Chiffrement de César
                case 'P':
                    return Playfair(textToEncrypt); // Chiffrement de Playfair
                case 'S':
                    return Substitution(textToEncrypt); // Chiffrement par Substitution
                default:
                    return "Unknown encryption method"; // Méthode inconnue
            }
        }
        catch (Exception ex)
        {
            return $"Erreur lors du traitement de la requête : {ex.Message}";
        }
    }

    // Méthode de chiffrement de César
    static string Cesar(string input)
    {
        try
        {
            int shift = 3; // Décalage exemple
            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsLetter(c))
                {
                    char offset = char.IsUpper(c) ? 'A' : 'a';
                    result.Append((char)(((c + shift - offset) % 26) + offset));
                }
                else
                {
                    result.Append(c); // Conserver les caractères non alphabétiques
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Erreur lors du chiffrement Cesar : {ex.Message}";
        }
    }

    // Méthode de chiffrement de Playfair
    static string Playfair(string input)
    {
        try
        {
            // Préparation du texte (remplacement de 'W' et ajout de 'X' si longueur impaire)
            input = PrepareText(input);
            StringBuilder encryptedText = new StringBuilder();

            for (int i = 0; i < input.Length; i += 2)
            {
                char firstLetter = input[i];
                char secondLetter = input[i + 1];

                (int firstRow, int firstCol) = FindPosition(playfairMatrix, firstLetter);
                (int secondRow, int secondCol) = FindPosition(playfairMatrix, secondLetter);

                if (firstRow == secondRow)
                {
                    // Même ligne, prendre les lettres à droite avec wrap-around
                    encryptedText.Append(playfairMatrix[firstRow, (firstCol + 1) % 5]);
                    encryptedText.Append(playfairMatrix[secondRow, (secondCol + 1) % 5]);
                }
                else if (firstCol == secondCol)
                {
                    // Même colonne, prendre les lettres en dessous avec wrap-around
                    encryptedText.Append(playfairMatrix[(firstRow + 1) % 5, firstCol]);
                    encryptedText.Append(playfairMatrix[(secondRow + 1) % 5, secondCol]);
                }
                else
                {
                    // Rectangle, échanger les coins opposés du rectangle
                    encryptedText.Append(playfairMatrix[secondRow, firstCol]);
                    encryptedText.Append(playfairMatrix[firstRow, secondCol]);
                }
            }

            return encryptedText.ToString();
        }
        catch (Exception ex)
        {
            return $"Erreur lors du chiffrement Playfair : {ex.Message}";
        }
    }

    // Matrice utilisée pour le chiffrement de Playfair
    static char[,] playfairMatrix = {
        { 'B', 'Y', 'D', 'G', 'Z' },
        { 'J', 'S', 'F', 'U', 'P' },
        { 'L', 'A', 'R', 'K', 'X' },
        { 'C', 'O', 'I', 'V', 'E' },
        { 'Q', 'N', 'M', 'H', 'T' }
    };

    // Préparation du texte pour le chiffrement de Playfair
    static string PrepareText(string input)
    {
        try
        {
            // Remplacer les 'W' par des 'X'
            input = input.Replace('W', 'X');

            // Ajouter un 'X' si la longueur est impaire
            if (input.Length % 2 != 0)
            {
                input += 'X';
            }

            return input;
        }
        catch (Exception ex)
        {
            return $"Erreur lors de la préparation du texte : {ex.Message}";
        }
    }

    // Trouver la position d'une lettre dans la matrice Playfair
    static (int, int) FindPosition(char[,] matrix, char letter)
    {
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                if (matrix[row, col] == letter)
                {
                    return (row, col);
                }
            }
        }
        throw new ArgumentException("Lettre non trouvée dans la matrice Playfair");
    }

    // Méthode de chiffrement par Substitution
    static string Substitution(string input)
    {
        try
        {
            // Clé de substitution
            char[] clearLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] cipheredLetters = "HIJKLMNVWXYZBCADEFGOPQRSTU".ToCharArray();

            StringBuilder encryptedText = new StringBuilder();

            foreach (char c in input)
            {
                int index = Array.IndexOf(clearLetters, c);
                if (index != -1)
                {
                    encryptedText.Append(cipheredLetters[index]);
                }
                else
                {
                    encryptedText.Append(c); // Conserver les caractères non alphabétiques tels quels
                }
            }

            return encryptedText.ToString();
        }
        catch (Exception ex)
        {
            return $"Erreur lors du chiffrement Substitution : {ex.Message}";
        }
    }
}
