   M S B A M L   `   `   `  ����A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Client.ViewsClient.Views  5   	   &     Client.Views.AuthorizationWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:Client.Views     5   	     � Title$  Connection settings��5   	    � SizeToContent$ WidthAndHeight=�6%    � WindowStartupLocation$ CenterOwner=�6D   .��5	      � $��10q�6    � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   Enter IP Address:64   �� 5   
   -   5        $��addressTextBox��5      $	��Top=�6(   $��1��6@   $	��250��6M   .��6\   ��5      �� 5       �� Path$ Address�6    �� ValidationRules 5      5  /System.Windows.Controls.DataErrorValidationRule  5      5      .��5      5      �� 5    
   $��2��6   $��0,18,0,0q�6!   Enter port:6@   �� 5!   
   -   5        $��portTextBox��5!      $	��Top=�6%   $��3��6=   $	��250��6J   .��6Y   ��5"      �� 5#      $
 Port�6    5$        5%      5'      .��5(      5)      �� 5+   
   $��4��6   $��0,18,0,0q�6!   Enter login:6A   �� 5,   
   -   5        $��loginTextBox��5,      $	��Top=�6&   $��5��6>   $	��250��6K   .��6Z   ��5-      �� 5.      $ Login�6    5/        50      52      .��53      54      � 56   
   $��6��6   $��Right=�6!   $��0,20,0,0q�6=   .+�57      �� -   5         �� 	IsDefault$
 True��57      $��60��6;   $��0,0,10,0q�6I   .��6[   OK6_   �� 58       �� IsCancel$
 True��6   $��60��6%   .��63   Cancel6;   59      5;      5<      