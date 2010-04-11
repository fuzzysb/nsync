﻿#pragma checksum "..\..\ExcludeWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B38A9AA8D9FD80F5997137B56B1A6751"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using nsync.Properties;


namespace nsync {
    
    
    /// <summary>
    /// ExcludeWindow
    /// </summary>
    public partial class ExcludeWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 5 "..\..\ExcludeWindow.xaml"
        internal nsync.ExcludeWindow WindowExclude;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.TextBlock TitleBar;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.Button ButtonClose;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.Button ButtonNext;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.Button ButtonClear;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.StackPanel BoxLeftPath;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.Label LabelLeftPath;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.StackPanel BoxRightPath;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.Label LabelRightPath;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.ComboBox ComboBoxFileType;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.Label LabelAddFileTypes;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.StackPanel BoxExclude;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.ListBox ListBoxExclude;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.TextBlock HintText;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.Image HintIcon;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\ExcludeWindow.xaml"
        internal System.Windows.Controls.Label LabelStatus;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/nsync;component/excludewindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ExcludeWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.WindowExclude = ((nsync.ExcludeWindow)(target));
            
            #line 5 "..\..\ExcludeWindow.xaml"
            this.WindowExclude.Loaded += new System.Windows.RoutedEventHandler(this.WindowExclude_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.TitleBar = ((System.Windows.Controls.TextBlock)(target));
            
            #line 11 "..\..\ExcludeWindow.xaml"
            this.TitleBar.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.titleBar_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ButtonClose = ((System.Windows.Controls.Button)(target));
            
            #line 12 "..\..\ExcludeWindow.xaml"
            this.ButtonClose.Click += new System.Windows.RoutedEventHandler(this.ButtonClose_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ButtonNext = ((System.Windows.Controls.Button)(target));
            
            #line 13 "..\..\ExcludeWindow.xaml"
            this.ButtonNext.Click += new System.Windows.RoutedEventHandler(this.ButtonNext_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ButtonClear = ((System.Windows.Controls.Button)(target));
            
            #line 14 "..\..\ExcludeWindow.xaml"
            this.ButtonClear.Click += new System.Windows.RoutedEventHandler(this.ButtonClear_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.BoxLeftPath = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 7:
            this.LabelLeftPath = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.BoxRightPath = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 9:
            this.LabelRightPath = ((System.Windows.Controls.Label)(target));
            return;
            case 10:
            this.ComboBoxFileType = ((System.Windows.Controls.ComboBox)(target));
            
            #line 21 "..\..\ExcludeWindow.xaml"
            this.ComboBoxFileType.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ComboBoxFileType_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 21 "..\..\ExcludeWindow.xaml"
            this.ComboBoxFileType.DropDownOpened += new System.EventHandler(this.ComboBoxFileType_DropDownOpened);
            
            #line default
            #line hidden
            return;
            case 11:
            this.LabelAddFileTypes = ((System.Windows.Controls.Label)(target));
            return;
            case 12:
            this.BoxExclude = ((System.Windows.Controls.StackPanel)(target));
            
            #line 25 "..\..\ExcludeWindow.xaml"
            this.BoxExclude.DragEnter += new System.Windows.DragEventHandler(this.BoxExclude_DragEnter);
            
            #line default
            #line hidden
            
            #line 25 "..\..\ExcludeWindow.xaml"
            this.BoxExclude.DragLeave += new System.Windows.DragEventHandler(this.BoxExclude_DragLeave);
            
            #line default
            #line hidden
            
            #line 25 "..\..\ExcludeWindow.xaml"
            this.BoxExclude.AddHandler(System.Windows.DragDrop.DropEvent, new System.Windows.DragEventHandler(this.BoxExclude_Drop));
            
            #line default
            #line hidden
            return;
            case 13:
            this.ListBoxExclude = ((System.Windows.Controls.ListBox)(target));
            
            #line 26 "..\..\ExcludeWindow.xaml"
            this.ListBoxExclude.MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.ListBoxExclude_MouseUp);
            
            #line default
            #line hidden
            return;
            case 14:
            this.HintText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 15:
            this.HintIcon = ((System.Windows.Controls.Image)(target));
            return;
            case 16:
            this.LabelStatus = ((System.Windows.Controls.Label)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
