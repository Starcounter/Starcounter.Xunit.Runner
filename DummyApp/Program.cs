using System;
using Starcounter;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace DummyApp
{
    internal class Program
    {
        public const string AppName = "/DummyApp";
        public static Random random = new Random();

        public static void Main()
        {
            var app = Application.Current;
            app.Use(new HtmlFromJsonProvider());
            app.Use(new PartialToStandaloneHtmlProvider());

            Handle.GET(AppName + "/createnewentry", () =>
            {
                CreateEntry();

                return "One new entry created";
            });

            Handle.GET(AppName + "/createnewentry/{?}", (int count) =>
            {
                if (count <= 0)
                {
                    return $"argument: \"{count}\" needs to be strict greater than 0";
                }

                CreateEntries(count);

                return $"{count} entries created";
            });

            Handle.GET(AppName + "/deleteall", () =>
            {
                QueryResultRows<DummyAppDb> entries = Db.SQL<DummyAppDb>("SELECT x FROM DummyApp.DummyAppDb x");
                List<DummyAppDb> entryList = entries.ToList();
                int count = entryList.Count();

                Db.Transact(() =>
                {
                    foreach (DummyAppDb entry in entryList)
                    {
                        entry.Delete();
                    }
                });

                return $"{count} entries deleted";
            });

            Handle.GET(AppName + "/listentries", () =>
            {
                ListEntries json = new ListEntries { Data = null };

                if (Session.Current == null)
                {
                    Session.Current = new Session(SessionOptions.PatchVersioning);
                }
                json.Session = Session.Current;

                return json;
            });

        }

        private static void CreateEntries(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateEntry();
            }
        }

        private static void CreateEntry()
        {
            Db.Transact(() =>
            {
                DummyAppDb entry = new DummyAppDb();
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