using System.Diagnostics;

namespace NetduinoSerbRemote
{
    public class WiiChuckTest
    {
        private WiiChuck wiiChuck;

        public WiiChuckTest()
        {
            wiiChuck = new WiiChuck(true);
        }

        public void Run()
        {
            while (Debugger.IsAttached)
            {
                // try to read the data from nunchucku
                if (wiiChuck.GetData())
                {
                    WiiChuck.PrintData(wiiChuck);
                }
            }
        }
    }
}