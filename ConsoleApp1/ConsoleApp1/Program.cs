using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<INotifier> notifiers = new List<INotifier>
            {
                new EmailNotifier(),
                new SmsNotifier(),
            };

            foreach (var notifier in notifiers)
            {
                notifier.Send("Интерфейсы — это круто!");
            }
        }
    }

    interface INotifier
    {
        void Send(string message);
    }

    public class EmailNotifier : INotifier
    {
        public void Send(string message)
        {
            Console.WriteLine($"Отправлено по Email: {message}");
        }
    }

    public class SmsNotifier : INotifier
    {
        public void Send(string message)
        {
            Console.WriteLine($"Отправлено по SMS: {message}");
        }
    }
}