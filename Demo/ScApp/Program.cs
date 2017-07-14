using System;
using Starcounter;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ScApp
{
    public class Program
    {
        private const string AppName = "/ScApp";
        private static Random random = new Random();
        public const string GetAllDbEntriesCommand = "SELECT x FROM ScApp.ScAppDb x";

        public static void Main()
        {
            Handle.GET(AppName + "/CreateNewEntry", () =>
            {
                CreateEntry();

                return "One new entry created";
            });

            Handle.GET(AppName + "/CreateNewEntry/{?}", (int count) =>
            {
                if (count <= 0)
                {
                    return $"argument: \"{count}\" needs to be strict greater than 0";
                }

                for (int i = 0; i < count; i++)
                {
                    CreateEntry();
                }

                return $"{count} entries created";
            });

            Handle.GET(AppName + "/DeleteAll", () =>
            {
                QueryResultRows<ScAppDb> entries = Db.SQL<ScAppDb>(GetAllDbEntriesCommand);
                List<ScAppDb> entryList = entries.ToList();
                int count = entryList.Count();

                Db.Transact(() =>
                {
                    foreach (ScAppDb entry in entryList)
                    {
                        entry.Delete();
                    }
                });

                return $"{count} entries deleted";
            });

            Handle.GET(AppName + "/ListAll", () =>
            {
                QueryResultRows<ScAppDb> entries = Db.SQL<ScAppDb>(GetAllDbEntriesCommand);

                string output = $"\"{GetAllDbEntriesCommand}\" entries: " + Environment.NewLine;
                foreach (ScAppDb entry in entries)
                {
                    output += $"Name: {entry.Name}, Integer: {entry.Integer}, DateCreated: {entry.DateCreated}" + Environment.NewLine;
                }

                return output;
            });

        }

        private static void CreateEntry()
        {
            Db.Transact(() =>
            {
                ScAppDb entry = new ScAppDb();
                entry.Name = RandomString(10);
                entry.Integer = random.Next(0, 1000);
                entry.DateCreated = DateTime.Now;
            });
        }

        private static string RandomString(int Size)
        {
            string input = "abcdefghijklmnopqrstuvwxyz";
            Random localRandom = new Random((int)DateTime.Now.Ticks);
            var chars = Enumerable.Range(0, Size)
                           .Select(x => input[localRandom.Next(0, input.Length)]);
            return new string(chars.ToArray());
        }
    }
}