   M S B A M L   `   `   `  ����@  <nsync, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:nsync.Propertiesnsync.Properties  5          nsync.MainWindow   5   	   W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      .constclr-namespace:nsync.Properties    5      8x,http://schemas.microsoft.com/winfx/2006/xaml    5      -   5        $��
WindowMain��5   �     � Title$  nsync��6   $	��340��6   $	��518��6     � 
ResizeMode$ NoResize=�6,    � AllowsTransparency .  � WindowStyle$
 None=�6\    � WindowStartupLocation$ CenterScreen=�6o   $	��518��6�   $	��340��6�    � Icon$* $/nsync;component/Resources/nsync.ico��6�   ��5      �� 5   
    �� ImageSource$ Resources/background.png��6    � 	Resources 5         slideLeftToRight �� TargetProperty	 g� AccelerationRatio
 g� DecelerationRatio �� 
TargetName g� Duration W� From	 W� To  slideRightToLeft��%F  &	        &	     �� 5   
   $, &RenderTransform.(TranslateTransform.X)�5      $	 .4Q�5      $
 .4Q�5      .��5      W� $ viewer��6   $ 0:0:0.6D�6=   $	 518X�6P   $ 0X�6[   W� 5      $ bordervisual��6   $ 0:0:0.6D�6C   $ 0X�6V   $
 -518X�6_   5      �� 5   
   $, &RenderTransform.(TranslateTransform.X)�5      $	 .4Q�5      $
 .4Q�5      .��5      W� $ viewer��6   $ 0:0:0.6D�6=   $
 -518X�6P   $ 0X�6\   W� 5      $ bordervisual��6   $ 0:0:0.6D�6C   $ 0X�6V   $	 518X�6_   5      5      .��5   
   � .+�5   
   �� -   5         ��Name$ TitleBar��5   G   $��38��6Y   $	��Top=�6e   �� 5    
   -   5        $��ButtonClose��5           nsync.Properties.Resources   closeToolTip � ToolTip# Z 6$     ButtonCloseTemplate#��[ 6V   $��25��6�   $��23��6�   $	��Top=�6�   $��Right=�6�   $
��5,15q�6�   6�   �� 5"   
   -   5        $��ButtonPageSettings��5"        ButtonPageTemplate#��[ 6+   $��15��6Z   $��15��6e   $	��Top=�6q   $
��Left=�6�   $��
240,35,0,0q�6�   6)  �� 5#   
   -   5        $��ButtonPageHome��5#      #��[ 6'   $��15��6V   $��15��6a   $	��Top=�6m   $
��Left=�6�   $��
250,35,0,0q�6�   {�.  6/  �� 5$   
   -   5        $��ButtonPageTrackBack��5$      #��[ 6,   $��15��6[   $��15��6f   $	��Top=�6r   $
��Left=�6�   $��
260,35,0,0q�6�   6,  �� 5%   
   -   5        $��ButtonMinimise��5%         minimiseToolTip# Z 6'     ButtonMinimiseTemplate#��[ 6\   $��22��6�   $��10��6�   $	��Top=�6�   $��Right=�6�   $��	0,26,25,0q�6�   6  �� 5(       �� Orientation$ 
Horizontal=�6   $	��Top=�62   $��Center=�6J   $
��0,40q�6g   .+�5)      �� -   5         ��Name$ viewList��5)      $��20��6,   $	��300��68    �� SelectedIndex$ 1��6D    �� DisplayMemberPath$ @Title��5+      $y�Hidden=�5,   B   ��5*      ��  	 views �� Source# [  �� XPath$ 
Views/View����5-      �� 5.      .R�5/      �� $ 
Horizontal=�6)   50      52      53      � 55      $��Center=�6   $	��Top=�60   $��0,53,0,0q�6H   .+�57      �� -	   5         ��Name$ bordervisual��57      ._�59      �� -
   5         ��Name$ rectanglevisual��59       z�5:      T� 5;      5=      �� 5?      -   5         ��Name$ viewer��5?       � DataContext 6/   ��  �� Path$ SelectedItem� �� ElementName$ viewList����5@      �� $
 Page����5B      �� 5C      .R�5D       �   �Name$ frame��6$      � Source  63   �� $ @Source��5E      z�5G      T� 5H      5J      5K      �� 5M   
   -   5        $��ButtonSideTabLeft��5M        ButtonSideTabLeftTemplate#��[ 6*   $��16��6`   $��56��6k   $	��Top=�6�   $
��Left=�6�   $��0,140q�6�   $y�Hidden=�6�   6�   �� 5N   
   -   5        $��ButtonSideTabRight��5N        ButtonSideTabRightTemplate#��[ 6+   $��16��6b   $��56��6m   $	��Top=�6�   $��Right=�6�   $��0,140q�6�   $y�Hidden=�6�   6�   �� 5O   
   -   5        $��PageToolTip��5O      $	��100��6$   $��20��60   $��Center=�6<   $	��Top=�6Y   $��0, 57q�6q   
���   �! J� Opacity$	! 0.8Q�6�   $y�Hidden=�6�   " �� CornerRadius$" 5��6�     DarkBorderBrush#��[ 6�   $��1q�6�   ._�5P      � .+�5Q      �� -   5        $��PageToolTipText��5Q      
�������$��Center=�6F   $��Center=�6a   $��10(�6~   6�   5R      5S      5T      5V      