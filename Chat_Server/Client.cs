using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Chat_Server
{
    public class Client
    {
        List<string> answersList = new List<string>(); //Список ответов на вопросы
        protected internal string Id { get; private set; } //id пользователя
        protected internal NetworkStream Stream { get; private set; } //поток
        string userName;

        TcpClient client; //объект TCP-клиента
        Server server; //объект сервера


        public Client(TcpClient tcpClient, Server serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            //Попытка соединения
            try
            {
                Stream = client.GetStream();//поток
                string message = GetMessage();//получение сообщений
                userName = message;//присваивание имени пользователя
                message = String.Format("{0} присоединился!", userName);
                Console.WriteLine(message);
                answersList.AddRange(server.answers.Split(";"));//разделение ответов на вопросы
                server.BroadcastMessage(message, this.Id);
                //Бесконечное получение сообщений
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        //далее идут ответы в зависимости от введенного вопроса. Ввод вопроса не чувствителен к регистру
                        if (message.Equals("Как дела?", StringComparison.CurrentCultureIgnoreCase))
                        {
                            message = answersList[0] + userName + "!";
                            server.TargetMessage(message, this.Id);
                        }
                        if (message.Equals("Что нового в кинотеатрах?", StringComparison.CurrentCultureIgnoreCase))
                        {
                            message = answersList[1];
                            server.TargetMessage(message, this.Id);
                        }
                        if (message.Equals("Какой сегодня день?", StringComparison.CurrentCultureIgnoreCase))
                        {
                            message = answersList[2] + DateTime.Now;
                            server.TargetMessage(message, this.Id);
                        }
                        if (message.Equals("Как меня зовут?", StringComparison.CurrentCultureIgnoreCase))
                        {
                            message = answersList[3] + this.userName.ToString();
                            server.TargetMessage(message, this.Id);
                        }
                        if (message.Equals("Участники", StringComparison.CurrentCultureIgnoreCase))
                        {
                            for (int i = 0; i < server.clients.Count; i++)
                            {
                                message = server.clients[i].userName;
                                server.TargetMessage(message, this.Id);
                            }
                        }
                        if (message.Equals("Пока", StringComparison.CurrentCultureIgnoreCase))
                        {
                            server.RemoveConnection(this.Id);
                            Close();
                        }
                        if ((message.Equals(server.answers.Split(";").ToString(), StringComparison.CurrentCultureIgnoreCase)))
                        {
                            message = String.Format("{0}: {1}", userName, message);
                            server.BroadcastMessage(message, this.Id);
                        }
                    }
                    catch
                    {
                        message = String.Format("{0} покинул чат", userName);
                        Console.WriteLine(message);
                        server.TargetMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                //в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        //чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; //буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        //закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }

    }
}
