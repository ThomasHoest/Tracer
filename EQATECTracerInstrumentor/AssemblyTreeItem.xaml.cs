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

namespace EQATEC.Tracer.TracerInstrumentor
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
    
    public AssemblyTreeItem(ILType type, Image img)
    {
      InitializeComponent();
      mILItem = type;
      mPanel = new StackPanel();

      
      
      mChkBox = new CheckBox();
      mChkBox.Margin = new System.Windows.Thickness(3, 1, 3, 1);
      mChkBox.Checked += new RoutedEventHandler(mChkBox_Checked);
      mChkBox.Unchecked += new RoutedEventHandler(mChkBox_Unchecked);      
      mChkBox.IsChecked = type.Enabled;

      mText = new TextBlock();
      mText.VerticalAlignment = System.Windows.VerticalAlignment.Center;
      mText.Margin = new System.Windows.Thickness(2, 0, 0, 0);
      mText.Text = type.FullName;

      if (!(type is AssemblyContainer))
        throw new InvalidOperationException("Tree item type not recognized");

      mPanel.Orientation = Orientation.Horizontal;
      //mPanel.Children.Add(mEllipse);

      img.Height = 15;      
      mPanel.Children.Add(img);
      mPanel.Children.Add(mChkBox);
      mPanel.Children.Add(mText);
      if (mMenu != null)
        mPanel.ContextMenu = mMenu;

      m_gBase.Children.Add(mPanel);
    }
       

    void mChkBox_Unchecked(object sender, RoutedEventArgs e)
    {
      mILItem.Enabled = false;
    }

    void mChkBox_Checked(object sender, RoutedEventArgs e)
    {
      mILItem.Enabled = true;
    }
        
    private void BuildAssemblyItem(AssemblyContainer assem)
    {
      //Add mEllipse
      //mEllipse.Fill = Brushes.Red;
      //mChkBox.IsThreeState = true;
    }      
    
  }
}
