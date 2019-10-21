using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace Chat_Server
{
    public class Server
    {
        static TcpListener tcpListener; //сервер для прослушивания
        public List<Client> clients = new List<Client>(); //подключенные пользователи
        public string answers = "Отлично, спасибо, ;Говорят, новый фильм 'Джокер' неплохой...;Текущие дата и время: ;Текущий пользователь: ;";//ответы на вопросы

        protected internal void AddConnection(Client clientObject)
        {
            clients.Add(clientObject); //добавление клиента в случае его подключения
        }
        protected internal void RemoveConnection(string id)
        {
            //получаем по id закрытое подключение
            Client client = clients.FirstOrDefault(c => c.Id == id);
            //и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }
        //прослушивание входящих подключений
        protected internal void Listen()
        {
            //попытка запуска сервера
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Client clientObject = new Client(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        //отправка сообщений конкретному id
        protected internal void TargetMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }
        //широковещательные сообщения
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id)
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }
        //отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop();//остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }

    }
}
