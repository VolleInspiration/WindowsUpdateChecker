using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsUpdateChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            long timer, start, end;
            bool sendEMail = true;

            if (args[0] == "mail=false") 
                sendEMail = false; 
            else if (args[0] == "mail=true") 
                sendEMail = true;

            Console.WriteLine("@ECHO OFF");
            checkForUpdate upd = new checkForUpdate();
            
            DateTime d = DateTime.Now;

            while (true)
            {
                Console.WriteLine("startingtime: " + d.TimeOfDay);
                
                start = d.Second;
                upd.isNewUpdate(sendEMail);
                
                end = d.Second;
                timer = end - start;

                for (int i = 0; i < 86400 - TimeSpan.FromTicks(timer).TotalSeconds; i++)
                {
                    Console.Write("\r>>> Sleeping {0}/{1} seconds...", i, (86400 - TimeSpan.FromTicks(timer).TotalSeconds));
                    System.Threading.Thread.Sleep(1000);
                }
                
            }
        }
    }
}
