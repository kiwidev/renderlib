using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace RenderLib
{
    public static class Render
    {
        // Using a DependencyProperty as the backing store for UsePhases.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsePhasesProperty =
            DependencyProperty.RegisterAttached("UsePhases", typeof(bool), typeof(Render), new PropertyMetadata(false, OnUsePhasesPropertyChanged));

        // Using a DependencyProperty as the backing store for UsePhases.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PhaseProperty =
            DependencyProperty.RegisterAttached("Phase", typeof(int), typeof(Render), new PropertyMetadata(0));

        public static void SetUsePhases(DependencyObject owner, bool value)
        {
            owner.SetValue(UsePhasesProperty, value);
        }

        public static bool GetUsePhases(DependencyObject owner)
        {
            return (bool)owner.GetValue(UsePhasesProperty);
        }

        public static void SetPhase(DependencyObject owner, int value)
        {
            owner.SetValue(PhaseProperty, value);
        }

        public static int GetPhase(DependencyObject owner)
        {
            return (int)owner.GetValue(PhaseProperty);
        }


        private static void OnUsePhasesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListViewBase gridView = d as ListViewBase;
            if (gridView == null)
                return;

            if ((bool) e.NewValue)
            {
                gridView.ContainerContentChanging += ListViewContainerContentChanging;
            }
            else
            {
                gridView.ContainerContentChanging -= ListViewContainerContentChanging;
            }
        }

        private static void ListViewContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            // We need to leave this as false, otherwise data binding will not work
            args.Handled = false;

            bool hasMore = PerformNextPhase(args);
            if (hasMore)
            {
                args.RegisterUpdateCallback(ListViewContainerContentChanging);
            }
        }

        private static bool PerformNextPhase(ContainerContentChangingEventArgs args)
        {
            args.Handled = false;
            var phaseIndex = (int)args.Phase;

            var controlChangingState = args.ItemContainer.Tag as ControlChangingState;
            if (controlChangingState == null)
            {
                controlChangingState = new ControlChangingState();
                args.ItemContainer.Tag = controlChangingState;
            }
            if (phaseIndex == 0)
            {
                controlChangingState.Initialise(args);
            }
            else
            {
                var itemsToShow = controlChangingState.GetControlsInPhase(phaseIndex-1);

                foreach (var item in itemsToShow)
                {
                    controlChangingState.ShowItem(args, item);
                }
            }

            if (phaseIndex <= controlChangingState.MaxPhaseIndex+1)
            {
                return true;
            }
            return false;
        }

        private class ControlChangingState
        {
            private List<ChangingControl> _changingControls = new List<ChangingControl>();
            private int _maxPhaseIndex = 0;

            public int MaxPhaseIndex
            {
                get
                {
                    return _maxPhaseIndex;
                }
            }

            private void AddChangingControl(ChangingControl changingControl)
            {
                _maxPhaseIndex = Math.Max(_maxPhaseIndex, changingControl.PhaseIndex);
                _changingControls.Add(changingControl);
            }

            public IList<FrameworkElement> GetControlsInPhase(int phase)
            {
                return _changingControls.Where(x => x.PhaseIndex == phase).Select(x => x.Control).ToArray();
            }

            internal void ShowItem(ContainerContentChangingEventArgs args, FrameworkElement item)
            {
                item.SetValue(FrameworkElement.DataContextProperty, DependencyProperty.UnsetValue);
                item.Opacity = 1;
            }

            internal void HideItem(FrameworkElement item)
            {
                item.SetValue(FrameworkElement.DataContextProperty, null);
                item.Opacity = 0;
            }

            internal void Initialise(ContainerContentChangingEventArgs args)
            {
                AddControlsToList(args.ItemContainer, _changingControls);
            }

            private void AddControlsToList(DependencyObject parent, IList<ChangingControl> changingControls)
            {
                if (parent == null)
                    return;

                FrameworkElement frameworkParent = parent as FrameworkElement;
                if (frameworkParent != null)
                {
                    var phase = Render.GetPhase(frameworkParent);
                    if (phase > 0)
                    {
                        AddChangingControl(new ChangingControl
                        {
                            Control = frameworkParent,
                            PhaseIndex = phase
                        });

                        HideItem(frameworkParent);
                    }
                }

                var childCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    AddControlsToList(child, changingControls);
                }
            }
        }

        private struct ChangingControl
        {
            public int PhaseIndex;
            public FrameworkElement Control;
        }
    }

}
