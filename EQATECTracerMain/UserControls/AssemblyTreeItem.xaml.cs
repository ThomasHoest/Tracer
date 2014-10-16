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
using EQATEC.Tracer.Tools;

namespace EQATEC.Tracer.UserControls
{
  /// <summary>
  /// Interaction logic for TreeItem.xaml
  /// </summary>
  public partial class AssemblyTreeItem : UserControl
  {
    CheckBox mChkBox;
    StackPanel mPanel;
    TextBlock mText;
    ContextMenu mMenu = null;
    ILType mILItem;
    Image mImg;
    
    public AssemblyTreeItem(ILType type, Image img)
    {
      InitializeComponent();
      mILItem = type;
      mPanel = new StackPanel();
      
      //Todo: Use binding
      mChkBox = new CheckBox();
      mChkBox.Margin = new System.Windows.Thickness(3, 1, 3, 1);
      Binding enabledBinding = new Binding("Enabled") { Source = type, Mode = BindingMode.TwoWay };
      mChkBox.SetBinding(CheckBox.IsCheckedProperty, enabledBinding);
      //mChkBox.Checked += new RoutedEventHandler(mChkBox_Checked);
      //mChkBox.Unchecked += new RoutedEventHandler(mChkBox_Unchecked);      
      //mChkBox.IsChecked = type.Enabled;

      mText = new TextBlock();
      mText.VerticalAlignment = System.Windows.VerticalAlignment.Center;
      mText.Margin = new System.Windows.Thickness(2, 0, 0, 0);
      mText.Text = type.AssemblyName;

      if (!(type is AssemblyContainer))
        throw new InvalidOperationException("Assembly item type not recognized");

      mPanel.Orientation = Orientation.Horizontal;
      //mPanel.Children.Add(mEllipse);
            
      mImg = img;

      if (mImg != null && type.Enabled == false)
      {
        mImg.Opacity = 0.5;
        mImg.Height = 15;
      }

      mPanel.Children.Add(img);
      mPanel.Children.Add(mChkBox);
      mPanel.Children.Add(mText);
      if (mMenu != null)
        mPanel.ContextMenu = mMenu;

      m_gBase.Children.Add(mPanel);
    }
       

    void mChkBox_Unchecked(object sender, RoutedEventArgs e)
    {
      if(mImg != null)
        mImg.Opacity = 0.5;
      mILItem.SetEnabledState(false);      
    }

    void mChkBox_Checked(object sender, RoutedEventArgs e)
    {
      if (mImg != null)
        mImg.Opacity = 1.0;
      mILItem.SetEnabledState(true);
    }
        
    private void BuildAssemblyItem(AssemblyContainer assem)
    {
      //Add mEllipse
      //mEllipse.Fill = Brushes.Red;
      //mChkBox.IsThreeState = true;
    }      
    
  }
}