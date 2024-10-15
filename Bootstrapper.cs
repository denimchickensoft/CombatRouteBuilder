
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using CRB.ViewModels;

namespace CRB
{
    class Bootstrapper : BootstrapperBase
    {

        private SimpleContainer container;

        public Bootstrapper()
        {
            Initialize();
            LogManager.GetLog = type => new DebugLog(type);
        }

  
        protected override void Configure()
        {
            container = new SimpleContainer();
            container.Instance(container);
            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<ShellViewModel>();

        }
        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetExecutingAssembly() };
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync<ShellViewModel>();
        }


    }
}
