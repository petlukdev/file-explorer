using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileExplorer.MVVM.Behaviours
{
    public static class TreeBehaviour
    {
        public static readonly DependencyProperty ExpandingBehaviourProperty =
            DependencyProperty.RegisterAttached("ExpandingBehaviour", typeof(ICommand), typeof(TreeBehaviour),
                new PropertyMetadata(OnExpandingBehaviourChanged));

        public static void SetExpandingBehaviour(DependencyObject o, ICommand value)
        {
            o.SetValue(ExpandingBehaviourProperty, value);
        }
        public static ICommand GetExpandingBehaviour(DependencyObject o)
        {
            return (ICommand)o.GetValue(ExpandingBehaviourProperty);
        }

        public static readonly DependencyProperty SelectedBehaviourProperty =
            DependencyProperty.RegisterAttached("SelectedBehaviour", typeof(ICommand), typeof(TreeBehaviour),
                new PropertyMetadata(OnSelectedBehaviourChanged));

        public static void SetSelectedBehaviour(DependencyObject o, ICommand value)
        {
            o.SetValue(SelectedBehaviourProperty, value);
        }
        public static ICommand GetSelectedBehaviour(DependencyObject o)
        {
            return (ICommand)o.GetValue(SelectedBehaviourProperty);
        }

        private static void OnExpandingBehaviourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem tvi = d as TreeViewItem;
            if (tvi != null)
            {
                ICommand ic = e.NewValue as ICommand;
                if (ic != null)
                {
                    tvi.Expanded += (s, a) =>
                    {
                        if (ic.CanExecute(a))
                        {
                            ic.Execute(a);
                        }
                        a.Handled = true;
                    };
                }
            }
        }

        private static void OnSelectedBehaviourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem tvi = d as TreeViewItem;
            if (tvi != null)
            {
                ICommand ic = e.NewValue as ICommand;
                if (ic != null)
                {
                    tvi.Selected += (s, a) =>
                    {
                        if (ic.CanExecute(a))
                        {
                            ic.Execute(a);

                        }
                        a.Handled = true;
                    };
                }
            }
        }
    }
}
