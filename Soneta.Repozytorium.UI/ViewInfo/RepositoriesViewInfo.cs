using Soneta.Business;

namespace Soneta.Repozytorium.UI
{
    public class RepositoriesViewInfo : ViewInfo
    {
        public RepositoriesViewInfo()
        {
            ResourceName = "Repositories";
            InitContext += RepositoriesViewInfo_InitContext;
            CreateView += RepositoriesViewInfo_CreateView;
        }

        void RepositoriesViewInfo_InitContext(object sender, ContextEventArgs args)
        {
            args.Context.Remove(typeof(WParams));
            args.Context.TryAdd(() => new WParams(args.Context));
        }

        void RepositoriesViewInfo_CreateView(object sender, CreateViewEventArgs args)
        {
            WParams parameters;
            if (!args.Context.Get(out parameters))
                return;
            args.View = ViewCreate(parameters);
        }

        public class WParams : ContextBase
        {
            public WParams(Context context) : base(context) { }
        }

        protected View ViewCreate(WParams pars)
        {
            View view = RepozytoriumModule.GetInstance(pars.Session).Repozytoria.CreateView();
            return view;
        }
    }
}