   M S B A M L   `   `   `  ���� A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Client.ViewsClient.Views  &     Client.Views.AuthorizationWindow   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:Client.Views       � Title$  Connection settings�� � SizeToContent$ WidthAndHeight=� � WindowStartupLocation$ CenterOwner=�.��� $��10q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��Enter IP Address:�� -   $��addressTextBox��$	��Top=�$��1��$	��250��.������  �� Path$ Address� �� ValidationRules 5  /System.Windows.Controls.DataErrorValidationRule  .���� $��2��$��0,18,0,0q�Enter port:�� -   $��portTextBox��$	��Top=�$��3��$	��250��.������ $
 Port�   .���� $��4��$��0,18,0,0q�Enter login:�� -   $��loginTextBox��$	��Top=�$��5��$	��250��.������ $ Login�   .��� $��6��$��Right=�$��0,20,0,0q�.+��� -    �� 	IsDefault$
 True��$��60��$��0,0,10,0q�.��OK��  �� IsCancel$
 True��$��60��.��Cancel