namespace HabitTracker
{
    
    public class FrontEnd
    {
        public FrontEnd()
        {
            Console.WriteLine("Please input your choice");
            char choice = char.Parse(Console.ReadLine());
            switch (choice)
            {
                case 'i':
                    //insert
                    break;
                case 'u':
                    //update
                    break;
                case 'd':
                    break;
            }
            
        }
    
    
    }
    
}

