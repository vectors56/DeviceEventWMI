using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceEventWMI
{
    class CommandLineArgs
    {
        private string[] args;
        private int cursor;

        public CommandLineArgs(string[] args)
        {
            this.args = args;
            this.cursor = 0;
        }

        public int expectSwitch(string[] options)
        {
            int idx = TakeSwitch(options);
            if (idx == -2)
            {
                if (cursor > 0) throw new ApplicationException("Missing argument after " + args[cursor - 1]);
                throw new ApplicationException("No arguments specified.");
            }
            else if (idx == -1)
            {
                throw new ApplicationException("Unexpected argument: " + args[cursor]);
            }
            return idx;
        }

        public int TakeSwitch(string[] options)
        {
            if (cursor == args.Length) return -2;
            string v = args[cursor];
            int idx = new List<string>(options).IndexOf(v);
            if (idx >= 0) cursor++;
            return idx;
        }

        public string ExpectValue()
        {
            string v = TakeValue();
            if (v == null) throw new ApplicationException("Missing argument after " + args[cursor - 1]);
            return v;
        }

        public string TakeValue()
        {
            if (cursor == args.Length) return null;
            return args[cursor++];
        }
    }
    class Options
    {
        public bool list;
        public string _class;
        public string filter;
        public string watch;
        public bool verbose;

        private readonly string[] SWITCHES = new string[] { "-l", "--list", "-f", "--filter", "-c", "--class", "-w", "--watch", "-v", "--verbose" };

        public Options(string[] arguments)
        {
            CommandLineArgs args = new CommandLineArgs(arguments);

            while (true)
            {
                int sw = args.TakeSwitch(SWITCHES);
                if (sw < 0) break;
                switch (sw / 2)
                {
                    case 0:
                        list = true;
                        break;
                    case 1:
                        filter = args.ExpectValue();
                        break;
                    case 2:
                        _class = args.ExpectValue();
                        break;
                    case 3:
                        watch = args.ExpectValue();
                        break;
                    case 4:
                        verbose = true;
                        break;
                }
            }
        }
    }

}
