using Starcounter;

namespace DummyApp
{
    partial class ListEntries : Json
    {
        protected override void OnData()
        {
            this.Entries = Db.SQL<DummyAppDb>("SELECT x FROM DummyApp.DummyAppDb x");
        }

        [ListEntries_json.Entries]
        partial class ListEntry : Json, IBound<DummyAppDb>
        {
            //public string DateCreated
            //{
            //    get
            //    {
            //        return this.Data.DateCreated.ToString();
            //    }
            //}
        }
}

    
}
