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
using EQATEC.Tracer.Windows;

namespace EQATEC.Tracer.UserControls
{
  /// <summary>
  /// Interaction logic for TreeItem.xaml
  /// </summary>
  public partial class ControlTreeItem : UserControl
  {
    CheckBox mChkBox;
    StackPanel mPanel;
    TextBlock mText;
    Image mImage;
    ContextMenu mMenu = null;
    ILType mILItem;
    bool mUpdating = false;

    public ControlTreeItem(ILType type, RoutedEventHandler checkBoxHandler)
    {
      InitializeComponent();
      mILItem = type;
      mPanel = new StackPanel();

      mChkBox = new CheckBox();
      mChkBox.Margin = new System.Windows.Thickness(2, 1, 2, 1);
      mChkBox.Tag = type;
      //mChkBox.Click += checkBoxHandler;
      //mChkBox.IsChecked = type.GetState();
      
      Binding enabledBinding = new Binding("Enabled") {Mode = BindingMode.TwoWay, Source = type};
      mChkBox.SetBinding(CheckBox.IsCheckedProperty, enabledBinding);
      
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
      else if (type is ApplicationContainer)
      {
        ApplicationContainer app = type as ApplicationContainer;
        BuildAppItem(app);
      }
      else
        throw new InvalidOperationException("Tree item type not recognized");

      
      mPanel.Orientation = Orientation.Horizontal;
      mImage.Width = 16;      
      mPanel.Children.Add(mImage);
      mPanel.Children.Add(mChkBox);
      mPanel.Children.Add(mText);
      if (mMenu != null)
        mPanel.ContextMenu = mMenu;

      m_gBase.Children.Add(mPanel);
    }
 
    private void BuildModuleItem(ModuleContainer mod)
    {
      if (mod.Types.Count == 0)
      {
        mChkBox.IsChecked = false;
        mChkBox.IsEnabled = false;
      }

      if(System.IO.Path.GetExtension(mod.FullName).ToLower() == ".exe")
        mImage = ImageCache.GetImage("../Resources/exefile.png");
      else
        mImage = ImageCache.GetImage("../Resources/dllfile.png");
    }

    private void BuildTypeItem(TypeContainer type)
    {
      if (type.Members.Count == 0)
      {
        mChkBox.IsChecked = false;
        mChkBox.IsEnabled = false;
      }

      mImage = ImageCache.GetImage("../Resources/Class.png");
    }

    private void BuildNamespaceItem(NamespaceContainer nam)
    {
      if (nam.Types.Count == 0)
      {
        mChkBox.IsChecked = false;
        mChkBox.IsEnabled = false;
      }

      mImage = ImageCache.GetImage("../Resources/Namespace.png");
    }

    private void BuildAppItem(ApplicationContainer app)
    {
      mImage = ImageCache.GetImage("../Resources/Plug.png");
    }

    private void BuildFunctionItem(MemberContainer member)
    {
      //Add mEllipse
      //mImage = UIHelpers.GetPngFromRessource("../Resources/Function.png");
      
      mImage = ImageCache.GetImage("../Resources/Function.png");
      mText.Text = member.NameWithParams + " : " + member.ReturnType;
      
      //Add menu
      //mMenu = UIUtils.BuildMemberMenu(member, null);
      //member.ContextMenu = (object)mMenu;      
    }
  }
}