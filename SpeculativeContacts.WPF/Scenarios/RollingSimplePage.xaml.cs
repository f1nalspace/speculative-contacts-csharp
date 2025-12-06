using SpeculativeContacts.Code;
using SpeculativeContacts.Controls;
using SpeculativeContacts.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpeculativeContacts.Scenarios
{
    /// <summary>
    /// Interaction logic for RollingSimplePage.xaml
    /// </summary>
    public partial class RollingSimplePage : BaseScenarioPage
    {
        protected override IEnumerable<RigidBody> Bodies => WPFUtils.FindVisualChildren<RigidBody>(this.LayoutRoot);

        protected override Canvas RootCanvas => SceneCanvas;

        public RollingSimplePage()
        {
            InitializeComponent();
        }
    }
}
