using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Starcounter;
using ScApp;
using Xunit;

namespace ScTestApp
{
    public class TestSetAddingDatabaseEntries
    {
        [Fact]
        public void TestCase_AddingNewRow_1()
        {
            AddNewRow();
        }

        [Fact]
        public void TestCase_AddingNewRow_2()
        {
            AddNewRow();
        }

        [Fact]
        public void TestCase_AddingNewRow_3()
        {
            AddNewRow();
        }

        private void AddNewRow()
        {
            Scheduling.ScheduleTask(() => {
                int entriesBeforeCount = Db.SQL<ScAppDb>(ScApp.Program.GetAllDbEntriesCommand).Count();

                Db.Transact(() =>
                {
                    ScAppDb entry = new ScAppDb();
                });

                int entriesAfterCount = Db.SQL<ScAppDb>(ScApp.Program.GetAllDbEntriesCommand).Count();

                Assert.True(int.Equals(entriesBeforeCount + 1, entriesAfterCount), $"ScApp.ScAppDb beforeCount={entriesBeforeCount} is not one less than afterCount={entriesAfterCount}");
            }, waitForCompletion: true);
        }
    }
}
