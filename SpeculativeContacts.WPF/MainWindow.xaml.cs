using SpeculativeContacts.Code;
using SpeculativeContacts.Engine;
using SpeculativeContacts.Scenarios;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpeculativeContacts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BaseScenarioPage activeScenarioPage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadScenario(ScenarioModel scenario)
        {
            if (FrameContainer is null)
                return;

            ArgumentNullException.ThrowIfNull(scenario);

            Uri source = new Uri(scenario.Source, UriKind.Relative);

            Page previousPage = FrameContainer.Content as Page;
            if (previousPage is IDisposable disposable)
                disposable.Dispose();

            Page newPage = (Page)Application.LoadComponent(source);
            FrameContainer.Content = newPage;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScenarioModel scenario = ScenarioCombobox.Items[ScenarioCombobox.SelectedIndex] as ScenarioModel;
            LoadScenario(scenario);
        }

        private void ScenarioCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScenarioModel scenario = e.AddedItems[0] as ScenarioModel;
            LoadScenario(scenario);
        }

        public void OnClickRestart(object sender, System.Windows.RoutedEventArgs e)
        {
            if (activeScenarioPage is null)
                return;

            activeScenarioPage.Restart();
        }

        private void OnStepClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (activeScenarioPage is null)
                return;

            if (!activeScenarioPage.IsStepping || !activeScenarioPage.IsPaused)
            {
                activeScenarioPage.Pause();
                activeScenarioPage.EnableStepping();
                stepButton.Content = "Next";
                pauseContinueButton.Content = "Continue";
            }
            else
                activeScenarioPage.NextStep();
        }

        private void OnPauseContinueClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (activeScenarioPage is null)
                return;

            if (!activeScenarioPage.IsPaused)
            {
                activeScenarioPage.Pause();
                pauseContinueButton.Content = "Continue";
                stepButton.Content = activeScenarioPage.IsStepping ? "Next" : "Step";
            }
            else
            {
                activeScenarioPage.Continue();
                pauseContinueButton.Content = "Pause";
                stepButton.Content = "Step";
            }
        }

        private void m_timeStepScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (activeScenarioPage is not null)
                activeScenarioPage.TimeStepScale = Scalar.ComputeTimeStepScale(e.NewValue, m_timeStepScaleSlider.Minimum, m_timeStepScaleSlider.Maximum, activeScenarioPage.MinTimeStepScale, activeScenarioPage.MaxTimeStepScale);
        }

        private void FrameContainer_Navigated(object sender, NavigationEventArgs e)
        {
            activeScenarioPage = e.Content as BaseScenarioPage;
            if (activeScenarioPage is not null)
            {
                activeScenarioPage.TimeStepScale = Scalar.ComputeTimeStepScale(m_timeStepScaleSlider.Value, m_timeStepScaleSlider.Minimum, m_timeStepScaleSlider.Maximum, activeScenarioPage.MinTimeStepScale, activeScenarioPage.MaxTimeStepScale);

                activeScenarioPage.SolverKind = (SolverType)m_solverTypeCombobox.SelectedIndex;

                int numIterations = m_iterationsCombobox.SelectedIndex + 1;
                if (numIterations == 4)
                    numIterations = 10;
                activeScenarioPage.NumIterations = numIterations;

                activeScenarioPage.IsShownMotionBoxes = renderMotionBoundsCheckbox.IsChecked ?? false;
                activeScenarioPage.IsShownContacts = renderContactsCheckbox.IsChecked ?? false;
                activeScenarioPage.IsShownOrigins = renderOriginsCheckbox.IsChecked ?? false;

                activeScenarioPage.IsPaused = string.Equals("Continue", pauseContinueButton.Content);
                activeScenarioPage.IsStepping = activeScenarioPage.IsPaused && string.Equals("Next", stepButton.Content);
            }
        }

        private void m_solverTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activeScenarioPage is not null)
                activeScenarioPage.SolverKind = (SolverType)m_solverTypeCombobox.SelectedIndex;
        }

        private void m_iterationsCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activeScenarioPage is not null)
            {
                int numIterations = m_iterationsCombobox.SelectedIndex + 1;
                if (numIterations == 4)
                    numIterations = 10;
                activeScenarioPage.NumIterations = numIterations;
            }
        }

        private void renderMotionBoundsCheckbox_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            if (activeScenarioPage is not null)
                activeScenarioPage.IsShownMotionBoxes = renderMotionBoundsCheckbox.IsChecked ?? false;
        }

        private void renderContactsCheckbox_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            if (activeScenarioPage is not null)
                activeScenarioPage.IsShownContacts = renderContactsCheckbox.IsChecked ?? false;
        }

        private void renderOriginsCheckbox_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            if (activeScenarioPage is not null)
                activeScenarioPage.IsShownOrigins = renderOriginsCheckbox.IsChecked ?? false;
        }
    }
}