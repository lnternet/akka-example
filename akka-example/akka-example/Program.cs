using Akka.Actor;
using System;

namespace akka_example
{
    class Program
    {
        static void Main(string[] args)
        {
            var sys = ActorSystem.Create("akka-actors");
            var props = Props.Create(typeof(MyActor));

            var actor1 = sys.ActorOf(props, "actor1");

            /* Use Tell method to send message to ActorRef. 
             This would send message and continue execution, without expecting a response: */
            var message = new AkkaMessage(MessageType.Simple, "Hello");
            Console.WriteLine($"Program: Telling actor message '{message.MessageText}'...");
            actor1.Tell(message);

            /* Use Ask method do send message to ActorRef and get response. 
             This expects ActorRef to 'Tell' something back. Here thread waits for Task to finish: */
            message = new AkkaMessage(MessageType.Question, "What is your name?");
            Console.WriteLine($"Program: Asking actor and waiting for response: '{message.MessageText}'...");
            var response = actor1.Ask<string>(message);
            response.Wait();
            Console.WriteLine($"Program: Response received from actor: {response.Result}");

            /* Use Inbox to send message to ActorRef and get response.
             This has some advantages over Ask: http://getakka.net/articles/actors/inbox.html */
            var inbox = Inbox.Create(sys);
            inbox.Send(actor1, message);
            try
            {
                var inboxMessage = inbox.Receive(TimeSpan.FromSeconds(10));
                Console.WriteLine($"Program: Response received in inbox: {inboxMessage}");
            }
            catch (TimeoutException) { Console.WriteLine("Program: Inbox wait for response timed out."); }

            Console.ReadLine();
        }
    }

    public class MyActor : UntypedActor
    {
        private readonly string Name = "John Wick";

        protected override void OnReceive(object message)
        {
            if(message is AkkaMessage)
            {
                Console.WriteLine($"    Actor: Received message:   {((AkkaMessage)message).MessageText}");
                if (((AkkaMessage)message).Type == MessageType.Question)
                {
                    Answer();
                }
            }
        }

        private void Answer()
        {
            Console.WriteLine($"    Actor: Answering sender:   {Sender.Path}");
            Sender.Tell(Name);
        }
    }

    class AkkaMessage
    {
        public MessageType Type;

        public string MessageText;

        public AkkaMessage(MessageType type, string messageText)
        {
            Type = type;
            MessageText = messageText;
        }
    }

    public enum MessageType
    {
        Simple,
        Question
    }
}
