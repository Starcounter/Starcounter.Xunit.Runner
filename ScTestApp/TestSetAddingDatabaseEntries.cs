using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Starcounter;
using DummyApp;
using Xunit;

namespace ScTestApp
{
    public class TestSetAddingDatabaseEntries
    {
        [Fact]
        public void TestCase_AddingNewRow_1()
        {
            Scheduling.ScheduleTask(() => {
                int entriesBeforeCount = Db.SQL<DummyAppDb>("SELECT x FROM DummyApp.DummyAppDb x").Count();

                Db.Transact(() =>
                {
                    DummyAppDb entry = new DummyAppDb();
                });

                int entriesAfterCount = Db.SQL<DummyAppDb>("SELECT x FROM DummyApp.DummyAppDb x").Count();

                Assert.True(int.Equals(entriesBeforeCount + 1, entriesAfterCount), $"DummyApp.DummyAppDb beforeCount={entriesBeforeCount} is not one less than afterCount={entriesAfterCount}");
            }, waitForCompletion: true);
        }

        [Fact]
        public void TestCase_AddingNewRow_2()
        {
            Scheduling.ScheduleTask(() => {
                int entriesBeforeCount = Db.SQL<DummyAppDb>("SELECT x FROM DummyApp.DummyAppDb x").Count();

                Db.Transact(() =>
                {
                    DummyAppDb entry = new DummyAppDb();
                });

                int entriesAfterCount = Db.SQL<DummyAppDb>("SELECT x FROM DummyApp.DummyAppDb x").Count();

                Assert.True(int.Equals(entriesBeforeCount + 1, entriesAfterCount), $"DummyApp.DummyAppDb beforeCount={entriesBeforeCount} is not one less than afterCount={entriesAfterCount}");
            }, waitForCompletion: true);
        }

        [Fact]
        public void TestCase_AddingNewRow_3()
        {
            Scheduling.ScheduleTask(() => {
                int entriesBeforeCount = Db.SQL<DummyAppDb>("SELECT x FROM DummyApp.DummyAppDb x").Count();

                Db.Transact(() =>
                {
                    DummyAppDb entry = new DummyAppDb();
                });

                int entriesAfterCount = Db.SQL<DummyAppDb>("SELECT x FROM DummyApp.DummyAppDb x").Count();

                Assert.True(int.Equals(entriesBeforeCount + 1, entriesAfterCount), $"DummyApp.DummyAppDb beforeCount={entriesBeforeCount} is not one less than afterCount={entriesAfterCount}");
            }, waitForCompletion: true);
        }
    }
}
