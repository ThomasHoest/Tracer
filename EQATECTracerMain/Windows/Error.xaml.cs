﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EQATEC.Tracer.Windows
{
  /// <summary>
  /// Interaction logic for Error.xaml
  /// </summary>
  public partial class Error : Window
  {
    public Error()
    {
      InitializeComponent();
    }

    private void button_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}
