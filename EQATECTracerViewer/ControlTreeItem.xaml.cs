using System;
using System.Collections.Generic;
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

using EQATEC.Tracer.Utilities;

namespace EQATEC.Tracer.TracerViewer
{
  /// <summary>
  /// Interaction logic for TreeItem.xaml
  /// </summary>
  public partial class ControlTreeItem : UserControl
  {
    CheckBox mChkBox;
    StackPanel mPanel;
    TextBlock mText;
    Ellipse mEllipse;
    ContextMenu mMenu = null;
    ILType mILItem;
    bool mUpdating = false;

    public ControlTreeItem(ILType type)
    {
      InitializeComponent();
      mILItem = type;
      mPanel = new StackPanel();

      mEllipse = new Ellipse();
      mEllipse.Width = 7;
      mEllipse.Width = 7;
      mEllipse.Margin = new System.Windows.Thickness(1, 3, 1, 3);
      
      mChkBox = new CheckBox();
      mChkBox.Margin = new System.Windows.Thickness(3, 1, 3, 1);
      //mChkBox.Click += new System.Windows.RoutedEventHandler(type.ChkBoxClick);
      mChkBox.Click += new RoutedEventHandler(mChkBox_Click);
      mChkBox.Checked += new RoutedEventHandler(mChkBox_Checked);
      mChkBox.Unchecked += new RoutedEventHandler(mChkBox_Unchecked);      
      mChkBox.IsChecked = type.GetState();

      mText = new TextBlock();
      mText.VerticalAlignment = System.Windows.VerticalAlignment.Center;
      mText.Margin = new System.Windows.Thickness(2, 0, 0, 0);
      mText.Text = type.Name;

      if (type is MemberContainer)
      {
        MemberContainer member = type as MemberContainer;
        BuildFunctionItem(member);
      }
      else if (type is TypeContainer)
      {
        TypeContainer typecon = type as TypeContainer;
        BuildTypeItem(typecon);
      }
      else if (type is ModuleContainer)
      {
        ModuleContainer mod = type as ModuleContainer;
        BuildModuleItem(mod);
      }
      else if (type is NamespaceContainer)
      {
        NamespaceContainer nam = type as NamespaceContainer;
        BuildNamespaceItem(nam);
      }  
      else
        throw new InvalidOperationException("Tree item type not recognized");

      mPanel.Orientation = Orientation.Horizontal;
      mPanel.Children.Add(mEllipse);
      mPanel.Children.Add(mChkBox);
      mPanel.Children.Add(mText);
      if (mMenu != null)
        mPanel.ContextMenu = mMenu;

      type.OnMemberChanged += new ILType.MemberChangedHandler(type_OnMemberChanged);

      m_gBase.Children.Add(mPanel);
    }

    void mChkBox_Click(object sender, RoutedEventArgs e)
    {
      mUpdating = true;
      mILItem.ChkBoxClick(sender, e);
      mUpdating = false;
    }

    void mChkBox_Unchecked(object sender, RoutedEventArgs e)
    {
      if (mMenu != null)
      {
        MenuItem mi = mMenu.Items[0] as MenuItem;
        mi.IsChecked = false;
      }
    }

    void mChkBox_Checked(object sender, RoutedEventArgs e)
    {
      if (mMenu != null)
      {
        MenuItem mi = mMenu.Items[0] as MenuItem;
        mi.IsChecked = true;
      }
    }

    void type_OnMemberChanged()
    {      
      if(!mUpdating)
        mChkBox.IsChecked = mILItem.GetState();

      if (mILItem.Parent != null)
        mILItem.Parent.SignalMemberChanged();

      if (mMenu != null)
      {
        MenuItem mi = mMenu.Items[0] as MenuItem;
        mi.IsChecked = mILItem.Enabled;
      }      
    }

    private void BuildModuleItem(ModuleContainer mod)
    {
      //Add mEllipse
      mEllipse.Fill = Brushes.Red;
      //mChkBox.IsThreeState = true;
    }

    private void BuildTypeItem(TypeContainer type)
    {
      //Add mEllipse
      mEllipse.Fill = Brushes.Green;
      //mChkBox.IsThreeState = true;
    }

    private void BuildNamespaceItem(NamespaceContainer nam)
    {
      mEllipse.Fill = Brushes.White;
    }

    private void BuildFunctionItem(MemberContainer member)
    {
      //Add mEllipse
      mEllipse.Fill = Brushes.Orange;
      mText.Text = member.NameWithParams + " : " + member.ReturnType;
      
      //Add menu
      mMenu = new ContextMenu();

      MenuItem item = new MenuItem();
      item.Header = "Enabled";
      item.IsCheckable = true;
      item.IsChecked = member.Enabled;
      item.Checked += new RoutedEventHandler(item_Checked);
      item.Unchecked += new RoutedEventHandler(item_Unchecked);
      mMenu.Items.Add(item);

      int prevCallers = 0;
      for (int i = 1; i <= 5; i++)
      {
        int temp = member.GetCallees(i);
        if (temp > 0 && temp > prevCallers)
        {
          if (i == 1)
          {
            Separator sep = new Separator();
            mMenu.Items.Add(sep);
          }

          item = new MenuItem();
          item.Header = "Enable for callers. " + i + " Level (" + temp + " functions)";
          item.Tag = new MenuLevelEnabler(member.ID, i);
          item.Click += new System.Windows.RoutedEventHandler(member.MenuClick);
          mMenu.Items.Add(item);
          prevCallers = temp;
        }
        else
          break;
      }
    }

    void item_Unchecked(object sender, RoutedEventArgs e)
    {
      mChkBox.IsChecked = false;
    }

    void item_Checked(object sender, RoutedEventArgs e)
    {
      mChkBox.IsChecked = true;
    }
  }
}
