using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Chat_Client
{
    class Connector
    {
        public static List<string> questionsList = new List<string>();//список вопросов
        static string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            Console.Write("Введите имя пользователя: ");
            userName = Console.ReadLine();
            client = new TcpClient();
            //попытка соединения
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); //получаем поток

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                //запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));//объявление потока
                receiveThread.Start(); //старт потока
                Console.WriteLine("Пользователь {0} присоединился!", userName);

                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        //отправка сообщений
        static void SendMessage()
        {
            Console.WriteLine("{0}, смотри, здесь вопросы, на которые я могу ответить:", userName);
            string questions = "1. Как дела?;2. Что нового в кинотеатрах?;3. Какой сегодня день?;4. Как меня зовут?;5. Чтобы вывести всех пользователей, присутствующих в чате введите 'Участники'";//список вопросов
            questionsList.AddRange(questions.Split(";"));
            for (int i = 0; i < questionsList.Count; i++)
            {
                Console.WriteLine(questionsList[i]);
            }

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }
        //получение сообщений
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; //буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);//вывод сообщения
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }
    }
}

