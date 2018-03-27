using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureSensorService
{
    public partial class TemperatureSensorService : ServiceBase
    {
        public TemperatureSensorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            TemperatureLogger logger = new TemperatureLogger();
        }

        protected override void OnStop()
        {
        }
    }
}
